
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
function _WebAssemblyInstantiate(bytes, imports) {
    const compiledModule = new WebAssembly.Module(bytes);
    const compiledInstance = new WebAssembly.Instance(compiledModule, imports);
    return Promise.resolve({ instance: compiledInstance, module: compiledModule });
}
