import { Interop } from './interop';
import { WASI, File, OpenFile, Fd } from '@bjorn3/browser_wasi_shim';
import 'fastestsmallesttextencoderdecoder';

const utf8Decoder = new TextDecoder();

interface ScreepsDotNetExports extends WebAssembly.Exports {
    memory: WebAssembly.Memory;
    _start(): unknown;
    malloc(sz: number): number;
    free(ptr: number): void;
    screepsdotnet_init(): void;
    screepsdotnet_loop(): void;
}

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

export class Bootloader {
    private readonly _pendingLogs: string[] = [];
    private readonly _deferLogsToTick: boolean;
    private readonly _profileFn: () => number;

    private readonly _stdin: Fd;
    private readonly _stdout: Fd;
    private readonly _stderr: Fd;

    private readonly _wasi: WASI;
    private readonly _interop: Interop;

    private _wasmModule?: WebAssembly.Module;
    private _wasmInstance?: ScreepsDotNetWasmInstance;
    private _compiled: boolean = false;
    private _started: boolean = false;

    private _inTick: boolean = false;

    public get compiled() { return this._compiled; }
    public get started() { return this._started; }

    constructor(env: 'world'|'arena', profileFn: () => number) {
        this._deferLogsToTick = env === 'arena';
        this._profileFn = profileFn;

        this._stdin = new OpenFile(new File([]));
        this._stdout = new Stdio(this.log.bind(this));
        this._stderr = new Stdio(this.log.bind(this));

        this._wasi = new WASI(['ScreepsDotNet'], [`ENV=${env}`], [this._stdin, this._stdout, this._stderr], { debug: false });
        this._interop = new Interop();

        this.setImports('object', {
            getProperty: (obj: Record<string | number | symbol, unknown>, key: string | number | symbol) => obj[key],
            setProperty: (obj: Record<string | number | symbol, unknown>, key: string | number | symbol, value: unknown) => obj[key] = value,
            create: (proto: object) => Object.create(proto),
        });
    }

    public setImports(moduleName: string, importTable: Record<string, (...args: any[]) => unknown>): void {
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
            this.log(`Reusing wasm module fromn previous attempt...`);
        } else {
            const t0 = this._profileFn();
            this._wasmModule = new WebAssembly.Module(wasmBytes);
            const t1 = this._profileFn();
            this.log(`Compiled wasm module in ${t1 - t0} ms`);
        }

        // Instantiate wasm module
        if (this._wasmInstance) {
            this.log(`Reusing wasm instance fromn previous attempt...`);
        } else {
            const t0 = this._profileFn();
            this._wasmInstance = new WebAssembly.Instance(this._wasmModule, this.getWasmImports()) as ScreepsDotNetWasmInstance;
            const t1 = this._profileFn();
            this.log(`Instantiated wasm module in ${t1 - t0} ms`);
        }

        // Wire things up
        this._interop.memory = this._wasmInstance.exports.memory;
        this._interop.malloc = this._wasmInstance.exports.malloc;
        this._compiled = true;
    }

    public start(): void {
        if (!this._wasmInstance || this._started) { return; }

        // Start WASI
        try {
            const t0 = this._profileFn();
            this._wasi.start(this._wasmInstance);
            const t1 = this._profileFn();
            this.log(`Started WASI in ${t1 - t0} ms`);
        } catch (err) {
            if (err instanceof Error) {
                this.log(err.stack ?? `${err}`);
            } else {
                this.log(`${err}`);
            }
        }

        // Run usercode init
        {
            const t0 = this._profileFn();
            this._wasmInstance.exports.screepsdotnet_init();
            const t1 = this._profileFn();
            this.log(`Init in ${t1 - t0} ms`);
        }
        this._started = true;
    }

    public loop(): void {
        if (!this._wasmInstance || this._started) { return; }

        // Dispatch log messages
        this._inTick = true;
        this.dispatchPendingLogs();

        // Run usercode loop
        {
            const t0 = this._profileFn();
            this._wasmInstance.exports.screepsdotnet_init();
            const t1 = this._profileFn();
            this.log(`Loop in ${t1 - t0} ms`);
        }
    }

    private getWasmImports(): WebAssembly.Imports {
        return {
            wasi_snapshot_preview1: {
                ...this._wasi.wasiImport,
                clock_res_get: (id: number, res_ptr: number) => {
                    const buffer = new DataView(this._wasi.inst.exports.memory.buffer);
                    buffer.setBigUint64(res_ptr, BigInt(0), true);
                    return 0;
                },
            },
            js: {
                ...this._interop.interopImport,
            },
        };
    }

    private dispatchPendingLogs(): void {
        for (let i = 0; i < this._pendingLogs.length; ++i) {
            this.dispatchLog(this._pendingLogs[i]);
        }
        this._pendingLogs.length = 0;
    }
}
