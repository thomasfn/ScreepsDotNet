import 'fastestsmallesttextencoderdecoder';
import base64Decode from 'fast-base64-decode'
import { decode as base32768Decode } from 'base32768';
import * as fflate from 'fflate';

import { ImportTable, Interop } from './interop.js';
import { ScreepsDotNetExports } from './common.js';
import { WasmMemoryManager, WasmMemoryView } from './memory.js';
import BaseBindings from './bindings/base.js';
import { getBindings } from './bindings/index.js';

const utf8Decoder = new TextDecoder();

class Stdio {
    private readonly _outFunc: (text: string) => void;

    private buffer?: string;

    constructor(outFunc: (text: string) => void) {
        this._outFunc = outFunc;
    }

    public write(view: WasmMemoryView, buf: number, buf_len: number): void {
        const buffer = view.u8.slice(buf, buf + buf_len);
        this.addTextToBuffer(utf8Decoder.decode(buffer));
    }

    private addTextToBuffer(text: string): void {
        if (!this.buffer) {
            this.buffer = text;
        } else {
            this.buffer += text;
        }
        let newlineIdx: number;
        while ((newlineIdx = this.buffer.indexOf('\n')) >= 0) {
            const line = this.buffer.substring(0, newlineIdx).trim();
            this._outFunc(line);
            this.buffer = this.buffer?.substring(newlineIdx + 1);
        }
    }
}

type ScreepsDotNetWasmInstance = WebAssembly.Instance & { exports: ScreepsDotNetExports };

const JSTYPE_TO_ENUM: Readonly<Record<'undefined' | 'string' | 'number' | 'bigint' | 'boolean' | 'object' | 'function' | 'symbol', number>> = {
    undefined: 0,
    string: 1,
    number: 2,
    bigint: 3,
    boolean: 4,
    object: 5,
    function: 6,
    symbol: 7,
};

export function decompressWasm(compressedBytes: Uint8Array, originalSize: number): Uint8Array {
    const decompressedBytes = new Uint8Array(originalSize);
    return fflate.inflateSync(compressedBytes, { out: decompressedBytes });
}

export function decodeWasm(encodedWasm: string, originalSize: number, encoding: 'b64'|'b32768'): Uint8Array {
    let bytes: Uint8Array;
    if (encoding == 'b64') {
        bytes = new Uint8Array(originalSize);
        base64Decode(encodedWasm, bytes);
    } else {
        bytes = base32768Decode(encodedWasm);
    }
    return bytes;
}

const EMPTY_ARR: unknown[] = [];

type Env = 'world'|'arena'|'test';

export class Bootloader {
    private readonly _env: Env;
    private readonly _pendingLogs: string[] = [];
    private readonly _deferLogsToTick: boolean;
    private readonly _profileFn: () => number;

    private readonly _stdout: Stdio;
    private readonly _stderr: Stdio;

    private readonly _interop: Interop;
    private readonly _bindings?: BaseBindings;
    private readonly _systemImport: Record<string, (...args: any[]) => unknown>;

    private _wasmModule?: WebAssembly.Module;
    private _wasmInstance?: ScreepsDotNetWasmInstance;
    private _memoryManager?: WasmMemoryManager;
    private _compiled: boolean = false;
    private _started: boolean = false;

    private _inTick: boolean = false;
    private _profilingEnabled: boolean = false;

    public get compiled() { return this._compiled; }
    public get started() { return this._started; }

    public get profilingEnabled() { return this._profilingEnabled; }
    public set profilingEnabled(value) { this._profilingEnabled = value; }

    public get exports() { return this._wasmInstance!.exports; }

    constructor(env: Env, profileFn: () => number) {
        this._env = env;
        this._deferLogsToTick = env === 'arena';
        this._profileFn = profileFn;

        this._stdout = new Stdio(this.log.bind(this));
        this._stderr = new Stdio(this.log.bind(this));

        this._interop = new Interop(profileFn);

        this.setImports('__object', {
            hasProperty: (obj: Record<string | number | symbol, unknown>, key: string) => key in obj,
            getTypeOfProperty: (obj: Record<string | number | symbol, unknown>, key: string) => JSTYPE_TO_ENUM[typeof obj[key]],
            getKeys: (obj: Record<string | number | symbol, unknown>) => (obj ? Object.keys(obj) : null) ?? EMPTY_ARR,
            getProperty: (obj: Record<string | number | symbol, unknown>, key: string | number | symbol) => obj[key],
            setProperty: (obj: Record<string | number | symbol, unknown>, key: string | number | symbol, value: unknown) => obj[key] = value,
            deleteProperty: (obj: Record<string | number | symbol, unknown>, key: string) => delete obj[key],
            create: (proto: object) => Object.create(proto),
        });

        this._bindings = getBindings(env, this.log.bind(this), this._interop);
        if (this._bindings) {
            for (const moduleName in this._bindings.imports) {
                this.setImports(moduleName, this._bindings.imports[moduleName]);
            }
        }

        this._systemImport = {
            ["get-time"]: this.sys_get_time.bind(this),
            ["get-random"]: this.sys_get_random.bind(this),
            ["write-stderr"]: this.sys_write_stderr.bind(this),
            ["write-stdout"]: this.sys_write_stdout.bind(this),
        };
    }

