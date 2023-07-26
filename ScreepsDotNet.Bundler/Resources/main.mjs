import * as utils from 'game/utils';
import * as prototypes from 'game/prototypes';
import { DotNet } from './bootloader';
import * as manifest from './bundle.mjs';

const dotNet = new DotNet(manifest);
dotNet.setPerfFn(utils.getCpuTime);
dotNet.setVerboseLogging(false);
dotNet.setModuleImports('game/utils', utils);
dotNet.setModuleImports('game/prototypes', prototypes);
dotNet.setModuleImports('game', {
    getUtils: () => utils,
    getPrototypes: () => prototypes,
});
dotNet.setModuleImports('object', {
    getConstructorOf: (x) => Object.getPrototypeOf(x).constructor,
    create: Object.create,
});
dotNet.setModuleImports('game/prototypes/wrapped', {
    ...buildWrappedPrototypes(),
    Store: {
        getCapacity: (thisObj, resourceType) => thisObj.getCapacity(resourceType),
        getUsedCapacity: (thisObj, resourceType) => thisObj.getUsedCapacity(resourceType),
        getFreeCapacity: (thisObj, resourceType) => thisObj.getFreeCapacity(resourceType),
    },
});
dotNet.init();

function buildWrappedPrototypes() {
    /** @type {Record<string, Record<string, Function>>} */
    const wrappedPrototypes = {};
    for (const prototypeName in prototypes) {
        /** @type {Record<string, Function>} */
        const wrappedPrototype = {};
        const constructor = prototypes[prototypeName];
        const prototype = constructor.prototype;
        const keys = Object.getOwnPropertyNames(prototype);
        for (const key of keys) {
            if (key === 'constructor') { continue; }
            const value = Object.getOwnPropertyDescriptor(prototype, key).value;
            if (typeof value !== 'function') { continue; }
            wrappedPrototype[key] = (thisObj, ...args) => value.call(thisObj, ...args);
        }
        wrappedPrototypes[prototypeName] = wrappedPrototype;
    }
    return wrappedPrototypes;
}

const exports = dotNet.getExports();

export function loop() {
    dotNet.loop(() => exports.ScreepsDotNet.Program.Loop());
}
