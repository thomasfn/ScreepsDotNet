import { promises as fs } from 'fs';
import { Bootloader } from '../src/bootloader.js';

const wasmFilename = '/ScreepsDotNet/ScreepsDotNet.wasm';

async function main() {
    console.log(`Loading '${wasmFilename}'...`);
    const wasmData = await fs.readFile(wasmFilename);

    console.log(`Starting bootloader...`);
    try {
        const bootloader = new Bootloader('world', () => performance.now());
        bootloader.setImports('test', {
            echo: (value: unknown) => { console.log(`echoing '${value}' (${typeof value})`); return value; },
            addTwo: (a: number, b: number) => a + b,
            toUppercase: (str: string) => str.toUpperCase(),
            stringify: (value: unknown) => JSON.stringify(value),
            reverseArray: (arr: number[]) => arr.reverse(),
            fillBuffer: (dataView: DataView) => {
                for (let i = 0; i < dataView.byteLength; ++i) {
                    dataView.setUint8(i, i + 1);
                }
            },
        });
        bootloader.compile(wasmData);
        bootloader.start();
        bootloader.loop();
    } catch (err) {
        console.log(err);
    }
}

main();
