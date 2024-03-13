/// <reference path="../../node_modules/@types/screeps/index.d.ts" />

import { ScreepsDotNetExports } from '../common.js';
import type { Importable } from '../interop.js';
import { WasmMemoryManager, WasmMemoryView } from '../memory.js';
import { BaseBindings } from './base.js';

declare const global: typeof globalThis;

const CPU_HALT_WHEN_NO_CHECKIN_FOR = 10;

type GamePrototype = {};

type GameConstructor = { readonly prototype: GamePrototype };

type ResourceConstantEx = ResourceConstant | "season";

const RESOURCE_LIST: readonly ResourceConstantEx[] = [
    "energy", "power",
    "H", "O", "U", "L", "K", "Z", "X", "G",
    "silicon", "metal", "biomass", "mist",
    "OH", "ZK", "UL", "UH", "UO", "KH", "KO", "LH", "LO", "ZH", "ZO", "GH", "GO",
    "UH2O", "UHO2", "KH2O", "KHO2", "LH2O", "LHO2", "ZH2O", "ZHO2", "GH2O", "GHO2",
    "XUH2O", "XUHO2", "XKH2O", "XKHO2", "XLH2O", "XLHO2", "XZH2O", "XZHO2", "XGH2O", "XGHO2",
    "ops",
    "utrium_bar", "lemergium_bar", "zynthium_bar", "keanium_bar", "ghodium_melt", "oxidant", "reductant", "purifier", "battery",
    "composite", "crystal", "liquid",
    "wire", "switch", "transistor", "microchip", "circuit", "device",
    "cell", "phlegm", "tissue", "muscle", "organoid", "organism",
    "alloy", "tube", "fixtures", "frame", "hydraulics", "machine",
    "condensate", "concentrate", "extract", "spirit", "emanation", "essence",
    "season",
]; // 85 total

const RESOURCE_TO_ENUM_MAP: Record<ResourceConstantEx, number> = {} as Record<ResourceConstantEx, number>;
{
    let i = 0;
    for (const resourceName of RESOURCE_LIST) {
        RESOURCE_TO_ENUM_MAP[resourceName] = i++;
    }
}

const BODYPART_LIST: readonly BodyPartConstant[] = [
    'move',
    'work',
    'carry',
    'attack',
    'ranged_attack',
    'tough',
    'heal',
    'claim',
]; // 8 total

const BODYPART_TO_ENUM_MAP: Record<BodyPartConstant, number> = {} as Record<BodyPartConstant, number>;
{
    let i = 0;
    for (const bodyPart of BODYPART_LIST) {
        BODYPART_TO_ENUM_MAP[bodyPart] = i++;
    }
}

export class WorldBindings extends BaseBindings {
    private _lastCheckIn: number = 0;

    private _memoryCache?: Memory;

    public init(exports: ScreepsDotNetExports, memoryManager: WasmMemoryManager): void {
        super.init(exports, memoryManager);
        const entrypointFn = exports.screepsdotnet_init_world;
        if (!entrypointFn) {
          this.log(`failed to call 'screepsdotnet_init_world' (not found in wasm exports)`);
          return;
        }
        entrypointFn();
        this._memoryCache = Memory;
        this._memoryCache = (RawMemory as unknown as { _parsed: Memory })._parsed;
        this._lastCheckIn = Game.time;
    }

    public loop(): void {
        super.loop();

        // Memhack
        const _global = global as unknown as { Memory?: Memory };
        delete _global.Memory;
        _global.Memory = this._memoryCache;
        (RawMemory as unknown as { _parsed: Memory })._parsed = this._memoryCache!;

        // Checkin
        const ticksSinceLestCheckIn = Game.time - this._lastCheckIn;
        if (ticksSinceLestCheckIn >= CPU_HALT_WHEN_NO_CHECKIN_FOR) {
            Game.cpu.halt && Game.cpu.halt();
        } else if (ticksSinceLestCheckIn >= CPU_HALT_WHEN_NO_CHECKIN_FOR - 1) {
            this.log(`no checkin for ${ticksSinceLestCheckIn} ticks, halting cpu next tick...`);
        }
    }

