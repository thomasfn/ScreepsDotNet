const DEFENSIVE_CHECKS = false; // Turn this on to debug memory corruption issues

export class WasmMemoryManager {
    private readonly _memory: WebAssembly.Memory;
    private _viewArrayBuffer?: ArrayBuffer;

    private _u8: Uint8Array;
    private _i8: Int8Array;
    private _u16: Uint16Array;
    private _i16: Int16Array;
    private _u32: Uint32Array;
    private _i32: Int32Array;
    private _f32: Float32Array;
    private _f64: Float64Array;
    private _dataView: DataView;

    private _rangeMin?: number;
    private _rangeMax?: number;
    private readonly _rangeStack: number[] = [];

    constructor(memory: WebAssembly.Memory) {
        this._memory = memory;
        this._viewArrayBuffer = memory.buffer;
        this._u8 = new Uint8Array(memory.buffer);
        this._i8 = new Int8Array(memory.buffer);
        this._u16 = new Uint16Array(memory.buffer);
        this._i16 = new Int16Array(memory.buffer);
        this._u32 = new Uint32Array(memory.buffer);
        this._i32 = new Int32Array(memory.buffer);
        this._f32 = new Float32Array(memory.buffer);
        this._f64 = new Float64Array(memory.buffer);
        this._dataView = new DataView(memory.buffer);
    }

    private checkAlignment(ptr: number, alignment: number): void {
        if (ptr % alignment !== 0) {
            throw new Error(`alignment error - expected ${alignment}, was misaligned by ${ptr % alignment}`);
        }
    }

    private checkConstrainedRange(ptr: number, sz: number): void {
        const min = this._rangeMin ?? 0;
        const max = this._rangeMax ?? this._memory.buffer.byteLength;
        if (ptr < min || (ptr + sz) > max) {
            throw new Error(`constrained range error - expected within ${min}->${max}, got ${ptr}->${ptr + sz}`);
        }
    }

    private checkDetached(): void {
        if (this._memory.buffer !== this._viewArrayBuffer) {
            throw new Error(`view array buffer has changed`);
        }
        if (this._viewArrayBuffer?.detached) {
            throw new Error(`view array buffer is detached`);
        }
    }

