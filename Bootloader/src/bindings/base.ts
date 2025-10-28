import { ScreepsDotNetExports } from '../common.js';
import type { ImportTable, Interop, MallocFunction } from '../interop.js';
import { WasmMemoryManager } from '../memory.js';

export default abstract class BaseBindings {
    public readonly bindingsImport: Record<string, (...args: any[]) => unknown>;
    public readonly imports: Record<string, ImportTable> = {};

    protected readonly _interop: Interop;

    private readonly logFunc: (text: string) => void;
    
    protected _memoryManager?: WasmMemoryManager;
    protected _malloc?: MallocFunction;

    constructor(logFunc: (text: string) => void, interop: Interop) {
        this.logFunc = logFunc;
        this._interop = interop;
        this.bindingsImport = {};
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
