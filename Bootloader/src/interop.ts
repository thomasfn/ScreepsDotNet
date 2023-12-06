import { createWasmMemoryView, WasmMemoryView } from './memory.js';

const enum InteropValueType {
    Void = 0,
    U1 = 1,
    U8 = 2,
    I8 = 3,
    U16 = 4,
    I16 = 5,
    U32 = 6,
    I32 = 7,
    U64 = 8,
    I64 = 9,
    F32 = 10,
    F64 = 11,
    Ptr = 12,
    Str = 13,
    Obj = 14,
    Arr = 15,
}

const INTEROP_VALUE_TYPE_NAMES: Record<InteropValueType, string> = [
    'void',
    'bool',
    'byte',
    'sbyte',
    'ushort',
    'short',
    'uint',
    'int',
    'ulong',
    'long',
    'float',
    'double',
    'void*',
    'char*',
    'JSObject',
    '[]',
];

interface ParamSpec {
    type: InteropValueType;
    nullable: boolean;
    nullAsUndefined: boolean;
    elementSpec?: ParamSpec;
}

interface FunctionSpec {
    returnSpec: ParamSpec;
    paramSpecs: ParamSpec[];
}

function stringifyParamSpec(paramSpec: Readonly<ParamSpec>): string {
    if (paramSpec.type === InteropValueType.Arr && paramSpec.elementSpec) {
        return `${stringifyParamSpec(paramSpec.elementSpec)}[]`;
    }
    return `${INTEROP_VALUE_TYPE_NAMES[paramSpec.type]}${paramSpec.nullable ? '?' : ''}`;
}

const EXCEPTION_PARAM_SPEC: Readonly<ParamSpec> = { type: InteropValueType.Str, nullable: false, nullAsUndefined: false };

type BoundImportFunction = (paramsBufferPtr: number) => number;

interface BoundImportSymbol {
    fullName: string;
    functionSpec: Readonly<FunctionSpec>;
}

const CLR_TRACKING_ID = Symbol('clr-tracking-id');

const TEMP_PROPERTY_DESCRIPTOR: PropertyDescriptor = {};

export type MallocFunction = (sz: number) => number;

export interface ImportTable {
    [key: string]: Importable;
}

export type Importable = ((...args: any[]) => unknown) | ImportTable;

export class Interop {
    public readonly interopImport: Record<string, (...args: any[]) => unknown>;

    private readonly _imports: Record<string, ImportTable> = {};
    
    private readonly _boundImportList: BoundImportFunction[] = [];
    private readonly _boundImportSymbolList: BoundImportSymbol[] = [];
    private readonly _objectTrackingList: Record<number, object> = {};

    private _memory?: WebAssembly.Memory;
    private _malloc?: MallocFunction;
    private _nextClrTrackingId: number = 0;

    private _memoryView?: WasmMemoryView;

    public get memory(): WebAssembly.Memory | undefined { return this._memory; }
    public set memory(value) {
        this._memory = value;
        this.recreateBufferViews();
    }

    public get malloc(): MallocFunction | undefined { return this._malloc; }
    public set malloc(value) { this._malloc = value; }

    constructor() {
        this.interopImport = {};
        this.interopImport.js_bind_import = this.js_bind_import.bind(this);
        this.interopImport.js_invoke_import = this.js_invoke_import.bind(this);
        this.interopImport.js_release_object_reference = this.js_release_object_reference.bind(this);
    }

    public setImports(moduleName: string, importTable: ImportTable): void {
        this._imports[moduleName] = importTable;
    }

    public recreateBufferViews(): void {
        if (this._memory) {
            this._memoryView = createWasmMemoryView(this._memory);
        } else {
            this._memoryView = undefined;
        }
    }

    private resolveImport(moduleName: string, importTable: Readonly<ImportTable>, importName: string): (...args: any[]) => unknown {
        const segments = importName.split('.');
        let currentValue: Importable = importTable;
        for (const segment of segments) {
            if (currentValue == null || typeof currentValue !== 'object') {
                throw new Error(`unable to resolve import '${importName}' from module '${moduleName}' (one or more keys along the path did not resolve to an object)`);
            }
            currentValue = currentValue[segment];
        }
        if (currentValue == null || typeof currentValue !== 'function') {
            throw new Error(`unable to resolve import '${importName}' from module '${moduleName}' (the path did not resolve to a function)`);
        }
        return currentValue;
    }

