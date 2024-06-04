import { promises as fs } from 'fs';
import { Bootloader } from '../src/bootloader.js';

const wasmFilename = '/ScreepsDotNet/ScreepsDotNet.ExampleWorldBot.wasm';

async function main() {
    console.log(`Loading '${wasmFilename}'...`);
    const wasmData = await fs.readFile(wasmFilename);

    console.log(`Starting bootloader...`);
    try {
        const bootloader = new Bootloader('test', () => performance.now());
        bootloader.compile(wasmData);
        bootloader.start([]);
        bootloader.loop();
    } catch (err) {
        console.log(err);
    }
}

main();
