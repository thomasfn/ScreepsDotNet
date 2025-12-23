/// <reference path="../../node_modules/@types/screeps/index.d.ts" />

import { getHeapStatistics } from 'node:v8';
import { ScreepsDotNetExports } from '../common.js';
import { WasmMemoryManager } from '../memory.js';
import BaseBindings from './base.js';

export default class TestBindings extends BaseBindings {
    private _memory?: string;

    private _gameObj?: Record<string, unknown>;
    private _memoryObj?: Record<string, unknown>;
    private _rawMemoryObj?: Record<string, unknown>;
    private _prototypes?: Record<string, unknown>;

    public init(exports: ScreepsDotNetExports, memoryManager: WasmMemoryManager): void {
        this._prototypes = {};
        this.resetGlobals();
        super.init(exports, memoryManager);
    }

    public loop(): void {
        this.resetGlobals();
        super.loop();
        this._memory = JSON.stringify(this._memoryObj);
    }

    private resetGlobals(): void {
        this._gameObj = {
            creeps: {},
            flags: {},
            powerCreeps: {},
            rooms: {},
            spawns: {},
            structures: {},
            cpu: {},
            market: {},
        };
        this._memoryObj = JSON.parse(this._memory ?? '{}');
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
            },
        };
        
    }

    private js_renew_object(jsHandle: number): number {
        return 1;
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
        return 0;
    }

    private js_fetch_object_room_position(jsHandle: number): number {
        return 0;
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
        return -1;
    }

    private js_get_object_id(jsHandle: number, outRawObjectIdPtr: number): number {
        return 0;
    }
}
