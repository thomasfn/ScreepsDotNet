export interface ScreepsDotNetExports extends WebAssembly.Exports {
    memory: WebAssembly.Memory;
    _start(): unknown;
    _initialize(): unknown;
    malloc(sz: number): number;
    free(ptr: number): void;
    screepsdotnet_init(): void;
    screepsdotnet_init_world(): void;
    screepsdotnet_loop(): void;
    screepsdotnet_invoke_room_callback(roomCoordX: number, roomCoordY: number): number;
    screepsdotnet_invoke_cost_callback(roomCoordX: number, roomCoordY: number, costMatrixJsHandle: number): number;
    screepsdotnet_invoke_route_callback(roomCoordX: number, roomCoordY: number, fromRoomCoordX: number, fromRoomCoordY: number): number;
}
