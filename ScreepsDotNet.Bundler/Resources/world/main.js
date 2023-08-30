console.log(`#${Game.time}: INIT state refreshed, beginning init sequence`);

var bootloader, bundle, dotNet, dotNetExports, startupComplete;
var accumCpuTime = 0;
var lastCheckIn = Game.time;

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
    let copyBufferPtr, copyBufferSize;
    dotNet = new bootloader.DotNet(bundle, 'world');
    dotNet.setPerfFn(function() { return getCpuTime() * 1000000; });
    dotNet.setVerboseLogging(false);
    dotNet.addCustomRuntimeSetupFunction(function(runtime) {
        const entrypointFn = runtime.Module['asm']['ScreepsDotNet_InitNative_World'];
        if (!entrypointFn) {
          console.log(`failed to call 'ScreepsDotNet_InitNative_World' (not found in wasm exports)`);
          return;
        }
        copyBufferSize = 1024 * 1024;
        copyBufferPtr = entrypointFn(copyBufferSize);
    });
    function fixupArray(obj) {
        if (obj == null) { return null; }
        if (Array.isArray(obj)) { return obj; }
        const arr = new Array(obj.length || 0);
        for (let i = 0; i < arr.length; ++i) {
            arr[i] = obj[i];
        }
        return arr;
    }
    function encodeRoomPosition({x, y, roomName}, outPtr) {
        const { HEAP32, HEAPU8 } = dotNet.runtimeApi.Module;
        // X:i32(4), y:i32(4), roomName:6i8(6), padding:2i8(2) total=16
        HEAP32[(outPtr + 0) >>> 2] = x;
        HEAP32[(outPtr + 4) >>> 2] = y;
        if (roomName.length < 4) {
            HEAPU8[outPtr + 8] = 0;
        } else {
            HEAPU8[outPtr + 8] = roomName.charCodeAt(0);
            HEAPU8[outPtr + 9] = roomName.charCodeAt(1);
            HEAPU8[outPtr + 10] = roomName.charCodeAt(2);
            HEAPU8[outPtr + 11] = roomName.charCodeAt(3);
            HEAPU8[outPtr + 12] = roomName.length >= 5 ? roomName.charCodeAt(4) : 0;
            HEAPU8[outPtr + 13] = roomName.length >= 6 ? roomName.charCodeAt(5) : 0;
            HEAPU8[outPtr + 14] = 0;
            HEAPU8[outPtr + 15] = 0;
        }
    }
    function encodeObjectId(id, outPtr) {
        const { HEAP32, HEAPU8 } = dotNet.runtimeApi.Module;
        if (id) {
            const l = id.length;
            for (let j = 0; j < l; ++j) {
                HEAPU8[outPtr + j] = id.charCodeAt(j);
            }
            for (let j = l; j < 24; ++j) {
                HEAPU8[outPtr + j] = 0;
            }
        } else {
            for (let j = 0; j < 6; ++j) {
                HEAP32[copyBufferHeadI32 + j] = 0;
            }
        }
    }
    const packetSizeInBytes = 56;
    const PACKET_FLAG_MY = (1 << 0);

    /** @param buffer {Uint8Array} */
    function encodeRoomObjectArray(arr, key) {
        const { HEAP32, HEAPU8 } = dotNet.runtimeApi.Module;
        if (copyBufferPtr == null) { throw new Error(`encodeRoomObjectArray failed as copy buffer was not allocated`); }
        // room object packet (54b):
        // - id (0..24)
        // - type id (24..28)
        // - flags (28..32)
        // - hits/progress/energy/mineralAmount (32..36)
        // - hitsMax/progressTotal/energyCapacity/density (36..40)
        // - roomPosition (40..54)

        let copyBufferHead = copyBufferPtr;
        let numEncoded = 0;
        for (let i = 0; i < arr.length; ++i) {
            if (copyBufferHead + packetSizeInBytes > copyBufferPtr + copyBufferSize) {
                console.log(`BUFFER OVERFLOW in encodeRoomObjectArray (trying to encode ${arr.length} room objects but only space for ${(copyBufferSize / packetSizeInBytes)|0})`);
                break;
            }
            const copyBufferHeadI32 = copyBufferHead >>> 2;
            let obj = arr[i];
            if (key) {
                obj = obj[key];
            }
            if (!(obj instanceof RoomObject) && obj.type) {
                obj = obj[obj.type];
            }
            if (!(obj instanceof RoomObject)) { continue; }
            ++numEncoded;
            encodeObjectId(obj.id, copyBufferHead);
            HEAP32[copyBufferHeadI32 + 6] = Object.getPrototypeOf(obj).constructor.__dotnet_typeId || 0;
            HEAP32[copyBufferHeadI32 + 7] = obj.my ? PACKET_FLAG_MY : 0;
            HEAP32[copyBufferHeadI32 + 8] = obj.hits || obj.progress || obj.energy || obj.mineralAmount || 0;
            HEAP32[copyBufferHeadI32 + 9] = obj.hitsMax || obj.progressTotal || obj.energyCapacity || obj.density || 0;
            encodeRoomPosition(obj.pos, copyBufferHead + 40);
            copyBufferHead += packetSizeInBytes;
        }
        return numEncoded;
    }
    dotNet.setModuleImports('object', {
        getConstructorOf: (x) => Object.getPrototypeOf(x).constructor,
        getKeysOf: (x) => Object.keys(x),
        interpretDateTime: (x) => x.getTime() / 1000,
        create: Object.create,
        set: (obj, key, val) => obj[key] = val,
        get: (obj, key) => obj[key],
        fixupArray,
        fixupArrayOnObject: (obj, key) => obj[key] = fixupArray(obj[key]),
        deleteOnObject: (obj, key) => delete obj[key],
    });
    const prototypes = {
        StructureSpawn,
        StructureContainer,
        StructureController,
        StructureExtension,
        StructureStorage,
        StructureRampart,
        StructureTower,
        StructureLink,
        StructureTerminal,
        StructureExtractor,
        OwnedStructure,
        StructureRoad,
        StructureWall,
        Structure,
        Source,
        Mineral,
        Creep,
        Flag,
        Resource,
        ConstructionSite,
        Tombstone,
        RoomObject,
        Room,
        RoomVisual,
    };
    dotNet.setModuleImports('game', {
        checkIn: () => lastCheckIn = Game.time,
        getGameObj: () => Game,
        getMemoryObj: () => Memory,
        getConstantsObj: () => global,
        getRawMemoryObj: () => RawMemory,
        getPrototypes: () => prototypes,
        createRoomPosition: (x, y, roomName) => new RoomPosition(x, y, roomName),
        createCostMatrix: () => new PathFinder.CostMatrix(),
        createRoomVisual: (roomName) => new RoomVisual(roomName),
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
        rawMemory: {
            get: (...args) => RawMemory.get(...args),
            set: (...args) => RawMemory.set(...args),
            setActiveSegments: (ids) => RawMemory.setActiveSegments(fixupArray(ids)),
            setActiveForeignSegment: (...args) => RawMemory.setActiveForeignSegment(...args),
            setDefaultPublicSegment: (...args) => RawMemory.setDefaultPublicSegment(...args),
            setPublicSegments: (ids) => RawMemory.setPublicSegments(fixupArray(ids)),
        },
        visual: {
            line: (...args) => Game.map.visual.line(...args),
            circle: (...args) => Game.map.visual.circle(...args),
            rect: (...args) => Game.map.visual.rect(...args),
            poly: (...args) => Game.map.visual.poly(...args),
            text: (...args) => Game.map.visual.text(...args),
            clear: (...args) => Game.map.visual.clear(...args),
            getSize: (...args) => Game.map.visual.getSize(...args),
            export: (...args) => Game.map.visual.export(...args),
            import: (...args) => Game.map.visual.import(...args),
        },
    });
    const wrappedPrototypes = buildWrappedPrototypes(prototypes);
    dotNet.setModuleImports('game/prototypes/wrapped', {
        ...wrappedPrototypes,
        RoomObject: {
            ...wrappedPrototypes.RoomObject,
            getEncodedRoomPosition: (thisObj, outPtr) => encodeRoomPosition(thisObj.pos, outPtr),
        },
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
            },
            findFast: (thisObj, type) => encodeRoomObjectArray(thisObj.find(type)),
            lookAtFast: (thisObj, x, y) => encodeRoomObjectArray(thisObj.lookAt(x, y)),
            lookAtAreaFast: (thisObj, top, left, bottom, right) => encodeRoomObjectArray(thisObj.lookAtArea(top, left, bottom, right, true)),
            lookForAtFast: (thisObj, type, x, y) => encodeRoomObjectArray(thisObj.lookForAt(type, x, y)),
            lookForAtAreaFast: (thisObj, type, top, left, bottom, right) => encodeRoomObjectArray(thisObj.lookForAtArea(type, top, left, bottom, right, true), type),
        },
        PathFinder: {
            ...PathFinder,
            compileRoomCallback: (opts, roomCostMap) => {
                opts.roomCallback = (roomName) => {
                    const val = roomCostMap[roomName];
                    if (val === true) { return undefined; }
                    if (val === false) { return false; }
                    if (val == null) { return roomCostMap.allowUnspecifiedRooms ? undefined : false; }
                    return val;
                };
            },
            search: (origin, goal, opts) => {
                return PathFinder.search(origin, goal, opts);
            },
        },
        RoomTerrain: {
            get: (thisObj, ...args) => thisObj.get(...args),
            getRawBuffer: (thisObj, memoryView) => memoryView.set(thisObj.getRawBuffer()),
        },
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

    dotNetExports = dotNet.getExports();
    console.log(`#${Game.time}: INIT SUCCESS (${Game.cpu.tickLimit - Game.cpu.getUsed()}ms remaining cpu this tick)`);

    return true;
}

function loop() {
    if (!startupComplete) {
        startupComplete = startup();
        if (startupComplete) {
            dotNetExports.ScreepsDotNet.Program.Init && dotNetExports.ScreepsDotNet.Program.Init();
        }
    }
    if (startupComplete && dotNetExports) {
        dotNet.loop(() => { dotNetExports.ScreepsDotNet.Program.Loop(); });
    }
    accumCpuTime += Game.cpu.getUsed();
}

module.exports.loop = loop;
