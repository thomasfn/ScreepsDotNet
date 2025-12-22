import { Interop } from '../interop.js';
import BaseBindings from './base.js';
import WorldBindings from './noop-world.js';
import ArenaBindings from './noop-arena.js';
import TestBindings from './test.js';

const bindingsTable: Record<string, { new(logFunc: (text: string) => void, interop: Interop): BaseBindings }> = {
    "arena": ArenaBindings,
    "world": WorldBindings,
    "test": TestBindings,
};

export function getBindings(env: string, logFunc: (text: string) => void, interop: Interop): BaseBindings | undefined {
    const bindingsCtor = bindingsTable[env];
    if (!bindingsCtor) {
        return undefined;
    }
    return new bindingsCtor(logFunc, interop);
}
