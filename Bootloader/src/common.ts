export interface ScreepsDotNetExports extends WebAssembly.Exports {
    memory: WebAssembly.Memory;
    _initialize(): unknown;
    malloc(sz: number): number;
    free(ptr: number): void;
    ['screeps:screepsdotnet/botapi#init'](): void;
    ['screeps:screepsdotnet/botapi#loop'](): void;
    ['screeps:screepsdotnet/botapi#invoke-room-callback'](roomCoordX: number, roomCoordY: number): number;
    ['screeps:screepsdotnet/botapi#invoke-cost-callback'](roomCoordX: number, roomCoordY: number, costMatrixJsHandle: number): number;
    ['screeps:screepsdotnet/botapi#invoke-route-callback'](roomCoordX: number, roomCoordY: number, fromRoomCoordX: number, fromRoomCoordY: number): number;
}
