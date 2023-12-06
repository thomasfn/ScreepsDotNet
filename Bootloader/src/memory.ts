export interface WasmMemoryView {
    readonly u8: Uint8Array;
    readonly i8: Int8Array;
    readonly u16: Uint16Array;
    readonly i16: Int16Array;
    readonly u32: Uint32Array;
    readonly i32: Int32Array;
    readonly f32: Float32Array;
    readonly f64: Float64Array;
}

export function createWasmMemoryView(memory: WebAssembly.Memory): WasmMemoryView {
    return {
        u8: new Uint8Array(memory.buffer),
        i8: new Int8Array(memory.buffer),
        u16: new Uint16Array(memory.buffer),
        i16: new Int16Array(memory.buffer),
        u32: new Uint32Array(memory.buffer),
        i32: new Int32Array(memory.buffer),
        f32: new Float32Array(memory.buffer),
        f64: new Float64Array(memory.buffer),
    };
}
