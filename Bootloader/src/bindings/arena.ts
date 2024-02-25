/// <reference path="../../node_modules/@types/screeps-arena/index.d.ts" />

import { ScreepsDotNetExports } from '../common.js';
import type { ImportTable } from '../interop.js';
import { WasmMemoryManager, WasmMemoryView } from '../memory.js';
import { BaseBindings } from './base.js';

import type { RoomPosition } from 'game/prototypes';

// Missing export in ts definitions
declare module "game/prototypes" {
    export const GameObject: _Constructor<GameObject>;
}

// Note: We're assuming these imports are prepended to the final bootloader.mjs file by rollup and so they're essentially available globally for us to use
declare const utils: typeof import('game/utils');
declare const prototypes: typeof import('game/prototypes');
declare const constants: typeof import('game/constants');
declare const pathFinder: typeof import('game/path-finder');
//declare const visual: typeof import('game/visual');

type GamePrototype = {};

const RESOURCE_LIST: string[] = [
    "energy",
    "score",
    "score_x",
    "score_y",
    "score_z",
]; // 5 total

const RESOURCE_TO_ENUM_MAP: Record<string, number> = {} as Record<string, number>;
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
    'heal',
    'tough',
]; // 7 total

const BODYPART_TO_ENUM_MAP: Record<BodyPartConstant, number> = {} as Record<BodyPartConstant, number>;
{
    let i = 0;
    for (const bodyPart of BODYPART_LIST) {
        BODYPART_TO_ENUM_MAP[bodyPart] = i++;
    }
}

export class ArenaBindings extends BaseBindings {
    private _lastCheckIn: number = 0;

    public init(exports: ScreepsDotNetExports, memoryManager: WasmMemoryManager): void {
        super.init(exports, memoryManager);
    }

    public loop(): void {
        super.loop();
    }

    protected setupImports(): void {
        super.setupImports();
        this.bindingsImport.js_renew_object = () => {};
        this.bindingsImport.js_batch_renew_objects = () => {};
        this.bindingsImport.js_fetch_object_room_position = () => {};
        this.bindingsImport.js_batch_fetch_object_room_positions = () => {};
        this.bindingsImport.js_get_object_by_id = () => {};
        this.imports['object'] = {
            getConstructorOf: (x: object) => Object.getPrototypeOf(x).constructor,
            interpretDateTime: (x: Date) => x.getTime() / 1000,
        };
        this.imports['game/utils'] = {
            ...utils,
            getTerrain: (minX: number, minY: number, maxX: number, maxY: number, outMemoryView: DataView) => {
                const pos: RoomPosition = { x: 0, y: 0 };
                let i = 0;
                for (let y = minY; y <= maxY; ++y) {
                    pos.y = y;
                    for (let x = minX; x <= maxX; ++x) {
                        pos.x = x;
                        outMemoryView.setInt8(i, utils.getTerrainAt(pos));
                        ++i;
                    }   
                }
            },
        };
        this.imports['game/prototypes'] = prototypes as unknown as ImportTable;
        this.imports['game/constants'] = {
            get: () => constants,
        };
        this.imports['game/pathFinder'] = {
            ...pathFinder,
            CostMatrix: {
                ...this.buildWrappedPrototype(pathFinder.CostMatrix),
                setRect: (thisObj: typeof pathFinder.CostMatrix, minX: number, minY: number, maxX: number, maxY: number, memoryView: DataView) => {
                    let i = 0;
                    for (let y = minY; y <= maxY; ++y) {
                        for (let x = minX; x <= maxX; ++x) {
                            thisObj.set(x, y, memoryView.getInt8(i));
                            ++i;
                        }   
                    }
                },
            },
            createCostMatrix: () => new pathFinder.CostMatrix(),
        };
        // this.imports['game/visual'] = {
        //     Visual: this.buildWrappedPrototype(visual.Visual),
        //     createVisual: (layer, persistent) => new visual.Visual(layer, persistent),
        // };
        this.imports['game'] = {
            getUtils: () => utils,
            getPrototypes: () => prototypes,
        };
        const wrappedPrototypes = this.buildWrappedPrototypes(prototypes as unknown as Record<string, _Constructor<unknown>>);
        this.imports['game/prototypes/wrapped'] = {
            ...wrappedPrototypes,
            Store: {
                getCapacity: (thisObj, resourceType) => thisObj.getCapacity(resourceType),
                getUsedCapacity: (thisObj, resourceType) => thisObj.getUsedCapacity(resourceType),
                getFreeCapacity: (thisObj, resourceType) => thisObj.getFreeCapacity(resourceType),
            },
        };
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

    private encodeRoomObjectArray(memoryView: WasmMemoryView, arr: readonly Record<string, unknown>[], key: string | undefined, outRawObjectIdPtr: number, outRoomObjectMetadataPtr: number, maxObjectCount: number): number {
        const { i32 } = memoryView;
        let numEncoded = 0;
        let nextRawObjectIdPtr = outRawObjectIdPtr;
        let nextRoomObjectMetadataPtr = outRoomObjectMetadataPtr;
        for (let i = 0; i < Math.min(maxObjectCount, arr.length); ++i) {
            // Lookup object
            let obj = arr[i];
            if (key) {
                obj = obj[key] as Record<string, unknown>;
            }
            if (!(obj instanceof prototypes.GameObject) && obj.type) {
                obj = obj[obj.type as string] as Record<string, unknown>;
            }
            if (!(obj instanceof prototypes.GameObject)) { continue; }

            // Copy id
            this.copyRawObjectId(memoryView, obj.id as string, nextRawObjectIdPtr);
            nextRawObjectIdPtr += 24;
            
            // Copy metadata
            i32[(nextRoomObjectMetadataPtr + 0) >> 2] = Object.getPrototypeOf(obj).constructor.__dotnet_typeId || 0;
            // For now do not assign clr tracking ids here because if we get here before clr has a chance to renew the objects, we memory leak as the old reference is lost and the object sticks around in the tracking list
            // i32[(nextRoomObjectMetadataPtr + 4) >> 2] = this._interop.getClrTrackingId(obj) ?? this._interop.assignClrTrackingId(obj);
            i32[(nextRoomObjectMetadataPtr + 4) >> 2] = -1;
            nextRoomObjectMetadataPtr += 8;

            ++numEncoded;
        }
        return numEncoded;
    }

    private encodeCreepBody(memoryView: WasmMemoryView, body: readonly BodyPartDefinition[], outPtr: number): number {
        const { i16 } = memoryView;
        for (let i = 0; i < body.length; ++i) {
            const { type, hits } = body[i];
            // Encode each body part to a 16 bit int as 2 bytes
            // type: 0-8 (4 bits 0-15) b1
            // hits: 0-100 (7 bits 0-127) b0
            let encodedBodyPart = 0;
            encodedBodyPart |= (BODYPART_TO_ENUM_MAP[type] << 8);
            encodedBodyPart |= hits;
            i16[outPtr >> 1] = encodedBodyPart;
            outPtr += 2;
        }
        return body.length;
    }

    private buildWrappedPrototypes(prototypes: Record<string, _Constructor<unknown>>): Record<string, GamePrototype> {
        const wrappedPrototypes: Record<string, GamePrototype> = {};
        for (const prototypeName in prototypes) {
            wrappedPrototypes[prototypeName] = this.buildWrappedPrototype(prototypes[prototypeName]);
        }
        return wrappedPrototypes;
    }
    
    private buildWrappedPrototype(constructor: _Constructor<unknown>): GamePrototype {
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
