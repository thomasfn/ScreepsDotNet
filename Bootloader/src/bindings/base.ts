import { ScreepsDotNetExports } from '../common.js';
import type { ImportTable, MallocFunction } from '../interop.js';
import { WasmMemoryManager } from '../memory.js';

export abstract class BaseBindings {
    public readonly imports: Record<string, ImportTable> = {};

    private readonly logFunc: (text: string) => void;

    protected _memoryManager?: WasmMemoryManager;
    protected _malloc?: MallocFunction;

    constructor(logFunc: (text: string) => void) {
        this.logFunc = logFunc;
        this.setupImports();
    }

    public init(exports: ScreepsDotNetExports, memoryManager: WasmMemoryManager): void {
        this._memoryManager = memoryManager;
        this._malloc = exports.malloc;
    }

    public loop(): void {

    }

    protected setupImports(): void {

    }

    protected log(text: string): void {
        this.logFunc(text);
    }
}
