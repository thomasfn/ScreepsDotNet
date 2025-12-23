import { promises as fs } from 'fs';
import { Bootloader } from '../src/bootloader.js';

const wasmFilename = '/ScreepsDotNet/world/ScreepsDotNet.wasm';

async function main() {
    console.log(`Loading '${wasmFilename}'...`);
    const wasmData = await fs.readFile(wasmFilename);

    console.log(`Starting bootloader...`);
    try {
        const bootloader = new Bootloader('test', () => performance.now());
        bootloader.compile(wasmData as unknown as Uint8Array<ArrayBuffer>);
        bootloader.start([]);
        bootloader.loop();
    } catch (err) {
        console.log(err);
    }
}

main();
