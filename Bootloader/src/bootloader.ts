import 'fastestsmallesttextencoderdecoder';
import { WASI, File, OpenFile, Fd } from '@bjorn3/browser_wasi_shim';
import base64Decode from 'fast-base64-decode'
import { decode as base32768Decode } from 'base32768';
import * as fflate from 'fflate';

import { ImportTable, Interop } from './interop.js';
import { ScreepsDotNetExports } from './common.js';
import { WasmMemoryManager } from './memory.js';
import { BaseBindings } from './bindings/base.js';
import { ArenaBindings } from './bindings/arena.js';
import { WorldBindings } from './bindings/world.js';
import { TestBindings } from './bindings/test.js';

const utf8Decoder = new TextDecoder();

class Stdio extends Fd {
    private readonly outFunc: (text: string) => void;

    private buffer?: string;

    constructor(outFunc: (text: string) => void) {
        super();
        this.outFunc = outFunc;
    }

    public fd_write(view8: Uint8Array, iovs: [{ buf_len: number, buf: number }]): {ret: number, nwritten: number} {
        let nwritten = 0;
        for (let iovec of iovs) {
            let buffer = view8.slice(iovec.buf, iovec.buf + iovec.buf_len);
            this.addTextToBuffer(utf8Decoder.decode(buffer));

            nwritten += iovec.buf_len;
        }
        return { ret: 0, nwritten };
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
            this.outFunc(line);
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

const enum WASI_CLOCKID {
    REALTIME = 0,
    MONOTONIC = 1,
}

const enum WASI_ERRNO {
    SUCCESS = 0,
    BADF = 8,
    INVAL = 28,
    PERM = 63,
}

export class Bootloader {
    private readonly _pendingLogs: string[] = [];
    private readonly _deferLogsToTick: boolean;
    private readonly _profileFn: () => number;

    private readonly _stdin: Fd;
    private readonly _stdout: Fd;
    private readonly _stderr: Fd;

    private readonly _wasi: WASI;
    private readonly _interop: Interop;
    private readonly _bindings?: BaseBindings;

    private _wasmModule?: WebAssembly.Module;
    private _wasmInstance?: ScreepsDotNetWasmInstance;
    private _memoryManager?: WasmMemoryManager;
    private _memorySize: number = 0;
    private _compiled: boolean = false;
    private _started: boolean = false;

    private _inTick: boolean = false;
    private _profilingEnabled: boolean = false;

    public get compiled() { return this._compiled; }
    public get started() { return this._started; }

    public get profilingEnabled() { return this._profilingEnabled; }
    public set profilingEnabled(value) { this._profilingEnabled = value; }

    public get exports() { return this._wasmInstance!.exports; }

    constructor(env: 'world'|'arena'|'test', profileFn: () => number) {
        this._deferLogsToTick = env === 'arena';
        this._profileFn = profileFn;

        this._stdin = new OpenFile(new File([]));
        this._stdout = new Stdio(this.log.bind(this));
        this._stderr = new Stdio(this.log.bind(this));

        this._wasi = new WASI(['ScreepsDotNet'], [`ENV=${env}`], [this._stdin, this._stdout, this._stderr], { debug: false });
        this._interop = new Interop(profileFn);

        this.setImports('__object', {
            hasProperty: (obj: Record<string | number | symbol, unknown>, key: string) => key in obj,
            getTypeOfProperty: (obj: Record<string | number | symbol, unknown>, key: string) => JSTYPE_TO_ENUM[typeof obj[key]],
            getKeys: (obj: Record<string | number | symbol, unknown>) => Object.keys(obj),
            getProperty: (obj: Record<string | number | symbol, unknown>, key: string | number | symbol) => obj[key],
            setProperty: (obj: Record<string | number | symbol, unknown>, key: string | number | symbol, value: unknown) => obj[key] = value,
            deleteProperty: (obj: Record<string | number | symbol, unknown>, key: string) => delete obj[key],
            create: (proto: object) => Object.create(proto),
        });

        switch (env) {
            case 'world':
                this._bindings = new WorldBindings(this.log.bind(this), this._interop);
                break;
            case 'arena':
                this._bindings = new ArenaBindings(this.log.bind(this), this._interop);
                break;
            case 'test':
                this._bindings = new TestBindings(this.log.bind(this), this._interop);
                break;
        }
        if (this._bindings) {
            for (const moduleName in this._bindings.imports) {
                this.setImports(moduleName, this._bindings.imports[moduleName]);
            }
        }
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

    public compile(wasmBytes: Uint8Array): void {
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
            this.log(`Instantiated wasm module in ${t1 - t0} ms (exports: ${JSON.stringify(Object.keys(this._wasmInstance.exports))})`);
        }

        // Wire things up
        this._memoryManager = new WasmMemoryManager(this._wasmInstance.exports.memory);
        this._interop.memoryManager = this._memoryManager;
        this._memorySize = this._wasmInstance.exports.memory.buffer.byteLength;
        this._interop.malloc = this._wasmInstance.exports.malloc;
        this._compiled = true;
    }

    public start(customInitExportNames?: ReadonlyArray<string>): void {
        if (!this._wasmInstance || !this._compiled || this._started || !this._memoryManager) { return; }

        // Start WASI
        try {
            const t0 = this._profileFn();
            this._wasi.initialize(this._wasmInstance);
            const t1 = this._profileFn();
            this.log(`Started WASI in ${t1 - t0} ms`);
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
            this._wasmInstance.exports.screepsdotnet_init();
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
            this._wasmInstance.exports.screepsdotnet_loop();
            const t1 = this._profileFn();
            if (this._profilingEnabled) {
                this.log(`Loop in ${(((t1 - t0) * 100)|0)/100} ms (${this._interop.buildProfilerString()})`);
            }
        }
    }

    private getWasmImports(): WebAssembly.Imports {
        return {
            wasi_snapshot_preview1: {
                ...this._wasi.wasiImport,
                // Override the wasi shim's implementation of clock_res_get and clock_time_get with our own
                clock_res_get: this.clock_res_get.bind(this),
                clock_time_get: this.clock_time_get.bind(this),
            },
            js: {
                ...this._interop.interopImport,
            },
            bindings: {
                ...this._bindings?.bindingsImport,
            }
        };
    }

    private clock_res_get(id: number, res_ptr: number): number {
        // We only support the realtime clock
        // The monotonic clock's implementation in the wasi shim uses performance.now which isn't available in screeps
        if (id === WASI_CLOCKID.REALTIME) {
            const dataView = this._memoryManager!.view.dataView;
            dataView.setBigUint64(res_ptr, BigInt(1), true);
            return 0;
        }
        return WASI_ERRNO.INVAL;
    }

    private clock_time_get(id: number, precision: number, time_ptr: number): number {
        // We only support the realtime clock
        // The monotonic clock's implementation in the wasi shim uses performance.now which isn't available in screeps
        if (id === WASI_CLOCKID.REALTIME) {
            const dataView = this._memoryManager!.view.dataView;
            dataView.setBigUint64(time_ptr, BigInt(new Date().getTime()) * 1000000n, true);
            return 0;
        }
        return WASI_ERRNO.INVAL;
    }

    private dispatchPendingLogs(): void {
        for (let i = 0; i < this._pendingLogs.length; ++i) {
            this.dispatchLog(this._pendingLogs[i]);
        }
        this._pendingLogs.length = 0;
    }
}