    private js_bind_import(moduleNamePtr: number, importNamePtr: number, functionSpecPtr: number): number {
        const moduleName = this.stringToJs(moduleNamePtr);
        const importTable = this._imports[moduleName];
        if (!importTable) {
            throw new Error(`unknown import module '${moduleName}'`);
        }
        const importName = this.stringToJs(importNamePtr);
        const importFunction = this.resolveImport(moduleName, importTable, importName);
        const functionSpec = this.functionSpecToJs(functionSpecPtr);
        const importIndex = this._boundImportList.length;
        const boundImportFunction = this.createImportBinding(importFunction, functionSpec, importIndex);
        this._boundImportList.push(boundImportFunction);
        this._boundImportSymbolList.push({ fullName: `${moduleName}::${importName}`, functionSpec });
        // console.log(this.stringifyImportBindingForDisplay(importIndex));
        return importIndex;
    }

    private js_invoke_import(importIndex: number, paramsBufferPtr: number): number {
        const boundImportFunction = this._boundImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        return boundImportFunction(paramsBufferPtr);
    }

    private js_release_object_reference(clrTrackingId: number): void {
        const obj = this._objectTrackingList[clrTrackingId];
        if (obj == null) { return; }
        delete this._objectTrackingList[clrTrackingId];
        this.clearClrTrackingId(obj);
    }

    private createImportBinding(importFunction: (...args: unknown[]) => unknown, functionSpec: Readonly<FunctionSpec>, importIndex: number): BoundImportFunction {
        return (paramsBufferPtr) => {
            // TODO: Cache args array to eliminate allocation here
            const args: unknown[] = [];
            args.length = functionSpec.paramSpecs.length;
            const returnValPtr = paramsBufferPtr;
            const exceptionValPtr = paramsBufferPtr + 16;
            let argsPtr = exceptionValPtr + 16;
            try {
                for (let i = 0; i < functionSpec.paramSpecs.length; ++i) {
                    args[i] = this.marshalToJs(argsPtr, functionSpec.paramSpecs[i]);
                    argsPtr += 16;
                }
            } catch (err) {
                throw new Error(`${this.stringifyImportBindingForDisplay(importIndex)}: ${(err as Error).message}`);
            }
            let returnVal: unknown;
            try {
                returnVal = importFunction(...args);
                this.marshalToClr(returnValPtr, functionSpec.returnSpec, returnVal);
                //console.log(`${importIndex}:${this._boundImportSymbolList[importIndex]}(${args.map(this.stringifyValueForDisplay.bind(this)).join(', ')}) -> ${this.stringifyValueForDisplay(returnVal)}`);
                return 1;
            } catch (err) {
                this.marshalToClr(exceptionValPtr, EXCEPTION_PARAM_SPEC, `${err}`);
                return 0;
            }
        };
    }