    public writeU8(ptr: number, value: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 1);
            this.checkConstrainedRange(ptr, 1);
            this.checkDetached();
        }
        this._u8[ptr] = value;
    }

    public writeI8(ptr: number, value: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 1);
            this.checkConstrainedRange(ptr, 1);
            this.checkDetached();
        }
        this._i8[ptr] = value;
    }

    public writeU16(ptr: number, value: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 2);
            this.checkConstrainedRange(ptr, 2);
            this.checkDetached();
        }
        this._u16[ptr >> 1] = value;
    }

    public writeI16(ptr: number, value: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 2);
            this.checkConstrainedRange(ptr, 2);
            this.checkDetached();
        }
        this._i16[ptr >> 1] = value;
    }

    public writeU32(ptr: number, value: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 4);
            this.checkConstrainedRange(ptr, 4);
            this.checkDetached();
        }
        this._u32[ptr >> 2] = value;
    }

    public writeI32(ptr: number, value: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 4);
            this.checkConstrainedRange(ptr, 4);
            this.checkDetached();
        }
        this._i32[ptr >> 2] = value;
    }

    public writeU64(ptr: number, value: bigint): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 8);
            this.checkConstrainedRange(ptr, 8);
            this.checkDetached();
        }
        this._dataView.setBigUint64(ptr, value, true);
    }

    public writeI64(ptr: number, value: bigint): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 8);
            this.checkConstrainedRange(ptr, 8);
            this.checkDetached();
        }
        this._dataView.setBigInt64(ptr, value, true);
    }

    public writeF32(ptr: number, value: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 4);
            this.checkConstrainedRange(ptr, 4);
            this.checkDetached();
        }
        this._f32[ptr >> 2] = value;
    }

    public writeF64(ptr: number, value: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 8);
            this.checkConstrainedRange(ptr, 8);
            this.checkDetached();
        }
        this._f64[ptr >> 3] = value;
    }

    public writeString(ptr: number, value: string, nullTerminated: boolean): void {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 2);
            this.checkConstrainedRange(ptr, value.length * 2 + (nullTerminated ? 2 : 0));
            this.checkDetached();
        }
        const first = ptr >> 1;
        for (let i = 0; i < value.length; ++i) {
            this._u16[first + i] = value.charCodeAt(i);
        }
        if (nullTerminated) {
            this._u16[first + value.length] = 0;
        }
    }

    public readU8(ptr: number): number {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 1);
            this.checkConstrainedRange(ptr, 1);
            this.checkDetached();
        }
        return this._u8[ptr];
    }

    public readI8(ptr: number): number {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 1);
            this.checkConstrainedRange(ptr, 1);
            this.checkDetached();
        }
        return this._i8[ptr];
    }

    public readU16(ptr: number): number {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 2);
            this.checkConstrainedRange(ptr, 2);
            this.checkDetached();
        }
        return this._u16[ptr >> 1];
    }

    public readI16(ptr: number): number {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 2);
            this.checkConstrainedRange(ptr, 2);
            this.checkDetached();
        }
        return this._i16[ptr >> 1];
    }

    public readU32(ptr: number): number {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 4);
            this.checkConstrainedRange(ptr, 4);
            this.checkDetached();
        }
        return this._u32[ptr >> 2];
    }

    public readI32(ptr: number): number {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 4);
            this.checkConstrainedRange(ptr, 4);
            this.checkDetached();
        }
        return this._i32[ptr >> 2];
    }

    public readU64(ptr: number): bigint {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 8);
            this.checkConstrainedRange(ptr, 8);
            this.checkDetached();
        }
        return this._dataView.getBigUint64(ptr, true);
    }

    public readI64(ptr: number): bigint {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 8);
            this.checkConstrainedRange(ptr, 8);
            this.checkDetached();
        }
        return this._dataView.getBigInt64(ptr, true);
    }

    public readF32(ptr: number): number {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 4);
            this.checkConstrainedRange(ptr, 4);
            this.checkDetached();
        }
        return this._f32[ptr >> 2];
    }

    public readF64(ptr: number): number {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 8);
            this.checkConstrainedRange(ptr, 8);
            this.checkDetached();
        }
        return this._f64[ptr >> 3];
    }

    public readNullTerminatedString(ptr: number): string {
        let result = "";
        let value = this.readU16(ptr);
        while (value !== 0) {
            result += String.fromCharCode(value);
            ptr += 2;
            value = this.readU16(ptr);
        }
        return result;
    }

    public readString(ptr: number, length: number): string {
        if (DEFENSIVE_CHECKS) {
            this.checkAlignment(ptr, 2);
            this.checkConstrainedRange(ptr, length * 2);
            this.checkDetached();
        }
        let result = "";
        const first = ptr >> 1;
        const last = first + length;
        for (let i = first; i < last; ++i) {
            result += String.fromCharCode(this._u16[i]);
        }
        return result;
    }

    public getDataView(ptr: number, sz: number): DataView {
        if (DEFENSIVE_CHECKS) {
            this.checkDetached();
        }
        return new DataView(this._memory.buffer, ptr, sz);
    }

    public getArrayView(ptr: number, sz: number): Uint8Array {
        if (DEFENSIVE_CHECKS) {
            this.checkDetached();
        }
        return new Uint8Array(this._memory.buffer, ptr, sz);
    }

    public memcpy(dst: number, src: number, sz: number): void {
        if (DEFENSIVE_CHECKS) {
            this.checkDetached();
        }
        this._u8.set(this._u8.subarray(src, src + sz), dst);
    }

    public enterConstrainedRange(ptr: number, sz: number): void {
        if (!DEFENSIVE_CHECKS) { return; }
        if (this._rangeMin != null) {
            this._rangeStack.push(this._rangeMin);
            this._rangeStack.push(this._rangeMax!);
        }
        this._rangeMin = ptr;
        this._rangeMax = ptr + sz;
    }

    public exitConstrainedRange(): void {
        if (!DEFENSIVE_CHECKS) { return; }
        this._rangeMax = this._rangeStack.pop();
        this._rangeMin = this._rangeStack.pop();
    }

    public flush(): void {
        if (this._memory.buffer === this._viewArrayBuffer) { return; }
        this._viewArrayBuffer = this._memory.buffer;
        this._u8 = new Uint8Array(this._memory.buffer);
        this._i8 = new Int8Array(this._memory.buffer);
        this._u16 = new Uint16Array(this._memory.buffer);
        this._i16 = new Int16Array(this._memory.buffer);
        this._u32 = new Uint32Array(this._memory.buffer);
        this._i32 = new Int32Array(this._memory.buffer);
        this._f32 = new Float32Array(this._memory.buffer);
        this._f64 = new Float64Array(this._memory.buffer);
        this._dataView = new DataView(this._memory.buffer);
    }
}
