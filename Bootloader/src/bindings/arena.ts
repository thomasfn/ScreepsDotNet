/// <reference path="../../node_modules/@types/screeps-arena/index.d.ts" />

import { ScreepsDotNetExports } from '../common.js';
import type { ImportTable } from '../interop.js';
import { WasmMemoryManager, WasmMemoryView } from '../memory.js';
import { BaseBindings } from './base.js';

import type { RoomPosition, GameObject, Store } from 'game/prototypes';
import type { FindPathResult, FindPathOpts } from 'game/path-finder';

// Note: We're assuming these imports are prepended to the final bootloader.mjs file by rollup and so they're essentially available globally for us to use
declare const utils: typeof import('game/utils');
declare const prototypes: typeof import('game/prototypes');
declare const constants: typeof import('game/constants');
declare const pathFinder: typeof import('game/path-finder');
declare const visual: typeof import('game/visual');

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
            searchPath: (origin: number, goalsPtr: number, goalsCnt: number, options?: FindPathOpts) => {
                const { i32 } = this._memoryManager!.view;
                let goal:
                    | RoomPosition
                    | { pos: RoomPosition; range: number }
                    | Array<RoomPosition | { pos: RoomPosition; range: number }>;
                let goalsPtrI32 = goalsPtr >> 2;
                if (goalsCnt == 1) {
                    const r = i32[goalsPtrI32 + 2];
                    if (r === 0) {
                        goal = { x: i32[goalsPtrI32 + 0], y: i32[goalsPtrI32 + 1] };
                    } else {
                        goal = { pos: { x: i32[goalsPtrI32 + 0], y: i32[goalsPtrI32 + 1] }, range: r };
                    }
                } else {
                    goal = [];
                    goal.length = goalsCnt;
                    for (let i = 0; i < goalsCnt; ++i) {
                        const r = i32[goalsPtrI32 + 2];
                        if (r === 0) {
                            goal[i] = { x: i32[goalsPtrI32 + 0], y: i32[goalsPtrI32 + 1] };
                        } else {
                            goal[i] = { pos: { x: i32[goalsPtrI32 + 0], y: i32[goalsPtrI32 + 1] }, range: r };
                        }
                        goalsPtrI32 += 3;
                    }
                }
                const originPos: RoomPosition = { x: origin >> 16, y: origin & 0xffff };
                return pathFinder.searchPath(originPos, goal, options);
            },
            decodePath: (resultObj: FindPathResult, outPtr: number) => this.copyPath(this._memoryManager!.view, resultObj.path, outPtr),
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
        this.imports['game/visual'] = {
            Visual: this.buildWrappedPrototype(visual.Visual),
            createVisual: (layer, persistent) => new visual.Visual(layer, persistent),
        };
        this.imports['game'] = {
            getUtils: () => utils,
            getPrototypes: () => prototypes,
        };
        const wrappedPrototypes = this.buildWrappedPrototypes(prototypes as unknown as Record<string, _Constructor<unknown>>);
        this.imports['game/prototypes/wrapped'] = {
            ...wrappedPrototypes,
            GameObject: {
                ...wrappedPrototypes.GameObject,
                findPathTo: (thisObj: GameObject, pos: RoomPosition, opts: FindPathOpts | undefined, outPtr: number) => {
                    const result = thisObj.findPathTo(pos, opts != null ? opts : undefined);
                    if (!result) { return 0; }
                    return this.copyPath(this._memoryManager!.view, result, outPtr);
                },
            },
            Store: {
                getCapacity: (thisObj: Store<'energy'>, resourceType?: 'energy') => thisObj.getCapacity(resourceType),
                getUsedCapacity: (thisObj: Store<'energy'>, resourceType?: 'energy') => thisObj.getUsedCapacity(resourceType),
                getFreeCapacity: (thisObj: Store<'energy'>, resourceType?: 'energy') => thisObj.getFreeCapacity(resourceType),
            },
            Creep: {
                ...wrappedPrototypes.Creep,
                getEncodedBody: (thisObj: Creep, outPtr: number) => this.encodeCreepBody(this._memoryManager!.view, thisObj.body, outPtr),
            },
        };
    }

    private encodeCreepBody(memoryView: WasmMemoryView, body: readonly BodyPartDefinition[], outPtr: number): number {
        const { i16 } = memoryView;
        let ptrI16 = outPtr >> 1;
        for (let i = 0; i < body.length; ++i) {
            const { type, hits } = body[i];
            // Encode each body part to a 16 bit int as 2 bytes
            // type: 0-8 (4 bits 0-15) b1
            // hits: 0-100 (7 bits 0-127) b0
            let encodedBodyPart = 0;
            encodedBodyPart |= (BODYPART_TO_ENUM_MAP[type] << 8);
            encodedBodyPart |= hits;
            i16[ptrI16] = encodedBodyPart;
            ++ptrI16;
        }
        return body.length;
    }

    private copyPath(memoryView: WasmMemoryView, path: readonly RoomPosition[], outPtr: number): number {
        const { i32 } = memoryView;
        let ptrI32 = outPtr >> 2;
        for (let i = 0; i < path.length; ++i) {
            i32[ptrI32 + 0] = path[i].x;
            i32[ptrI32 + 1] = path[i].y;
            ptrI32 += 2;
        }
        return path.length;
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
