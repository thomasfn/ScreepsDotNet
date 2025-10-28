import { Interop } from '../interop.js';
import BaseBindings from './base.js';
import WorldBindings from './noop-world.js';
import ArenaBindings from './noop-arena.js';

const bindingsTable: Record<string, { new(logFunc: (text: string) => void, interop: Interop): BaseBindings }> = {
    "arena": ArenaBindings,
    "world": WorldBindings,
};

export function getBindings(env: string, logFunc: (text: string) => void, interop: Interop): BaseBindings | undefined {
    const bindingsCtor = bindingsTable[env];
    if (!bindingsCtor) {
        return undefined;
    }
    return new bindingsCtor(logFunc, interop);
}
