export interface ScreepsDotNetExports extends WebAssembly.Exports {
    memory: WebAssembly.Memory;
    _start(): unknown;
    malloc(sz: number): number;
    free(ptr: number): void;
    screepsdotnet_init(): void;
    screepsdotnet_init_world(copyBufferSize: number): number;
    screepsdotnet_loop(): void;
}