    private sys_get_time(time_ptr: number): void {
        const dataView = this._memoryManager!.view.dataView;
        dataView.setBigUint64(time_ptr, BigInt(new Date().getTime()) * 1000000n, true);
    }

    private sys_get_random(buf: number, buf_len: number): void {
        const { u32, u8 } = this._memoryManager!.view;
        while (buf_len >= 4) {
            u32[buf >> 2] = Math.random() * (1 << 32);
            buf += 4;
            buf_len -= 4;
        }
        while (buf_len > 0) {
            u8[buf] = Math.random() * (1 << 8);
            ++buf;
            --buf_len;
        }
    }

    private sys_write_stderr(buf: number, buf_len: number): void {
        if (!this._memoryManager) { return; }
        this._stderr.write(this._memoryManager.view, buf, buf_len);
    }

    private sys_write_stdout(buf: number, buf_len: number): void {
        if (!this._memoryManager) { return; }
        this._stdout.write(this._memoryManager.view, buf, buf_len);
    }

    public setImports(moduleName: string, importTable: ImportTable): void {
        this._interop.setImports(moduleName, importTable);
    }

    public log(text: string): void {
        if (!this._deferLogsToTick || this._inTick) {
            this.dispatchLog(text);
            return;
        }
        this._pendingLogs.push(text);
    }

    private dispatchLog(text: string): void {
        console.log(`DOTNET: ${text}`);
    }

    public compile(wasmBytes: Uint8Array<ArrayBuffer>): void {
        if (this._compiled) { return; }

        // Compile wasm module
        if (this._wasmModule) {
            this.log(`Reusing wasm module from previous attempt...`);
        } else {
            const t0 = this._profileFn();
            this._wasmModule = new WebAssembly.Module(wasmBytes);
            const t1 = this._profileFn();
            this.log(`Compiled wasm module in ${t1 - t0} ms`);
        }

        // Instantiate wasm module
        if (this._wasmInstance) {
            this.log(`Reusing wasm instance from previous attempt...`);
        } else {
            const t0 = this._profileFn();
            this._wasmInstance = new WebAssembly.Instance(this._wasmModule, this.getWasmImports()) as ScreepsDotNetWasmInstance;
            const t1 = this._profileFn();
            this.log(`Instantiated wasm module in ${t1 - t0} ms`);
        }

        // Wire things up
        this._memoryManager = new WasmMemoryManager(this._wasmInstance.exports.memory);
        this._interop.memoryManager = this._memoryManager;
        this._interop.malloc = this._wasmInstance.exports.malloc;
        this._compiled = true;
    }

    public start(customInitExportNames?: ReadonlyArray<string>): void {
        if (!this._wasmInstance || !this._compiled || this._started || !this._memoryManager) { return; }

        // Run WASM entrypoint
        try {
            const t0 = this._profileFn();
            this._wasmInstance.exports._initialize();
            const t1 = this._profileFn();
            this.log(`Started in ${t1 - t0} ms`);
        } catch (err) {
            if (err instanceof Error) {
                this.log(err.stack ?? `${err}`);
            } else {
                this.log(`${err}`);
            }
        }

        // Run bindings init
        this._bindings?.init(this._wasmInstance.exports, this._memoryManager);

        // Run usercode init
        {
            const t0 = this._profileFn();
            if (customInitExportNames) {
                for (const exportName of customInitExportNames) {
                    (this._wasmInstance.exports as unknown as Record<string, () => void>)[exportName]();
                }
            }
            this._wasmInstance.exports['screeps:screepsdotnet/botapi#init']();
            const t1 = this._profileFn();
            if (this._profilingEnabled) {
                this.log(`Init in ${(((t1 - t0) * 100)|0)/100} ms (${this._interop.buildProfilerString()})`);
            }
        }
        this._started = true;
    }

    public loop(): void {
        if (!this._wasmInstance || !this._started) { return; }

        // Run bindings loop
        this._interop.loop();
        this._bindings?.loop();

        // Dispatch log messages
        this._inTick = true;
        this.dispatchPendingLogs();

        // Run usercode loop
        {
            const t0 = this._profileFn();
            this._wasmInstance.exports['screeps:screepsdotnet/botapi#loop']();
            const t1 = this._profileFn();
            if (this._profilingEnabled) {
                this.log(`Loop in ${(((t1 - t0) * 100)|0)/100} ms (${this._interop.buildProfilerString()})`);
            }
        }
    }

    private getWasmImports(): WebAssembly.Imports {
        const imports: WebAssembly.Imports = {
            ['screeps:screepsdotnet/js-bindings']: {
                ...this._interop.interopImport,
            },
            ['screeps:screepsdotnet/system-bindings']: {
                ...this._systemImport,
            },
        };
        if (this._env === 'world' || this._env === 'test') {
            imports['screeps:screepsdotnet/world-bindings'] = {
                ...this._bindings?.bindingsImport,
            };
        } else if (this._env === 'arena') {
            imports['screeps:screepsdotnet/arena-bindings'] = {
                ...this._bindings?.bindingsImport,
            };
        }
        return imports;
    }

    private dispatchPendingLogs(): void {
        for (let i = 0; i < this._pendingLogs.length; ++i) {
            this.dispatchLog(this._pendingLogs[i]);
        }
        this._pendingLogs.length = 0;
    }
}
