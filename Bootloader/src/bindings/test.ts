/// <reference path="../../node_modules/@types/screeps/index.d.ts" />

import { getHeapStatistics } from 'node:v8';
import { ScreepsDotNetExports } from '../common.js';
import { MemoryArea, WasmMemoryManager } from '../memory.js';
import BaseBindings from './base.js';

export default class TestBindings extends BaseBindings {
    private _rawMemory?: string;

    private _gameObj?: Record<string, unknown>;
    private _memoryObj?: Record<string, unknown>;
    private _rawMemoryObj?: Record<string, unknown>;
    private _prototypes?: Record<string, unknown>;

    public init(exports: ScreepsDotNetExports, memory: WasmMemoryManager): void {
        this._prototypes = {};
        this.resetGlobals();
        super.init(exports, memory);
    }

    public loop(): void {
        this.resetGlobals();
        super.loop();
        this._rawMemory = JSON.stringify(this._memoryObj);
    }

    private resetGlobals(): void {
        this._gameObj = {
            creeps: {},
            flags: {},
            powerCreeps: {},
            rooms: {},
            spawns: {},
            structures: {},
            cpu: {
                limit: 500,
                tickLimit: 500,
                bucket: 10000,
                shardLimits: {},
                unlocked: true,
                unlockedTime: 0,
            },
            market: {},
            shard: {
                name: "wasmtest",
                type: "normal",
                ptr: false,
            },
        };
        this._memoryObj = JSON.parse(this._rawMemory ?? '{}');
        this._rawMemoryObj = {

        };
    }

    protected setupImports(): void {
        super.setupImports();
        this.bindingsImport['renew-object'] = this.js_renew_object.bind(this);
        this.bindingsImport['batch-renew-objects'] = this.js_batch_renew_objects.bind(this);
        this.bindingsImport['fetch-object-room-position'] = this.js_fetch_object_room_position.bind(this);
        this.bindingsImport['batch-fetch-object-room-positions'] = this.js_batch_fetch_object_room_positions.bind(this);
        this.bindingsImport['get-object-by-id'] = this.js_get_object_by_id.bind(this);
        this.bindingsImport['get-object-id'] = this.js_get_object_id.bind(this);
        this.imports['object'] = {
            getConstructorOf: (x: object) => Object.getPrototypeOf(x).constructor,
            interpretDateTime: (x: Date) => x.getTime() / 1000,
        };
        this.imports['game'] = {
            checkIn: () => {},
            getGameObj: () => this._gameObj,
            getMemoryObj: () => this._memoryObj,
            getConstantsObj: () => global,
            getRawMemoryObj: () => this._rawMemoryObj,
            getPrototypes: () => this._prototypes,
            cpu: {
                getHeapStatistics: () => {
                    const heapInfo = getHeapStatistics();
                    return { ...heapInfo, externally_allocated_size: heapInfo.external_memory };
                },
                getUsed: () => 0,
            },
            rawMemory: {
                setActiveSegments: (segments: number[]) => {},
            }
        };
    }

    private js_renew_object(jsHandle: number): number {
        return 1;
    }

    private js_batch_renew_objects(jsHandleListPtr: number, count: number): number {
        this._interop.memory!.flush();
        try {
            this._memory!.enterConstrainedRange(jsHandleListPtr, count * 4, MemoryArea.Stack);
            let numSuccess = 0;
            for (let i = 0; i < count; ++i) {
                if (this.js_renew_object(this._memory!.readI32(jsHandleListPtr)) === 0) {
                    ++numSuccess;
                } else {
                    this._memory!.writeI32(jsHandleListPtr, -1);
                }
                jsHandleListPtr += 4;
            }
            return 0;
        } finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private js_fetch_object_room_position(jsHandle: number): number {
        return 0;
    }

    private js_batch_fetch_object_room_positions(jsHandleListPtr: number, count: number, outRoomPosListPtr: number): void {
        this._interop.memory!.flush();
        for (let i = 0; i < count; ++i) {
            this._memory!.writeI32(outRoomPosListPtr, this.js_fetch_object_room_position(this._memory!.readI32(jsHandleListPtr)));
            jsHandleListPtr += 4;
            outRoomPosListPtr += 4;
        }
    }

    private js_get_object_by_id(objectIdPtr: number): number {
        return -1;
    }

    private js_get_object_id(jsHandle: number, outRawObjectIdPtr: number): number {
        return 0;
    }
}
