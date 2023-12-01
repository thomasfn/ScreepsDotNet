import { promises as fs } from 'fs';

import { Interop } from './interop';

const wasmFilename = '/ScreepsDotNet/ScreepsDotNet.wasm';

interface ScreepsDotNetExports extends WebAssembly.Exports {
    screepsdotnet_init(): void;
    screepsdotnet_loop(): void;
}

async function main() {
    console.log(`Loading '${wasmFilename}'...`);
    const wasmData = await fs.readFile(wasmFilename);

    console.log(`Compiling wasm module...`);
    const t0 = performance.now();
    const module = await WebAssembly.compile(wasmData);
    const t1 = performance.now();
    console.log(`Compiled wasm module in ${t1 - t0} ms`);

    console.log(`Preparing WASI...`);
    const { WASI, File, OpenFile, PreopenDirectory, Fd } = await import("@bjorn3/browser_wasi_shim");

    const utf8Decoder = new TextDecoder('utf8');

    class Stdio extends Fd {
        private readonly outFunc: (text: string) => void;
    
        constructor(outFunc: (text: string) => void) {
            super();
            this.outFunc = outFunc;
        }

        public fd_write(view8: Uint8Array, iovs: [{ buf_len: number, buf: number }]): {ret: number, nwritten: number} {
            let nwritten = 0;
            for (let iovec of iovs) {
                let buffer = view8.slice(iovec.buf, iovec.buf + iovec.buf_len);
                this.outFunc(utf8Decoder.decode(buffer));

                nwritten += iovec.buf_len;
            }
            return { ret: 0, nwritten };
        }
    }

    const stdin = new OpenFile(new File([]));
    const stdout = new Stdio(console.log);
    const stderr = new Stdio(console.log);

    const args: string[] = ['ScreepsDotNet'];
    const env: string[] = [];

    const wasi = new WASI(args, env, [
        stdin,
        stdout,
        stderr,
    ]);

    console.log(`Preparing interop...`);
    const interop = new Interop({
        object: {
            getProperty: (obj: Record<string | number | symbol, unknown>, key: string | number | symbol) => obj[key],
            setProperty: (obj: Record<string | number | symbol, unknown>, key: string | number | symbol, value: unknown) => obj[key] = value,
            create: (proto: object) => Object.create(proto),
        },
        test: {
            echo: (value: unknown) => { console.log(`echoing '${value}' (${typeof value})`); return value; },
            addTwo: (a: number, b: number) => a + b,
            toUppercase: (str: string) => str.toUpperCase(),
            stringify: (value: unknown) => JSON.stringify(value),
        }
    });

    console.log(`Instantiating wasm instance...`);
    const t2 = performance.now();
    const instance = await WebAssembly.instantiate(module, {
        wasi_snapshot_preview1: {
            ...wasi.wasiImport,
            clock_res_get(id: number, res_ptr: number) {
                const buffer = new DataView(wasi.inst.exports.memory.buffer);
                buffer.setBigUint64(res_ptr,BigInt(0),true);
                return 0;
            },
        },
        js: {
            ...interop.interopImport,
        },
    });
    const t3 = performance.now();
    console.log(`Instantiated wasm instance in ${t3 - t2} ms`);

    console.log(`Starting app via WASI...`);
    const instanceEx = instance as { exports: { memory: WebAssembly.Memory, _start: () => unknown, malloc: (sz: number) => number } };
    interop.memory = instanceEx.exports.memory;
    interop.malloc = instanceEx.exports.malloc;
    try {
        wasi.start(instanceEx);
    } catch (err) {
        if (err instanceof Error) {
            console.log(err.stack);
        } else {
            console.log(err);
        }
    }

    console.log(`Running ScreepsDotNet Init...`);
    (instance.exports as ScreepsDotNetExports).screepsdotnet_init();

    console.log(`Running ScreepsDotNet Loop...`);
    (instance.exports as ScreepsDotNetExports).screepsdotnet_loop();
}


main();
