export interface WasmMemoryView {
    readonly u8: Uint8Array;
    readonly i8: Int8Array;
    readonly u16: Uint16Array;
    readonly i16: Int16Array;
    readonly u32: Uint32Array;
    readonly i32: Int32Array;
    readonly f32: Float32Array;
    readonly f64: Float64Array;
    readonly dataView: DataView;
    flush(): void;
}

class WasmMemoryViewImpl implements WasmMemoryView {
    private readonly _memory: WebAssembly.Memory;
    private _viewArrayBuffer?: ArrayBuffer;

    public u8: Uint8Array;
    public i8: Int8Array;
    public u16: Uint16Array;
    public i16: Int16Array;
    public u32: Uint32Array;
    public i32: Int32Array;
    public f32: Float32Array;
    public f64: Float64Array;
    public dataView: DataView;

    constructor(memory: WebAssembly.Memory) {
        this._memory = memory;
        this._viewArrayBuffer = memory.buffer;
        this.u8 = new Uint8Array(memory.buffer);
        this.i8 = new Int8Array(memory.buffer);
        this.u16 = new Uint16Array(memory.buffer);
        this.i16 = new Int16Array(memory.buffer);
        this.u32 = new Uint32Array(memory.buffer);
        this.i32 = new Int32Array(memory.buffer);
        this.f32 = new Float32Array(memory.buffer);
        this.f64 = new Float64Array(memory.buffer);
        this.dataView = new DataView(memory.buffer);
    }

    public flush(): void {
        if (this._memory.buffer === this._viewArrayBuffer) { return; }
        this._viewArrayBuffer = this._memory.buffer;
        this.u8 = new Uint8Array(this._memory.buffer);
        this.i8 = new Int8Array(this._memory.buffer);
        this.u16 = new Uint16Array(this._memory.buffer);
        this.i16 = new Int16Array(this._memory.buffer);
        this.u32 = new Uint32Array(this._memory.buffer);
        this.i32 = new Int32Array(this._memory.buffer);
        this.f32 = new Float32Array(this._memory.buffer);
        this.f64 = new Float64Array(this._memory.buffer);
        this.dataView = new DataView(this._memory.buffer);
    }
}

export class WasmMemoryManager {
    private readonly _view: WasmMemoryView;
    
    public get view() {
        this._view.flush();
        return this._view;
    }

    constructor(memory: WebAssembly.Memory) {
        this._view = new WasmMemoryViewImpl(memory);
    }
}
