console.log(`#${Game.time}: INIT state refreshed, beginning init sequence`);

var bootloader, bundle, dotNet, dotNetExports, startupComplete;
var accumCpuTime = 0;

function getCpuTime() {
    return accumCpuTime + Game.cpu.getUsed();
}

// ~50ms
function loadJs() {
    const before = Game.cpu.getUsed();
    bootloader = require('./bootloader');
    bundle = require('./bundle');
    const after = Game.cpu.getUsed();
    console.log(`#${Game.time} INIT: loaded js dependencies (${after - before}ms)`);
}

// ~310ms
function initDotNet() {
    const before = Game.cpu.getUsed();
    dotNet = new bootloader.DotNet(bundle, 'world');
    dotNet.setPerfFn(function() { return getCpuTime() * 1000000; });
    dotNet.setVerboseLogging(false);
    dotNet.setModuleImports('object', {
        getConstructorOf: (x) => Object.getPrototypeOf(x).constructor,
        getKeysOf: (x) => Object.keys(x),
        interpretDateTime: (x) => x.getTime() / 1000,
        create: Object.create,
        set: (obj, key, val) => obj[key] = val,
        get: (obj, key) => obj[key],
    });
    const prototypes = {
        StructureSpawn,
        StructureContainer,
        StructureController,
        StructureExtension,
        StructureStorage,
        StructureRampart,
        OwnedStructure,
        StructureRoad,
        StructureWall,
        Structure,
        Source,
        Creep,
        Flag,
        Resource,
        ConstructionSite,
        RoomObject,
        Room,
    };
    dotNet.setModuleImports('game', {
        getGameObj: () => Game,
        getPrototypes: () => prototypes,
        createRoomPosition: (x, y, roomName) => new RoomPosition(x, y, roomName),
        createCostMatrix: () => new PathFinder.CostMatrix(),
        game: {
            getObjectById: (...args) => Game.getObjectById(...args),
            notify: (...args) => Game.notify(...args),
        },
        map: {
            describeExits: (...args) => Game.map.describeExits(...args),
            findExit: (...args) => Game.map.findExit(...args),
            findRoute: (...args) => Game.map.findRoute(...args),
            getRoomLinearDistance: (...args) => Game.map.getRoomLinearDistance(...args),
            getRoomTerrain: (...args) => Game.map.getRoomTerrain(...args),
            getWorldSize: (...args) => Game.map.getWorldSize(...args),
            getRoomStatus: (...args) => Game.map.getRoomStatus(...args),
        },
        cpu: {
            getHeapStatistics: (...args) => Game.cpu.getHeapStatistics(...args),
            getUsed: (...args) => Game.cpu.getUsed(...args),
            halt: (...args) => Game.cpu.halt(...args),
            setShardLimits: (...args) => Game.cpu.setShardLimits(...args),
            unlock: (...args) => Game.cpu.unlock(...args),
            generatePixel: (...args) => Game.cpu.generatePixel(...args),
        },
    });
    const wrappedPrototypes = buildWrappedPrototypes(prototypes);
    dotNet.setModuleImports('game/prototypes/wrapped', {
        ...wrappedPrototypes,
        Spawning: buildWrappedPrototype(StructureSpawn.Spawning),
        Store: {
            getCapacity: (thisObj, resourceType) => thisObj.getCapacity(resourceType),
            getUsedCapacity: (thisObj, resourceType) => thisObj.getUsedCapacity(resourceType),
            getFreeCapacity: (thisObj, resourceType) => thisObj.getFreeCapacity(resourceType),
        },
        CostMatrix: {
            ...buildWrappedPrototype(PathFinder.CostMatrix),
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
        Room: {
            ...wrappedPrototypes.Room,
            createFlag: (thisObj, ...args) => {
                const result = thisObj.createFlag(...args);
                if (typeof result === 'string') {
                    return { name: result, code: 0 };
                } else {
                    return { code: result };
                }
            }
        },
        PathFinder,
    });
    try {
        dotNet.init();
    } catch (err) {
        console.log(`#${Game.time} INIT: fatal error initialising runtime ${err}`);
        console.log(`#${Game.time} INIT: shutting down and starting again`);
        //Game.cpu.halt && Game.cpu.halt();
        return false;
    }
    const after = Game.cpu.getUsed();
    console.log(`#${Game.time} INIT: created dotnet instance (${after - before}ms)`);
}

function buildWrappedPrototypes(prototypes) {
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

function startup() {
    if (!dotNet) {
        if (Game.cpu.bucket < 1000) {
            console.log(`#${Game.time}: INIT FROM COLD START REFUSED - waiting for bucket to be full (currently ${Game.cpu.bucket}ms of 1000ms)`);
            return false;
        }
        loadJs();
        initDotNet();
        return false;
    }
    if (Game.cpu.tickLimit < 500) {
        console.log(`#${Game.time}: INIT REFUSED - waiting for tickLimit to be full (currently ${Game.cpu.tickLimit}ms of 500ms)`);
        return false;
    }

    if (!dotNet.ready) {
        dotNet.loop();
        return false;
    }
    // if (!dotNet.ready) {
    //     console.log(`#${Game.time} INIT: fatal error initialising runtime (was not ready at expected time, probably script exec timeout)`);
    //     console.log(`#${Game.time} INIT: shutting down and starting again`);
    //     //Game.cpu.halt && Game.cpu.halt();
    //     return false;
    // }
    dotNetExports = dotNet.getExports();

    console.log(`#${Game.time}: INIT SUCCESS (${Game.cpu.tickLimit - Game.cpu.getUsed()}ms remaining cpu this tick)`);

    return true;
}

function loop() {
    if (!startupComplete) {
        startupComplete = startup();
    }
    if (startupComplete && dotNetExports) {
        dotNet.loop(() => { dotNetExports.ScreepsDotNet.Program.Loop(); });
    }
    accumCpuTime += Game.cpu.getUsed();
}

module.exports.loop = loop;