    private marshalToJs(valuePtr: number, paramSpec: Readonly<ParamSpec>): unknown {
        const valueType: InteropValueType = this._memoryView!.u8[valuePtr + 12];
        if (valueType === InteropValueType.Void && paramSpec.nullable) {
            return paramSpec.nullAsUndefined ? undefined : null;
        }
        if (paramSpec.type === InteropValueType.Arr && paramSpec.elementSpec?.type === InteropValueType.Str && valueType === InteropValueType.Arr) {
            return this.stringArrayToJs(this._memoryView!.i32[valuePtr >> 2], this._memoryView!.i32[(valuePtr + 8) >> 2], paramSpec.elementSpec);
        }
        if (paramSpec.type === InteropValueType.I32 && valueType === InteropValueType.Ptr) {
            return this._memoryView!.i32[valuePtr >> 2];
        }
        if (valueType !== paramSpec.type) {
            throw new Error(`failed to marshal ${stringifyParamSpec(paramSpec)} from '${INTEROP_VALUE_TYPE_NAMES[valueType] ?? 'unknown'}'`);
        }
        switch (paramSpec.type) {
            case InteropValueType.Void: return undefined;
            case InteropValueType.U1: return this._memoryView!.u8[valuePtr] !== 0;
            case InteropValueType.U8: return this._memoryView!.u8[valuePtr];
            case InteropValueType.I8: return this._memoryView!.i8[valuePtr];
            case InteropValueType.U16: return this._memoryView!.u16[valuePtr >> 1];
            case InteropValueType.I16: return this._memoryView!.i16[valuePtr >> 1];
            case InteropValueType.U32: return this._memoryView!.u32[valuePtr >> 2];
            case InteropValueType.I32: return this._memoryView!.i32[valuePtr >> 2];
            case InteropValueType.U64: return (this._memoryView!.u32[valuePtr >> 2] << 32) | this._memoryView!.u32[(valuePtr + 4) >> 2];
            case InteropValueType.I64: return (this._memoryView!.i32[valuePtr >> 2] << 32) | this._memoryView!.i32[(valuePtr + 4) >> 2];
            case InteropValueType.F32: return this._memoryView!.f32[valuePtr >> 2];
            case InteropValueType.F64: return this._memoryView!.f64[valuePtr >> 3];
            case InteropValueType.Ptr: return new DataView(this._memory!.buffer, this._memoryView!.i32[valuePtr >> 2], this._memoryView!.i32[(valuePtr + 8) >> 2]);
            case InteropValueType.Str: return this.stringToJs(this._memoryView!.i32[valuePtr >> 2]);
            case InteropValueType.Obj: return this._objectTrackingList[this._memoryView!.i32[(valuePtr + 4) >> 2]];
            case InteropValueType.Arr:
                if (paramSpec.elementSpec == null) {
                    throw new Error(`malformed param spec (array with no element spec)`);
                }
                return this.arrayToJs(this._memoryView!.i32[valuePtr >> 2], this._memoryView!.i32[(valuePtr + 8) >> 2], paramSpec.elementSpec);
            default: throw new Error(`failed to marshal ${stringifyParamSpec(paramSpec)} from '${INTEROP_VALUE_TYPE_NAMES[valueType] ?? 'unknown'}'`);
        }
    }

    private marshalToClr(valuePtr: number, paramSpec: Readonly<ParamSpec>, value: unknown): void {
        if (value == null) {
            if (paramSpec.nullable || paramSpec.type === InteropValueType.Void) {
                this._memoryView!.u8[valuePtr + 12] = InteropValueType.Void;
                return;
            }
            throw new Error(`failed to marshal null as '${stringifyParamSpec(paramSpec)}'`);
        }
        switch (paramSpec.type) {
            case InteropValueType.Void:
                this._memoryView!.u8[valuePtr + 12] = InteropValueType.Void;
                break;
            case InteropValueType.U1:
                this._memoryView!.u8[valuePtr] = value ? 1 : 0;
                this._memoryView!.u8[valuePtr + 12] = InteropValueType.U1;
                break;
            case InteropValueType.U8:
            case InteropValueType.I8:
            case InteropValueType.U16:
            case InteropValueType.I16:
            case InteropValueType.U32:
            case InteropValueType.I32:
            case InteropValueType.U64:
            case InteropValueType.I64:
            case InteropValueType.F32:
            case InteropValueType.F64:
                if (typeof value === 'number') {
                    this.marshalNumericToClr(valuePtr, paramSpec, value);
                    break;
                }
                if (value instanceof BigInt) {
                    throw new Error(`failed to marshal BigInt as '${stringifyParamSpec(paramSpec)}' (not yet implemented)`);
                }
                throw new Error(`failed to marshal non-numeric as '${stringifyParamSpec(paramSpec)}'`);
            // case InteropValueType.Ptr: return;
            case InteropValueType.Str:
                this._memoryView!.i32[valuePtr >> 2] = this.stringToClr(typeof value === 'string' ? value : `${value}`);
                this._memoryView!.u8[valuePtr + 12] = InteropValueType.Str;
                break;
            case InteropValueType.Obj:
                if (typeof value !== 'object' && typeof value !== 'function') {
                    throw new Error(`failed to marshal ${typeof value} as '${stringifyParamSpec(paramSpec)}' (not an object)`);
                }
                const clrTrackingId = this.getClrTrackingId(value) ?? this.assignClrTrackingId(value);
                this._memoryView!.i32[(valuePtr + 4) >> 2] = clrTrackingId;
                this._memoryView!.u8[valuePtr + 12] = InteropValueType.Obj;
                break;
            case InteropValueType.Arr:
                if (paramSpec.elementSpec == null) {
                    throw new Error(`malformed param spec (array with no element spec)`);
                }
                if (!Array.isArray(value)) {
                    //value = [value];
                    // TODO: We could have a param spec flag that wraps single values in arrays in case we need to support apis that sometimes returns an array and sometimes a single value
                    throw new Error(`failed to marshal ${typeof value} as '${stringifyParamSpec(paramSpec)}' (not an array)`);
                }
                if (paramSpec.elementSpec.type === InteropValueType.Str) {
                    this._memoryView!.i32[valuePtr >> 2] = this.stringArrayToClr(value, paramSpec.elementSpec);
                } else {
                    this._memoryView!.i32[valuePtr >> 2] = this.arrayToClr(value, paramSpec.elementSpec);
                }
                this._memoryView!.i32[(valuePtr + 8) >> 2] = value.length;
                this._memoryView!.u8[valuePtr + 12] = InteropValueType.Arr;
                break;
            default: throw new Error(`failed to marshal '${typeof value}' as '${stringifyParamSpec(paramSpec)}' (not yet implemented)`);
        }
    }

