
import {
    setTimeout,
    clearTimeout,
    setInterval,
    clearInterval,
    setImmediate,
} from './timeouts';
import * as importedLogging from './logging';
import { TextEncoder, TextDecoder } from 'fastestsmallesttextencoderdecoder-encodeinto';
const console = { ...importedLogging };
const globalThis = this;
globalThis.setTimeout = setTimeout;
globalThis.clearTimeout = clearTimeout;
globalThis.setInterval = setInterval;
globalThis.clearInterval = clearInterval;
globalThis.setImmediate = setImmediate;
import Promise from 'promise-polyfill';
function _import(moduleName) {
    importedLogging.debug(`intercepted import with module name of '${moduleName}'`);
    return new Promise((resolve) => {});
}
const _document = {
    currentScript: {
        src: './',
    },
};
let wasmPrevAttemptBytes, wasmPrevAttemptModule;
function _WebAssemblyInstantiate(bytes, imports) {
    try {
        let compiledModule;
        if (wasmPrevAttemptBytes == bytes && wasmPrevAttemptModule) {
            compiledModule = wasmPrevAttemptModule;
            importedLogging.debug(`wasm module found from previous pass`);
        } else {
            wasmPrevAttemptBytes = bytes;
            if ('Game' in globalThis) {
                const t0 = Game.cpu.getUsed();
                wasmPrevAttemptModule = new WebAssembly.Module(bytes);
                const t1 = Game.cpu.getUsed();
                importedLogging.debug(`wasm module compiled in ~${t1 - t0}ms`);
            } else {
                wasmPrevAttemptModule = new WebAssembly.Module(bytes);
            }
            compiledModule = wasmPrevAttemptModule;
        }
        const compiledInstance = new WebAssembly.Instance(compiledModule, imports);
        wasmPrevAttemptBytes = null;
        wasmPrevAttemptModule = null;
        return Promise.resolve({ instance: compiledInstance, module: compiledModule });
    } catch (err) {
        importedLogging.error(`Failed to create wasm instance: ${err}`);
        return Promise.reject(err);
    }
}
const fromCharCode = String.fromCharCode;
function decodeUtf16(o, t, e) {
    let n = "";
    const len = (t - e) >> 1;
    if (len <= 1024) {
        for (let r = 0; r < t - e; r += 2) {
            const t = o.getValue(e + r, "i16");
            n += fromCharCode(t);
        }
    } else {
        const arr = [];
        arr.length = len;
        for (let r = 0; r < t - e; r += 2) {
            const t = o.getValue(e + r, "i16");
            arr[r >> 1] = fromCharCode(t);
        }
        n = n.concat(...arr);
    }
    return n;
}