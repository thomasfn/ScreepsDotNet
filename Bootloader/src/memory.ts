import { ScreepsDotNetExports } from "./common.js";
import { FreeFunction, MallocFunction } from "./interop.js";

const DEFENSIVE_CHECKS = true; // Turn this on to debug memory corruption issues
const SIMPLE_TRANSIENT_ALLOCATOR = false;
const CANARY_SIZE = 4;

const INITIAL_TRANSIENT_PAGE_SIZE = 128 * 1024;

interface TransientPage {
    ptr: number;
    size: number;
    head: number;
}

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

    private readonly _malloc: MallocFunction;
    private readonly _free: FreeFunction;
    private readonly _stackPointer: WebAssembly.Global;
    private readonly _heapBase: number;
    private readonly _stackHigh: number;
    private readonly _stackLow: number;

    private _transientPage: TransientPage;
    private readonly _freeList: [ptr: number, sz: number][] = [];
    private readonly _stalePageList: TransientPage[] = [];

    private _rangeMin?: number;
    private _rangeMax?: number;
    private readonly _rangeStack: number[] = [];

    constructor(exports: ScreepsDotNetExports) {
        this._memory = exports.memory;
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

        this._malloc = exports.malloc;
        this._free = exports.free;
        this._stackPointer = exports.__stack_pointer;
        this._heapBase = exports.__heap_base.value;
        this._stackHigh = exports.__stack_high.value;
        this._stackLow = exports.__stack_low.value;

        this.reportGrowth();

        this._transientPage = this.createTransientPage(INITIAL_TRANSIENT_PAGE_SIZE);
    }

    private reportGrowth(): void {
        console.log(`WASM linear memory growth to ${this._memory.buffer.byteLength}b (sp=${this._stackPointer.value}, heapBase=${this._heapBase}, stackHigh=${this._stackHigh}, stackLow=${this._stackLow})`);
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

    private checkStack(): void {
        const stackPtr: number = this._stackPointer.value;
        if (stackPtr < this._stackLow) { throw new Error(`stack pointer is too low (${stackPtr} < ${this._stackLow})`); }
        if (stackPtr > this._stackHigh) { throw new Error(`stack pointer is too high (${stackPtr} > ${this._stackHigh})`); }
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

    public writeBytes(ptr: number, bytes: Uint8Array): void;

    public writeBytes(dataView: DataView, bytes: Uint8Array): void;

    public writeBytes(ptrOrDataView: number | DataView, bytes: Uint8Array): void {
        const ptr = typeof ptrOrDataView === "number" ? ptrOrDataView : ptrOrDataView.byteOffset;
        if (DEFENSIVE_CHECKS) {
            this.checkDetached();
            this.checkConstrainedRange(ptr, bytes.length);
            if (typeof ptrOrDataView !== "number") {
                if (ptrOrDataView.buffer !== this._memory.buffer) { throw new Error(`DataView stale`); }
                if (ptrOrDataView.byteLength < bytes.length) { throw new Error(`DataView too small to accept bytes`); }
            }
        }
        this._u8.set(bytes, ptr);
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
        //this._u8.set(this._u8.subarray(src, src + sz), dst);
        this._u8.copyWithin(dst, src, src + sz);
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
        this.reportGrowth();
    }

    public allocateTransient(sz: number, alignment: number): number {
        if (SIMPLE_TRANSIENT_ALLOCATOR) {
            if (DEFENSIVE_CHECKS) {
                const ptr = this._malloc(sz + CANARY_SIZE * 2);
                this.flush();
                if (ptr === 0) { throw new Error(`failed to allocate transient (${sz}b)`); }
                if (ptr < this._heapBase) { throw new Error(`malloc overlapping stack (${sz}b, ${ptr} < ${this._heapBase})`); }
                this.checkAlignment(ptr, 4);
                const leftCanary = ptr;
                const returnPtr = ptr + CANARY_SIZE;
                const rightCanary = returnPtr + sz;
                try {
                    this.enterConstrainedRange(ptr, ptr + sz + CANARY_SIZE * 2);
                    for (let i = 0; i < CANARY_SIZE; ++i) {
                        this.writeU8(leftCanary + i, 0xCC);
                        this.writeU8(rightCanary + i, 0xCC);
                    }
                } finally {
                    this.exitConstrainedRange();
                }
                this._freeList.push([ptr, sz]);
                return returnPtr;
            } else {
                const ptr = this._malloc(sz);
                this.flush();
                if (ptr === 0) { throw new Error(`failed to allocate transient (${sz}b)`); }
                if (ptr < this._heapBase) { throw new Error(`malloc overlapping stack (${sz}b, ${ptr} < ${this._heapBase})`); }
                this._freeList.push([ptr, sz]);
                return ptr;
            }
        }
        
        let pagePtr = this.allocateWithinTransientPage(sz, alignment);
        if (pagePtr !== 0) {
            if (DEFENSIVE_CHECKS) {
                if (pagePtr < this._transientPage.ptr || pagePtr + sz > this._transientPage.ptr + this._transientPage.size) { throw new Error(`allocateWithinTransientPage returned invalid ptr (${pagePtr}, ptr=${this._transientPage.ptr}, size=${this._transientPage.size})`); }
                this.checkAlignment(pagePtr, alignment);
            }
            return pagePtr;
        }

        // No space in any pages, allocate new
        let nextSize = Math.max(WasmMemoryManager.npo2(sz), this._transientPage.size * 2);
        this._freeList.push([this._transientPage.ptr, this._transientPage.size]);
        this._transientPage = this.createTransientPage(nextSize);

        pagePtr = this.allocateWithinTransientPage(sz, alignment);
        if (pagePtr === 0) { throw new Error(`failed to allocate within transient page (pageHead=${this._transientPage.head}, pageSize=${this._transientPage.size}, sz=${sz})`); }
        if (DEFENSIVE_CHECKS) {
            if (pagePtr < this._transientPage.ptr || pagePtr + sz > this._transientPage.ptr + this._transientPage.size) { throw new Error(`allocateWithinTransientPage returned invalid ptr (${pagePtr}, ptr=${this._transientPage.ptr}, size=${this._transientPage.size})`); }
            this.checkAlignment(pagePtr, alignment);
        }
        return pagePtr;
    }

    private allocateWithinTransientPage(sz: number, alignment: number): number {
        const alignedHeadPtr = WasmMemoryManager.align(this._transientPage.ptr + this._transientPage.head, alignment);
        const newHead = alignedHeadPtr + sz - this._transientPage.ptr;
        if (newHead > this._transientPage.size) { return 0; }
        this._transientPage.head = newHead;
        return alignedHeadPtr;
    }

    private createTransientPage(sz: number): TransientPage {
        const page: TransientPage = {
            ptr: this._malloc(sz),
            head: 0,
            size: sz,
        };
        this.flush();
        if (DEFENSIVE_CHECKS) {
            if (page.ptr === 0) { throw new Error(`failed to allocate transient page (${sz}b)`); }
            if (page.ptr < this._heapBase) { throw new Error(`transient page was < heap base (${page.ptr} < ${this._heapBase})`); }
            this._u8.fill(0xCC, page.ptr, page.ptr + page.size);
        }        
        console.log(`allocated transient page (${page.size}b @ ${page.ptr})`);
        return page;
    }

    private resetTransientPage(page: TransientPage): void {
        if (DEFENSIVE_CHECKS) {
            for (let i = page.ptr + page.head; i < page.ptr + page.size; ++i) {
                if (this._u8[i] !== 0xCC) {
                    console.log(`transient page corruption detected - expecting 0xCC, found '${this._u8[i]}' @ ${i} (ptr=${page.ptr}, size=${page.size}, head=${page.head})`);
                }
            }
            this._u8.fill(0xCC, page.ptr, page.ptr + page.size);
        }
        page.head = 0;
    }

    private destroyTransientPage(page: TransientPage): void {
        this.resetTransientPage(page);
        this._free(page.ptr);
        page.ptr = 0;
        page.size = 0;
    }

    public freeAllTransient(): void {
        if (SIMPLE_TRANSIENT_ALLOCATOR) {
            for (const [ptr, sz] of this._freeList) {
                if (DEFENSIVE_CHECKS) {
                    const leftCanary = ptr;
                    const returnPtr = ptr + CANARY_SIZE;
                    const rightCanary = returnPtr + sz;
                    for (let i = 0; i < CANARY_SIZE; ++i) {
                        let c: number;
                        if ((c = this.readU8(leftCanary + i)) !== 0xCC) {
                            console.log(`freeAllTransient left canary corrupted (ptr=${ptr}, sz=${sz}, testPtr=${leftCanary + i}, expected=${0xCC}, actual=${c})`);
                        }
                        if ((c = this.readU8(rightCanary + i)) !== 0xCC) {
                            console.log(`freeAllTransient right canary corrupted (ptr=${ptr}, sz=${sz}, testPtr=${rightCanary + i}, expected=${0xCC}, actual=${c})`);
                        }
                    }
                }
                this._free(ptr);
            }
        } else {
            for (const page of this._stalePageList) {
                this.destroyTransientPage(page);
            }
            this._stalePageList.length = 0;
        }
        this._transientPage.head = 0;
    }

    private static npo2(v: number): number {
        v += v === 0 ? 1 : 0;
        --v;
        v |= v >>> 1;
        v |= v >>> 2;
        v |= v >>> 4;
        v |= v >>> 8;
        v |= v >>> 16;
        return v + 1;
    }

    private static align(v: number, alignment: number): number {
        --alignment;
        return (v + alignment) & ~alignment;
    }

    private static align8(v: number): number {
        return (v + 7) & ~7;
    }
}