    protected setupImports(): void {
        super.setupImports();
        this.bindingsImport.js_renew_object = this.js_renew_object.bind(this);
        this.bindingsImport.js_batch_renew_objects = this.js_batch_renew_objects.bind(this);
        this.bindingsImport.js_fetch_object_room_position = this.js_fetch_object_room_position.bind(this);
        this.bindingsImport.js_batch_fetch_object_room_positions = this.js_batch_fetch_object_room_positions.bind(this);
        this.bindingsImport.js_get_object_by_id = this.js_get_object_by_id.bind(this);
        this.bindingsImport.js_get_object_id = this.js_get_object_id.bind(this);
        const gameConstructors: Record<string, GameConstructor> = {
            StructureContainer,
            StructureController,
            StructureExtension,
            StructureExtractor,
            StructureFactory,
            StructureInvaderCore,
            StructureKeeperLair,
            StructureLab,
            StructureLink,
            StructureNuker,
            StructureObserver,
            StructurePowerBank,
            StructurePowerSpawn,
            StructurePortal,
            StructureRampart,
            StructureRoad,
            StructureSpawn,
            StructureStorage,
            StructureTerminal,
            StructureTower,
            StructureWall,
            OwnedStructure,
            Structure,
            Source,
            Mineral,
            Deposit,
            Creep,
            Flag,
            Resource,
            ConstructionSite,
            Tombstone,
            Ruin,
            RoomObject,
            Room,
            RoomVisual,
            Nuke,
        }
        this.imports['object'] = {
            getConstructorOf: (x: object) => Object.getPrototypeOf(x).constructor,
            interpretDateTime: (x: Date) => x.getTime() / 1000,
        };
        this.imports['game'] = {
            checkIn: () => this._lastCheckIn = Game.time,
            getGameObj: () => Game,
            getMemoryObj: () => this._memoryCache,
            getConstantsObj: () => global,
            getRawMemoryObj: () => RawMemory,
            getPrototypes: () => gameConstructors,
            createRoomPosition: (x, y, roomName) => new RoomPosition(x, y, roomName),
            createCostMatrix: () => new PathFinder.CostMatrix(),
            createRoomVisual: (roomName) => new RoomVisual(roomName),
            game: {
                getObjectById: (id: Id<_HasId>) => Game.getObjectById(id),
                notify: (message: string, groupInterval?: number) => Game.notify(message, groupInterval),
            },
            interShardMemory: {
                getLocal: () => InterShardMemory.getLocal(),
                setLocal: (value: string) => InterShardMemory.setLocal(value),
                getRemote: (shard: string) => InterShardMemory.getRemote(shard),
            },
            map: {
                describeExits: (roomName: string) => Game.map.describeExits(roomName),
                findExit: (fromRoom: string | Room, toRoom: string | Room, opts?: RouteOptions) => Game.map.findExit(fromRoom, toRoom, opts),
                findRoute: (fromRoom: string | Room, toRoom: string | Room, opts?: RouteOptions) => Game.map.findRoute(fromRoom, toRoom, opts),
                getRoomLinearDistance: (roomName1: string, roomName2: string, continuous?: boolean) => Game.map.getRoomLinearDistance(roomName1, roomName2, continuous),
                getRoomTerrain: (roomName: string) => Game.map.getRoomTerrain(roomName),
                getWorldSize: () => Game.map.getWorldSize(),
                getRoomStatus: (roomName: string) => Game.map.getRoomStatus(roomName),
            },
            market: {
                calcTransactionCost: (amount: number, roomName1: string, roomName2: string) => Game.market.calcTransactionCost(amount, roomName1, roomName2),
                cancelOrder: (orderId: string) => Game.market.cancelOrder(orderId),
                changeOrderPrice: (orderId: string, newPrice: number) => Game.market.changeOrderPrice(orderId, newPrice),
                createOrder: (params: {
                    type: ORDER_BUY | ORDER_SELL;
                    resourceType: MarketResourceConstant;
                    price: number;
                    totalAmount: number;
                    roomName?: string;
                }) => Game.market.createOrder(params),
                deal: (orderId: string, amount: number, targetRoomName?: string) => Game.market.deal(orderId, amount, targetRoomName),
                extendOrder: (orderId: string, addAmount: number) => Game.market.extendOrder(orderId, addAmount),
                getAllOrders: (filter?: OrderFilter | ((o: Order) => boolean)) => Game.market.getAllOrders(filter),
                getHistory: (resource?: MarketResourceConstant) => {
                    const result = Game.market.getHistory(resource);
                    return Array.isArray(result) ? result : [];
                },
                getOrderById: (id: string) => Game.market.getOrderById(id),
            },
            cpu: {
                getHeapStatistics: () => Game.cpu.getHeapStatistics && Game.cpu.getHeapStatistics(),
                getUsed: () => Game.cpu.getUsed(),
                halt: () => Game.cpu.halt && Game.cpu.halt(),
                setShardLimits: (limits: CPUShardLimits) => Game.cpu.setShardLimits(limits),
                unlock: () => Game.cpu.unlock(),
                generatePixel: () => Game.cpu.generatePixel(),
            },
            rawMemory: {
                get: () => RawMemory.get(),
                set: (value: string) => RawMemory.set(value),
                setActiveSegments: (ids: number[]) => RawMemory.setActiveSegments(ids),
                setActiveForeignSegment: (username: string | null, id?: number) => RawMemory.setActiveForeignSegment(username, id),
                setDefaultPublicSegment: (id: number | null) => RawMemory.setDefaultPublicSegment(id),
                setPublicSegments: (ids: number[]) => RawMemory.setPublicSegments(ids),
            },
            visual: {
                line: (pos1: RoomPosition, pos2: RoomPosition, style?: MapLineStyle) => Game.map.visual.line(pos1, pos2, style),
                circle: (pos: RoomPosition, style?: MapCircleStyle) => Game.map.visual.circle(pos, style),
                rect: (topLeftPos: RoomPosition, width: number, height: number, style?: MapPolyStyle) => Game.map.visual.rect(topLeftPos, width, height, style),
                poly: (points: RoomPosition[], style?: MapPolyStyle) => Game.map.visual.poly(points, style),
                text: (text: string, pos: RoomPosition, style?: MapTextStyle) => Game.map.visual.text(text, pos, style),
                clear: () => Game.map.visual.clear(),
                getSize: () => Game.map.visual.getSize(),
                export: () => Game.map.visual.export(),
                import: (data: string) => Game.map.visual.import(data),
            },
        };
        const wrappedPrototypes = this.buildWrappedPrototypes(gameConstructors);
        this.imports['game/prototypes/wrapped'] = {
            ...wrappedPrototypes,
            Spawning: this.buildWrappedPrototype(StructureSpawn.Spawning),
            RoomObject: {
                ...wrappedPrototypes.RoomObject,
                getStoreCapacity: (thisObj: { store: Store<ResourceConstant, boolean> }, resourceType: ResourceConstant) => thisObj.store.getCapacity(resourceType),
                getStoreUsedCapacity: (thisObj: { store: Store<ResourceConstant, boolean> }, resourceType: ResourceConstant) => thisObj.store.getUsedCapacity(resourceType),
                getStoreFreeCapacity: (thisObj: { store: Store<ResourceConstant, boolean> }, resourceType: ResourceConstant) => thisObj.store.getFreeCapacity(resourceType),
                getStoreContainedResources: (thisObj: { store: Store<ResourceConstant, boolean> }) => Object.keys(thisObj.store),
                indexStore: (thisObj: { store: Store<ResourceConstant, boolean> }, resourceType: ResourceConstant) => thisObj.store[resourceType],
            },
            CostMatrix: {
                ...this.buildWrappedPrototype(PathFinder.CostMatrix),
                setRect: (thisObj: CostMatrix, minX: number, minY: number, maxX: number, maxY: number, dataView: DataView) => {
                    let i = 0;
                    for (let y = minY; y <= maxY; ++y) {
                        for (let x = minX; x <= maxX; ++x) {
                            thisObj.set(x, y, dataView.getUint8(i));
                            ++i;
                        }   
                    }
                },
            },
            Room: {
                ...wrappedPrototypes.Room,
                createFlag: (thisObj: Room, x: number, y: number, name?: string, color?: ColorConstant, secondaryColor?: ColorConstant) => {
                    const result = thisObj.createFlag(x, y, name, color, secondaryColor);
                    if (typeof result === 'string') {
                        return { name: result, code: 0 };
                    } else {
                        return { code: result };
                    }
                },
                findFast: (thisObj: Room, type: FindConstant, outRoomObjectArrayPtr: number, maxObjectCount: number) =>
                    this.encodeRoomObjectArray(this._memoryManager!.view, thisObj.find(type) as unknown as Record<string, unknown>[], undefined, outRoomObjectArrayPtr, maxObjectCount),
                lookAtFast: (thisObj: Room, x: number, y: number, outRoomObjectArrayPtr: number, maxObjectCount: number) =>
                    this.encodeRoomObjectArray(this._memoryManager!.view, thisObj.lookAt(x, y), undefined, outRoomObjectArrayPtr, maxObjectCount),
                lookAtAreaFast: (thisObj: Room, top: number, left: number, bottom: number, right: number, outRoomObjectArrayPtr: number, maxObjectCount: number) =>
                    this.encodeRoomObjectArray(this._memoryManager!.view, thisObj.lookAtArea(top, left, bottom, right, true), undefined, outRoomObjectArrayPtr, maxObjectCount),
                lookForAtFast: (thisObj: Room, type: LookConstant, x: number, y: number, outRoomObjectArrayPtr: number, maxObjectCount: number) =>
                    this.encodeRoomObjectArray(this._memoryManager!.view, thisObj.lookForAt(type, x, y) as unknown as Record<string, unknown>[], undefined, outRoomObjectArrayPtr, maxObjectCount),
                lookForAtAreaFast: (thisObj: Room, type: LookConstant, top: number, left: number, bottom: number, right: number, outRoomObjectArrayPtr: number, maxObjectCount: number) =>
                    this.encodeRoomObjectArray(this._memoryManager!.view, thisObj.lookForAtArea(type, top, left, bottom, right, true), type, outRoomObjectArrayPtr, maxObjectCount),
            },
            Creep: {
                ...wrappedPrototypes.Creep,
                getEncodedBody: (thisObj: Creep, outPtr: number) => this.encodeCreepBody(this._memoryManager!.view, thisObj.body, outPtr),
            },
            PathFinder: {
                ...(PathFinder as unknown as Importable),
                compileRoomCallback: (opts: PathFinderOpts, roomCostMap: { allowUnspecifiedRooms: boolean, [roomName: string]: boolean | CostMatrix }) => {
                    opts.roomCallback = (roomName: string) => {
                        const val = roomCostMap[roomName];
                        if (typeof val === 'boolean') { return val; }
                        if (val == null) { return roomCostMap.allowUnspecifiedRooms || false; }
                        return val;
                    };
                },
                search: (origin: RoomPosition,
                    goal:
                        | RoomPosition
                        | { pos: RoomPosition; range: number }
                        | Array<RoomPosition | { pos: RoomPosition; range: number }>,
                    opts?: PathFinderOpts
                ) => PathFinder.search(origin, goal, opts),
            },
            RoomTerrain: {
                get: (thisObj: RoomTerrain, x: number, y: number) => thisObj.get(x, y),
                getRawBuffer: (thisObj: RoomTerrain, dataView: DataView) => (thisObj as unknown as { getRawBuffer(destination: Uint8Array): void }).getRawBuffer(new Uint8Array(dataView.buffer, dataView.byteOffset, dataView.byteLength)),
            },
        };
    }

