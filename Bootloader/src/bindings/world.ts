import { ScreepsDotNetExports } from '../common.js';
import type { Importable } from '../interop.js';
import { BaseBindings } from './base.js';

declare const global: typeof globalThis;

const PACKET_SIZE_IN_BYTES = 56;
const PACKET_FLAG_MY = (1 << 0);

type GamePrototype = {};

type GameConstructor = { readonly prototype: GamePrototype };

const GAME_CONSTRUCTORS: Record<string, GameConstructor> = {
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
};

export class WorldBindings extends BaseBindings {
    private _copyBufferSize: number = 0;
    private _copyBufferPtr: number | null = null;
    private _copyBufferHeadI32: number = 0;

    private _lastCheckIn: number = 0;

    private _memoryCache?: Memory;

    public init(exports: ScreepsDotNetExports): void {
        super.init(exports);
        const entrypointFn = exports.screepsdotnet_init_world;
        if (!entrypointFn) {
          this.log(`failed to call 'screepsdotnet_init_world' (not found in wasm exports)`);
          return;
        }
        this._copyBufferSize = 1024 * 1024;
        this._copyBufferPtr = entrypointFn(this._copyBufferSize);
        this._memoryCache = Memory;
        this._memoryCache = (RawMemory as unknown as { _parsed: Memory })._parsed;
    }

    public loop(): void {
        super.loop();
        const _global = global as unknown as { Memory?: Memory };
        delete _global.Memory;
        _global.Memory = this._memoryCache;
        (RawMemory as unknown as { _parsed: Memory })._parsed = this._memoryCache!;
    }

    protected setupImports(): void {
        super.setupImports();
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
            getPrototypes: () => GAME_CONSTRUCTORS,
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
        const wrappedPrototypes = this.buildWrappedPrototypes(GAME_CONSTRUCTORS);
        this.imports['game/prototypes/wrapped'] = {
            ...wrappedPrototypes,
            RoomObject: {
                ...wrappedPrototypes.RoomObject,
                getEncodedRoomPosition: (thisObj: RoomObject, outPtr: number) => this.encodeRoomPosition(thisObj.pos, outPtr),
            },
            Spawning: this.buildWrappedPrototype(StructureSpawn.Spawning),
            Store: {
                getCapacity: (thisObj: Store<ResourceConstant, boolean>, resourceType: ResourceConstant) => thisObj.getCapacity(resourceType),
                getUsedCapacity: (thisObj: Store<ResourceConstant, boolean>, resourceType: ResourceConstant) => thisObj.getUsedCapacity(resourceType),
                getFreeCapacity: (thisObj: Store<ResourceConstant, boolean>, resourceType: ResourceConstant) => thisObj.getFreeCapacity(resourceType),
            },
            CostMatrix: {
                ...this.buildWrappedPrototype(PathFinder.CostMatrix),
                setRect: (thisObj: CostMatrix, minX: number, minY: number, maxX: number, maxY: number, dataView: DataView) => {
                    const w = (maxX - minX) + 1;
                    const h = (maxY - minY) + 1;
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
                findFast: (thisObj: Room, type: FindConstant) => this.encodeRoomObjectArray(thisObj.find(type) as unknown as Record<string, unknown>[]),
                lookAtFast: (thisObj: Room, x: number, y: number) => this.encodeRoomObjectArray(thisObj.lookAt(x, y)),
                lookAtAreaFast: (thisObj: Room, top: number, left: number, bottom: number, right: number) => this.encodeRoomObjectArray(thisObj.lookAtArea(top, left, bottom, right, true)),
                lookForAtFast: (thisObj: Room, type: LookConstant, x: number, y: number) => this.encodeRoomObjectArray(thisObj.lookForAt(type, x, y) as unknown as Record<string, unknown>[]),
                lookForAtAreaFast: (thisObj: Room, type: LookConstant, top: number, left: number, bottom: number, right: number) => this.encodeRoomObjectArray(thisObj.lookForAtArea(type, top, left, bottom, right, true), type),
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
                get: (thisObj, ...args) => thisObj.get(...args),
                getRawBuffer: (thisObj, memoryView) => memoryView.set(thisObj.getRawBuffer()),
            },
        };
    }

    private encodeRoomPosition({x, y, roomName}: RoomPosition, outPtr: number) {
        // X:i32(4), y:i32(4), roomName:6i8(6), padding:2i8(2) total=16
        const { u8, i32 } = this._memoryView!;
        i32[(outPtr + 0) >>> 2] = x;
        i32[(outPtr + 4) >>> 2] = y;
        if (roomName.length < 4) {
            u8[outPtr + 8] = 0;
        } else {
            u8[outPtr + 8] = roomName.charCodeAt(0);
            u8[outPtr + 9] = roomName.charCodeAt(1);
            u8[outPtr + 10] = roomName.charCodeAt(2);
            u8[outPtr + 11] = roomName.charCodeAt(3);
            u8[outPtr + 12] = roomName.length >= 5 ? roomName.charCodeAt(4) : 0;
            u8[outPtr + 13] = roomName.length >= 6 ? roomName.charCodeAt(5) : 0;
            u8[outPtr + 14] = 0;
            u8[outPtr + 15] = 0;
        }
    }
    
    private encodeObjectId(id: string, outPtr: number) {
        const { u8, i32 } = this._memoryView!;
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
                i32[this._copyBufferHeadI32 + j] = 0;
            }
        }
    }

    private encodeRoomObjectArray(arr: Record<string, unknown>[], key?: string) {
        const { u8, i32 } = this._memoryView!;
        if (this._copyBufferPtr == null) { throw new Error(`encodeRoomObjectArray failed as copy buffer was not allocated`); }
        // room object packet (54b):
        // - id (0..24)
        // - type id (24..28)
        // - flags (28..32)
        // - hits/progress/energy/mineralAmount (32..36)
        // - hitsMax/progressTotal/energyCapacity/density (36..40)
        // - roomPosition (40..54)

        let copyBufferHead = this._copyBufferPtr;
        let numEncoded = 0;
        for (let i = 0; i < arr.length; ++i) {
            if (copyBufferHead + PACKET_SIZE_IN_BYTES > this._copyBufferPtr + this._copyBufferSize) {
                console.log(`BUFFER OVERFLOW in encodeRoomObjectArray (trying to encode ${arr.length} room objects but only space for ${(this._copyBufferSize / PACKET_SIZE_IN_BYTES)|0})`);
                break;
            }
            const copyBufferHeadI32 = copyBufferHead >>> 2;
            let obj = arr[i];
            if (key) {
                obj = obj[key] as Record<string, unknown>;
            }
            if (!(obj instanceof RoomObject) && obj.type) {
                obj = obj[obj.type as string] as Record<string, unknown>;
            }
            if (!(obj instanceof RoomObject)) { continue; }
            ++numEncoded;
            this.encodeObjectId(obj.id as string, copyBufferHead);
            i32[copyBufferHeadI32 + 6] = Object.getPrototypeOf(obj).constructor.__dotnet_typeId || 0;
            i32[copyBufferHeadI32 + 7] = obj.my ? PACKET_FLAG_MY : 0;
            i32[copyBufferHeadI32 + 8] = (obj.hits || obj.progress || obj.energy || obj.mineralAmount || 0) as number;
            i32[copyBufferHeadI32 + 9] = (obj.hitsMax || obj.progressTotal || obj.energyCapacity || obj.density || 0) as number;
            this.encodeRoomPosition(obj.pos, copyBufferHead + 40);
            copyBufferHead += PACKET_SIZE_IN_BYTES;
        }
        return numEncoded;
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