    private marshalNumericToClr(valuePtr: number, paramSpec: Readonly<ParamSpec>, value: number): void {
        switch (paramSpec.type) {
            case InteropValueType.U8: this._memoryView!.u8[valuePtr] = value; break;
            case InteropValueType.I8: this._memoryView!.i8[valuePtr] = value; break;
            case InteropValueType.U16: this._memoryView!.u16[valuePtr >> 1] = value; break;
            case InteropValueType.I16: this._memoryView!.i16[valuePtr >> 1] = value; break;
            case InteropValueType.U32: this._memoryView!.u32[valuePtr >> 2] = value; break;
            case InteropValueType.I32: this._memoryView!.i32[valuePtr >> 2] = value; break;
            // case InteropValueType.U64: break;
            // case InteropValueType.I64: break;
            case InteropValueType.F32: this._memoryView!.f32[valuePtr >> 2] = value; break;
            case InteropValueType.F64: this._memoryView!.f64[valuePtr >> 3] = value; break;
            default: throw new Error(`failed to marshal numeric as '${stringifyParamSpec(paramSpec)}' (not yet implemented)`);
        }
        this._memoryView!.u8[valuePtr + 12] = paramSpec.type;
    }

    private stringToJs(stringPtr: number): string {
        let code: number;
        let result = '';
        do {
            code = this._memoryView!.u16[stringPtr >> 1];
            if (code !== 0) { result += String.fromCharCode(code); }
            stringPtr += 2;
        } while (code !== 0);
        return result;
    }

    private stringToClr(str: string): number {
        const strPtr = this._malloc!((str.length + 1) * 2);
        let charPtr = strPtr;
        for (let i = 0; i < str.length; ++i) {
            this._memoryView!.u16[charPtr >> 1] = str.charCodeAt(i);
            charPtr += 2;
        }
        this._memoryView!.u16[charPtr >> 1] = 0;
        return strPtr;
    }

    private arrayToJs(arrayPtr: number, arrayLen: number, elementSpec: Readonly<ParamSpec>): unknown[] {
        const result: unknown[] = [];
        result.length = arrayLen;
        for (let i = 0; i < arrayLen; ++i) {
            result[i] = this.marshalToJs(arrayPtr, elementSpec);
            arrayPtr += 16;
        }
        return result;
    }

    private arrayToClr(value: unknown[], elementSpec: Readonly<ParamSpec>): number {
        const arrPtr = this._malloc!(value.length * 16);
        let elPtr = arrPtr;
        for (let i = 0; i < value.length; ++i) {
            this.marshalToClr(elPtr, elementSpec, value[i]);
            elPtr += 16;
        }
        return arrPtr;
    }

    private stringArrayToJs(arrayPtr: number, arrayLen: number, elementSpec: Readonly<ParamSpec>): (string | null | undefined)[] {
        const result: (string | null | undefined)[] = [];
        result.length = arrayLen;
        for (let i = 0; i < arrayLen; ++i) {
            let code: number;
            if (elementSpec.nullable) {
                code = this._memoryView!.u16[arrayPtr >> 1];
                arrayPtr += 2;
                if (code === 0) {
                    result[i] = elementSpec.nullAsUndefined ? undefined : null;
                    arrayPtr += 2;
                    break;
                }
            }
            let element = '';
            do {
                code = this._memoryView!.u16[arrayPtr >> 1];
                if (code !== 0) { element += String.fromCharCode(code); }
                arrayPtr += 2;
            } while (code !== 0);
            result[i] = element;
        }
        return result;
    }