    private js_renew_object(jsHandle: number): number {
        const oldObject = this._interop.getClrTrackedObject(jsHandle);
        if (oldObject == null) { return 1; } // clr tracked object not found (clr object disposed?)
        if (oldObject instanceof Room) {
            const newRoom = Game.rooms[oldObject.name];
            if (newRoom == null) { return 3; } // no longer exists (lost visibilty)
            this._interop.replaceClrTrackedObject(jsHandle, newRoom);
            return 0; // success
        }
        if (oldObject instanceof Flag) {
            const newFlag = Game.flags[oldObject.name];
            if (newFlag == null) { return 3; } // no longer exists (removed or lost visibilty)
            this._interop.replaceClrTrackedObject(jsHandle, newFlag);
            return 0; // success
        }
        const id = (oldObject as { id: string | undefined }).id;
        if (id == null) { return 2; } // unrenewable (not a room object, e.g. unrelated js object)
        const newRoomObject = Game.getObjectById(id);
        if (newRoomObject == null) { return 3; } // no longer exists (destroyed or lost visibilty)
        this._interop.replaceClrTrackedObject(jsHandle, newRoomObject);
        return 0;
    }

    private js_batch_renew_objects(jsHandleListPtr: number, count: number): number {
        const { i32 } = this._memoryManager!.view;
        const baseIdx = jsHandleListPtr >> 2;
        let numSuccess = 0;
        for (let i = 0; i < count; ++i) {
            if (this.js_renew_object(i32[baseIdx + i]) === 0) {
                ++numSuccess;
            } else {
                i32[baseIdx + i] = -1;
            }
        }
        return numSuccess;
    }

