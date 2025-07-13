console.log(`#${Game.time}: INIT state refreshed, beginning init sequence`);

var bootloaderJs, rawWasm;
function loadJs() {
    const before = Game.cpu.getUsed();
    bootloaderJs = require('./bootloader');
    rawWasm = require('./ScreepsDotNet');
    const after = Game.cpu.getUsed();
    console.log(`#${Game.time}: INIT loaded js dependencies (${after - before}ms)`);
}

var decompressedRawWasm;
function decompressWasm() {
    if (/*WASM_COMPRESSED*/) {
        const before = Game.cpu.getUsed();
        decompressedRawWasm = bootloaderJs.decompressWasm(rawWasm, /*ORIGINAL_WASM_SIZE*/);
        const after = Game.cpu.getUsed();
        console.log(`#${Game.time}: INIT decompressed wasm (${after - before}ms)`);
        return;
    } else {
        decompressedRawWasm = rawWasm;
    }
}

var bootloader;
function startup() {
    if (Game.cpu.bucket < 1000) {
        console.log(`#${Game.time}: INIT FROM COLD START REFUSED - waiting for bucket to be full (currently ${Game.cpu.bucket}ms of 1000ms)`);
        return false;
    }
    if (!bootloaderJs || !rawWasm) {
        loadJs();
    }
    if (!decompressedRawWasm) {
        decompressWasm();
    }
    if (!bootloader) {
        bootloader = global.DOTNET = new bootloaderJs.Bootloader('world', Game.cpu.getUsed);
        bootloader.profilingEnabled = false; // Set this to true to get some startup and per-tick profiling output in console
    }
    if (!bootloader.compiled) {
        bootloader.compile(decompressedRawWasm);
        return false;
    }
    if (!bootloader.started) {
        bootloader.start([/*CUSTOM_INIT_EXPORT_NAMES*/]);
    }

    console.log(`#${Game.time}: INIT SUCCESS (${Game.cpu.tickLimit - Game.cpu.getUsed()}ms remaining cpu this tick)`);
    return true;
}

var startupComplete;
