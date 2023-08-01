import * as utils from 'game/utils';
import * as prototypes from 'game/prototypes';
import * as constants from 'game/constants';
import * as pathFinder from 'game/path-finder';
import * as visual from 'game/visual';
import { DotNet } from './bootloader';
import * as manifest from './bundle.mjs';

const dotNet = new DotNet(manifest);
dotNet.setPerfFn(utils.getCpuTime);
dotNet.setVerboseLogging(false);
dotNet.setModuleImports('game/utils', {
    ...utils,
    getTerrain: (minX, minY, maxX, maxY, outMemoryView) => {
        const w = (maxX - minX) + 1;
        const h = (maxY - minY) + 1;
        const arr = new Uint8Array(w * h);
        const pos = {};
        let i = 0;
        for (let y = minY; y <= maxY; ++y) {
            pos.y = y;
            for (let x = minX; x <= maxX; ++x) {
                pos.x = x;
                arr[i] = utils.getTerrainAt(pos);
                ++i;
            }   
        }
        outMemoryView.set(arr);
    },
});
dotNet.setModuleImports('game/prototypes', prototypes);
dotNet.setModuleImports('game/constants', {
    get: () => constants,
});
dotNet.setModuleImports('game/pathFinder', {
    ...pathFinder,
    CostMatrix: {
        ...buildWrappedPrototype(pathFinder.CostMatrix),
        setRect: (thisObj, minX, minY, maxX, maxY, memoryView) => {
            const w = (maxX - minX) + 1;
            const h = (maxY - minY) + 1;
            const arr = new Uint8Array(w * h);
            memoryView.copyTo(arr);
            let i = 0;
            for (let y = minY; y <= maxY; ++y) {
                for (let x = minX; x <= maxX; ++x) {
                    thisObj.set(x, y, arr[i]);
                    ++i;
                }   
            }
        },
    },
    createCostMatrix: () => new pathFinder.CostMatrix(),
});
dotNet.setModuleImports('game/visual', {
    Visual: buildWrappedPrototype(visual.Visual),
    createVisual: (layer, persistent) => new visual.Visual(layer, persistent),
});
dotNet.setModuleImports('game', {
    getUtils: () => utils,
    getPrototypes: () => prototypes,
});
dotNet.setModuleImports('object', {
    getConstructorOf: (x) => Object.getPrototypeOf(x).constructor,
    create: Object.create,
    set: (obj, key, val) => obj[key] = val,
    get: (obj, key) => obj[key],
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
        wrappedPrototypes[prototypeName] = buildWrappedPrototype(prototypes[prototypeName]);
    }
    return wrappedPrototypes;
}

function buildWrappedPrototype(constructor) {
    /** @type {Record<string, Function>} */
    const wrappedPrototype = {};
    const prototype = constructor.prototype;
    const keys = Object.getOwnPropertyNames(prototype);
    for (const key of keys) {
        if (key === 'constructor') { continue; }
        const value = Object.getOwnPropertyDescriptor(prototype, key).value;
        if (typeof value !== 'function') { continue; }
        wrappedPrototype[key] = (thisObj, ...args) => value.call(thisObj, ...args);
    }
    return wrappedPrototype;
}

const exports = dotNet.getExports();

export function loop() {
    dotNet.loop(() => exports.ScreepsDotNet.Program.Loop());
}