    private js_fetch_object_room_position(jsHandle: number): number {
        const roomObject = this._interop.getClrTrackedObject(jsHandle);
        const pos = (roomObject as { pos: RoomPosition | undefined }).pos;
        if (pos == null) { return 0; }
        return (pos as unknown as { __packedPos: number }).__packedPos;
    }

    private js_batch_fetch_object_room_positions(jsHandleListPtr: number, count: number, outRoomPosListPtr: number): void {
        const { i32 } = this._memoryManager!.view;
        const baseJsHandleIdx = jsHandleListPtr >> 2;
        const baseOutRoomPostListIdx = outRoomPosListPtr >> 2;
        for (let i = 0; i < count; ++i) {
            i32[baseOutRoomPostListIdx + i] = this.js_fetch_object_room_position(i32[baseJsHandleIdx + i]);
        }
    }

    private js_get_object_by_id(objectIdPtr: number): number {
        const { u8 } = this._memoryManager!.view;
        let id = '';
        for (let i = 0; i < 24; ++i) {
            const code = u8[objectIdPtr + i];
            if (code === 0) { break; }
            id += String.fromCharCode(code);
        }
        const obj = Game.getObjectById(id);
        if (obj == null) {
            //this.log(`js_get_object_by_id: failed to retrieve '${id}'`);
            return -1;
        }
        return this._interop.getClrTrackingId(obj) ?? this._interop.assignClrTrackingId(obj);
    }

