import { ScreepsDotNetExports } from '../common.js';
import type { ImportTable, MallocFunction } from '../interop.js';
import { createWasmMemoryView, WasmMemoryView } from '../memory.js';

export abstract class BaseBindings {
    public readonly imports: Record<string, ImportTable> = {};

    private readonly logFunc: (text: string) => void;

    protected _memory?: WebAssembly.Memory;
    protected _malloc?: MallocFunction;

    protected _memoryView?: WasmMemoryView;

    constructor(logFunc: (text: string) => void) {
        this.logFunc = logFunc;
        this.setupImports();
    }

    public init(exports: ScreepsDotNetExports): void {
        this._memory = exports.memory;
        this.recreateBufferViews();
        this._malloc = exports.malloc;
    }

    public loop(): void {

    }

    public recreateBufferViews(): void {
        if (this._memory) {
            this._memoryView = createWasmMemoryView(this._memory);
        } else {
            this._memoryView = undefined;
        }
    }

    protected setupImports(): void {

    }

    protected log(text: string): void {
        this.logFunc(text);
    }
}