    private stringArrayToClr(value: unknown[], elementSpec: Readonly<ParamSpec>): number {
        let bufferSize = 0;
        for (const element of value) {
            if (elementSpec.nullable) {
                ++bufferSize;
                if (element == null) {
                    ++bufferSize;
                    continue;
                }
            }
            const str = typeof element === 'string' ? element : `${element}`;
            bufferSize += str.length + 1;
        }
        const strPtr = this._malloc!(bufferSize * 2);
        let charPtr = strPtr;
        for (const element of value) {
            if (elementSpec.nullable) {
                this._memoryView!.u16[charPtr >> 1] = element != null ? 1 : 0;
                charPtr += 2;
            }
            const str = typeof element === 'string' ? element : `${element}`;
            for (let i = 0; i < str.length; ++i) {
                this._memoryView!.u16[charPtr >> 1] = str.charCodeAt(i);
                charPtr += 2;
            }
            this._memoryView!.u16[charPtr >> 1] = 0;
            charPtr += 2;
        }
        return strPtr;
    }

    private paramSpecToJs(paramSpecPtr: number): ParamSpec {
        const type = this._memoryView!.u8[paramSpecPtr];
        const flags = this._memoryView!.u8[paramSpecPtr + 1];
        const elementType = this._memoryView!.u8[paramSpecPtr + 2];
        const elementFlags = this._memoryView!.u8[paramSpecPtr + 3];
        return {
            type: type,
            nullable: (flags & 1) === 1,
            nullAsUndefined: (flags & 2) === 2,
            elementSpec: elementType !== InteropValueType.Void ? {
                type: elementType,
                nullable: (elementFlags & 1) === 1,
                nullAsUndefined: (elementFlags & 2) === 2,
            } : undefined,
        };
    }

    private functionSpecToJs(functionSpecPtr: number): FunctionSpec {
        const result: FunctionSpec = {
            returnSpec: this.paramSpecToJs(functionSpecPtr),
            paramSpecs: []
        };
        functionSpecPtr += 4;
        for (let i = 0; i < 8; ++i) {
            const paramSpec = this.paramSpecToJs(functionSpecPtr);
            if (paramSpec.type === InteropValueType.Void) { break; }
            result.paramSpecs.push(paramSpec);
            functionSpecPtr += 4;
        }
        return result;
    }

    private getClrTrackingId(obj: object): number | undefined {
        return Object.getOwnPropertyDescriptor(obj, CLR_TRACKING_ID)?.value;
    }

    private assignClrTrackingId(obj: object): number {
        const clrTrackingId = this._nextClrTrackingId++;
        TEMP_PROPERTY_DESCRIPTOR.value = clrTrackingId;
        TEMP_PROPERTY_DESCRIPTOR.configurable = true;
        Object.defineProperty(obj, CLR_TRACKING_ID, TEMP_PROPERTY_DESCRIPTOR);
        this._objectTrackingList[clrTrackingId] = obj;
        return clrTrackingId;
    }

    private clearClrTrackingId(obj: object): void {
        TEMP_PROPERTY_DESCRIPTOR.value = undefined;
        TEMP_PROPERTY_DESCRIPTOR.configurable = true;
        Object.defineProperty(obj, CLR_TRACKING_ID, TEMP_PROPERTY_DESCRIPTOR);
    }

    private stringifyValueForDisplay(value: unknown): string {
        if (value === undefined) { return 'undefined'; }
        if (value === null) { return 'null'; }
        if (typeof value === 'string') { return `'${value}'`; }
        if (typeof value === 'number' || typeof value === 'boolean') { return `${value}`; }
        if (Array.isArray(value)) { return `array[#${value.length}, %${this.getClrTrackingId(value)}]`; }
        if (typeof value === 'object') { return `object[#${Object.keys(value).length}, %${this.getClrTrackingId(value)}]`; }
        return typeof value;
    }

    private stringifyImportBindingForDisplay(importIndex: number): string {
        const boundImportSymbol = this._boundImportSymbolList[importIndex];
        return `${importIndex}: ${stringifyParamSpec(boundImportSymbol.functionSpec.returnSpec)} ${boundImportSymbol.fullName}(${boundImportSymbol.functionSpec.paramSpecs.map(stringifyParamSpec).join(', ')})`;
    }
}
