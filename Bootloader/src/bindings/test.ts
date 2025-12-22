/// <reference path="../../node_modules/@types/screeps/index.d.ts" />

import { ScreepsDotNetExports } from '../common.js';
import { WasmMemoryManager } from '../memory.js';
import BaseBindings from './base.js';

export default class TestBindings extends BaseBindings {

    public init(exports: ScreepsDotNetExports, memoryManager: WasmMemoryManager): void {
        super.init(exports, memoryManager);
    }

    public loop(): void {
        super.loop();
    }

    protected setupImports(): void {
        super.setupImports();
        this.bindingsImport.js_renew_object = this.js_renew_object.bind(this);
        this.bindingsImport.js_batch_renew_objects = this.js_batch_renew_objects.bind(this);
        this.bindingsImport.js_fetch_object_room_position = this.js_fetch_object_room_position.bind(this);
        this.bindingsImport.js_batch_fetch_object_room_positions = this.js_batch_fetch_object_room_positions.bind(this);
        this.bindingsImport.js_get_object_by_id = this.js_get_object_by_id.bind(this);
        this.bindingsImport.js_get_object_id = this.js_get_object_id.bind(this);
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
