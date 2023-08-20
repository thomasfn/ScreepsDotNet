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
    dotNet = new bootloader.DotNet(bundle, 'world');
    dotNet.setPerfFn(function() { return getCpuTime() * 1000000; });
    dotNet.setVerboseLogging(false);
    function fixupArray(obj) {
        if (obj == null) { return null; }
        if (Array.isArray(obj)) { return obj; }
        const arr = new Array(obj.length || 0);
        for (let i = 0; i < arr.length; ++i) {
            arr[i] = obj[i];
        }
        return arr;
    }
    const wChr = 'W'.charCodeAt(0);
    const eChr = 'E'.charCodeAt(0);
    const sChr = 'S'.charCodeAt(0);
    const nChr = 'N'.charCodeAt(0);
    const _0Chr = '0'.charCodeAt(0);
    const _9Chr = '9'.charCodeAt(0);
    function encodeRoomPosition({x, y, roomName}) {
        // bit 0-0   (1): sim room
        // bit 1-7   (7): room x
        // bit 8-14  (7): room y
        // bit 15-20 (6): local x
        // bit 21-26 (6): local y
        let result = 0;
        if (roomName == "sim") {
            result |= 1;
        } else {
            let roomX, roomY;
            let ptr = 0;
            let _0, _1, _2;
            _0 = roomName.charCodeAt(ptr), _1 = roomName.charCodeAt(ptr + 1), _2 = roomName.charCodeAt(ptr + 2);
            if (_0 == wChr) {
                if (roomName.length > ptr + 2 && _2 >= _0Chr && _2 <= _9Chr) {
                    roomX = -((_1 - _0Chr) * 10 + (_2 - _0Chr) + 1);
                    ptr += 3;
                } else {
                    roomX = -(_1 - _0Chr + 1);
                    ptr += 2;
                }
            } else if (_0 == eChr) {
                if (roomName.length > ptr + 2 && _2 >= _0Chr && _2 <= _9Chr) {
                    roomX = (_1 - _0Chr) * 10 + (_2 - _0Chr);
                    ptr += 3;
                } else {
                    roomX = _1 - _0Chr;
                    ptr += 2;
                }
            } else {
                throw new Error(`Room name '${roomName}' does not follow standard pattern`);
            }

            _0 = roomName.charCodeAt(ptr), _1 = roomName.charCodeAt(ptr + 1), _2 = roomName.charCodeAt(ptr + 2);
            if (_0 == sChr) {
                if (roomName.length > ptr + 2 && _2 >= _0Chr && _2 <= _9Chr) {
                    roomY = -((_1 - _0Chr) * 10 + (_2 - _0Chr) + 1);
                    ptr += 3;
                } else {
                    roomY = -(_1 - _0Chr + 1);
                    ptr += 2;
                }
            } else if (_0 == nChr) {
                if (roomName.length > ptr + 2 && _2 >= _0Chr && _2 <= _9Chr) {
                    roomY = (_1 - _0Chr) * 10 + (_2 - _0Chr);
                    ptr += 3;
                } else {
                    roomY = _1 - _0Chr;
                    ptr += 2;
                }
            } else {
                throw new Error(`Room name '${roomName}' does not follow standard pattern`);
            }

            result |= (roomX + 64) << 1;
            result |= (roomY + 64) << 8;
        }
        result |= x << 15;
        result |= y << 21;
        return result;
    }
    const copyBuffer = new ArrayBuffer(1024 * 1024);
    const copyBufferU8 = new Uint8Array(copyBuffer);
    const copyBufferI32 = new Int32Array(copyBuffer);
    let copyBufferHead = 0;

    /** @param buffer {Uint8Array} */
    function encodeRoomObjectArray(arr) {
        // room object packet (40b):
        // - id (32b)
        // - type id (4b)
        // - encoded position (4b)
        copyBufferHead = 0;
        for (let i = 0; i < arr.length; ++i) {
            if (copyBufferHead + 40 > copyBuffer.byteLength) {
                console.log(`BUFFER OVERFLOW in encodeRoomObjectArray (trying to encode ${arr.length} room objects but only space for ${(copyBuffer.byteLength / 40)|0})`);
                break;
            }
            const obj = arr[i];
            const id = obj.id;
            for (let j = 0; j < 32; ++j) {
                copyBufferU8[copyBufferHead + j] = id ? id.charCodeAt(j) : 0;
            }
            copyBufferHead += 32;
            copyBufferI32[copyBufferHead >> 2] = Object.getPrototypeOf(obj).constructor.__dotnet_typeId || 0;
            copyBufferHead += 4;
            copyBufferI32[copyBufferHead >> 2] = obj.pos ? encodeRoomPosition(obj.pos) : 0;
            copyBufferHead += 4;
        }
    }
    dotNet.setModuleImports('copybuffer', {
        getMaxSize: () => copyBuffer.byteLength,
        read: (memoryView) => {
            memoryView.set(copyBufferU8);
            return copyBufferHead;
        },
        write: (memoryView) => {
            memoryView.copyTo(copyBufferU8);
            memoryView.dispose();
            copyBufferHead = Math.min(copyBuffer.byteLength, memoryView.byteLength);
            return copyBufferHead;
        },
    });
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
            getEncodedRoomPosition: (thisObj) => encodeRoomPosition(thisObj.pos),
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
            findFast: (thisObj, ...args) => {
                const result = thisObj.find(...args);
                encodeRoomObjectArray(result);
                return result.length;
            },
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
    }
    if (startupComplete && dotNetExports) {
        dotNet.loop(() => { dotNetExports.ScreepsDotNet.Program.Loop(); });
    }
    accumCpuTime += Game.cpu.getUsed();
}

module.exports.loop = loop;