    private js_get_object_id(jsHandle: number, outRawObjectIdPtr: number): number {
        const obj = this._interop.getClrTrackedObject(jsHandle);
        if (obj == null) { return 0; }
        const id = (obj as { id: unknown }).id;
        if (typeof id !== 'string') { return 0; }
        this.copyRawObjectId(this._memoryManager!.view, id, outRawObjectIdPtr);
        return id.length;
    }
    
    private copyRawObjectId(memoryView: WasmMemoryView, id: string, outPtr: number): void {
        const { u8, i32 } = memoryView;
        if (id) {
            const l = id.length;
            for (let j = 0; j < l; ++j) {
                u8[outPtr + j] = id.charCodeAt(j);
            }
            for (let j = l; j < 24; ++j) {
                u8[outPtr + j] = 0;
            }
        } else {
            for (let j = 0; j < 6; ++j) {
                i32[outPtr + j] = 0;
            }
        }
    }

    private encodeRoomObjectArray(memoryView: WasmMemoryView, arr: readonly Record<string, unknown>[], key: string | undefined, outRoomObjectArrayPtr: number, maxObjectCount: number): number {
        const { i32 } = memoryView;
        let numEncoded = 0;
        let nextRoomObjectArrayPtrI32 = outRoomObjectArrayPtr >> 2;
        for (let i = 0; i < Math.min(maxObjectCount, arr.length); ++i) {
            // Lookup object
            let obj = arr[i];
            if (key) {
                obj = obj[key] as Record<string, unknown>;
            }
            if (!(obj instanceof RoomObject) && obj.type) {
                obj = obj[obj.type as string] as Record<string, unknown>;
            }
            if (!(obj instanceof RoomObject)) { continue; }
            
            // Copy metadata
            i32[nextRoomObjectArrayPtrI32++] = Object.getPrototypeOf(obj).constructor.__dotnet_typeId || 0;
            i32[nextRoomObjectArrayPtrI32++] = this._interop.getOrAssignClrTrackingId(obj);

            ++numEncoded;
        }
        return numEncoded;
    }

    private encodeCreepBody(memoryView: WasmMemoryView, body: readonly BodyPartDefinition[], outPtr: number): number {
        const { i32 } = memoryView;
        for (let i = 0; i < body.length; ++i) {
            const { type, hits, boost } = body[i];
            // Encode each body part to a 32 bit int as 4 bytes
            // unused: b3
            // type: 0-8 (4 bits 0-15) b2
            // hits: 0-100 (7 bits 0-127) b1
            // boost: null or 0-85 (7 bits 0-127, 127 means null) b0
            let encodedBodyPart = 0;
            encodedBodyPart |= (BODYPART_TO_ENUM_MAP[type] << 16);
            encodedBodyPart |= (hits << 8);
            encodedBodyPart |= (boost == null ? 127 : RESOURCE_TO_ENUM_MAP[boost as ResourceConstantEx]);
            i32[outPtr >> 2] = encodedBodyPart;
            outPtr += 4;
        }
        return body.length;
    }

    private buildWrappedPrototypes(prototypes: Record<string, GameConstructor>): Record<string, GamePrototype> {
        const wrappedPrototypes: Record<string, GamePrototype> = {};
        for (const prototypeName in prototypes) {
            wrappedPrototypes[prototypeName] = this.buildWrappedPrototype(prototypes[prototypeName]);
        }
        return wrappedPrototypes;
    }
    
    private buildWrappedPrototype(constructor: GameConstructor): GamePrototype {
        /** @type {Record<string, Function>} */
        const wrappedPrototype: Record<string, (thisObj: object, ...args: unknown[]) => unknown> = {};
        const prototype = constructor.prototype;
        const keys = Object.getOwnPropertyNames(prototype);
        for (const key of keys) {
            if (key === 'constructor') { continue; }
            const value = Object.getOwnPropertyDescriptor(prototype, key)?.value;
            if (typeof value !== 'function') { continue; }
            wrappedPrototype[key] = (thisObj: object, ...args: unknown[]) => value.call(thisObj, ...args);
        }
        return wrappedPrototype;
    }
}
