import { WasmMemoryManager } from './memory.js';
import { ObjectInterop } from './object-interop.js';

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
    Pointer = 12,
    String = 13,
    Object = 14,
    Array = 15,
    Name = 16,
    Struct = 17,
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
    'Name',
    'struct',
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
    if (paramSpec.type === InteropValueType.Array && paramSpec.elementSpec) {
        return `${stringifyParamSpec(paramSpec.elementSpec)}[]`;
    }
    return `${INTEROP_VALUE_TYPE_NAMES[paramSpec.type]}${paramSpec.nullable ? '?' : ''}`;
}

const EXCEPTION_PARAM_SPEC: Readonly<ParamSpec> = { type: InteropValueType.String, nullable: false, nullAsUndefined: false };

type BoundImportFunction = (paramsBufferPtr: number) => number;

type BoundRawImportFunction = (...args: unknown[]) => unknown;

interface BoundImportSymbol {
    fullName: string;
    functionSpec: Readonly<FunctionSpec>;
}

interface StructFieldSpec {
    fieldName: string;
    paramSpec: ParamSpec;
}

interface StructSpec {
    fieldSpecs: StructFieldSpec[];
}

export type MallocFunction = (sz: number) => number;

export interface ImportTable {
    [key: string]: Importable;
}

export type Importable = ((...args: any[]) => unknown) | ImportTable;

const IMPORT_BINDING_SCOPE = {
    EXCEPTION_PARAM_SPEC,
};

export class Interop {
    public readonly interopImport: Record<string, (...args: any[]) => unknown>;

    private readonly _profileFn: () => number;
    private readonly _imports: Record<string, ImportTable> = {};

    private readonly _objects = new ObjectInterop();

    public get objects() { return this._objects; }
    
    private readonly _boundImportList: BoundImportFunction[] = [];
    private readonly _boundRawImportList: BoundRawImportFunction[] = [];
    private readonly _boundImportSymbolList: BoundImportSymbol[] = [];
    private readonly _nameList: string[] = [];
    private readonly _nameTable: Record<string, number> = {};
    private readonly _structList: StructSpec[] = [];

    private _memory?: WasmMemoryManager;
    private _malloc?: MallocFunction;

    private _numBoundImportInvokes: number = 0;
    private _numImportBinds: number = 0;
    private _timeInInterop: number = 0;
    private _timeInJsUserCode: number = 0;

    public get memory(): WasmMemoryManager | undefined { return this._memory; }
    public set memory(value) {
        this._memory = value;
    }

    public get malloc(): MallocFunction | undefined { return this._malloc; }
    public set malloc(value) { this._malloc = value; }

    constructor(profileFn: () => number) {
        this._profileFn = profileFn;
        this.interopImport = {};
        this.interopImport['bind-import'] = this.js_bind_import.bind(this);
        this.interopImport['invoke-import'] = this.js_invoke_import.bind(this);
        this.interopImport['release-object-reference'] = this.js_release_object_reference.bind(this);
        this.interopImport['set-name'] = this.js_set_name.bind(this);
        this.interopImport['define-struct'] = this.js_define_struct.bind(this);

        this.interopImport['invoke-i-i'] = this.js_invoke_i_i.bind(this);
        this.interopImport['invoke-i-ii'] = this.js_invoke_i_ii.bind(this);
        this.interopImport['invoke-i-iii'] = this.js_invoke_i_iii.bind(this);
        this.interopImport['invoke-i-o'] = this.js_invoke_i_o.bind(this);
        this.interopImport['invoke-i-oi'] = this.js_invoke_i_oi.bind(this);
        this.interopImport['invoke-i-on'] = this.js_invoke_i_on.bind(this);
        this.interopImport['invoke-i-oii'] = this.js_invoke_i_oii.bind(this);
        this.interopImport['invoke-i-oo'] = this.js_invoke_i_oo.bind(this);
        this.interopImport['invoke-i-ooi'] = this.js_invoke_i_ooi.bind(this);
        this.interopImport['invoke-i-ooii'] = this.js_invoke_i_ooii.bind(this);
        this.interopImport['invoke-d-v'] = this.js_invoke_d_v.bind(this);
    }

    public setImports(moduleName: string, importTable: ImportTable): void {
        this._imports[moduleName] = importTable;
    }

    public loop(): void {
        this._numBoundImportInvokes = 0;
        this._numImportBinds = 0;
        this._timeInInterop = 0;
        this._timeInJsUserCode = 0;
        this._objects.loop();
    }

    public buildProfilerString(): string {
        const phrases: string[] = [
            `${((this._timeInInterop * 100)|0)/100} ms in interop`,
            `${((this._timeInJsUserCode * 100)|0)/100} ms in screeps api`,
            `${this._numBoundImportInvokes} js interop calls`,
            `${this._objects.numTotalTrackingObjects} +${this._objects.numBeginTrackingObjects} -${this._objects.numReleaseTrackingObjects} tracked js objects`,
        ];
        if (this._numBoundImportInvokes > 0) {
            phrases.push(`${this._boundImportList.length} +${this._numImportBinds} bound imports`);
        }
        return phrases.join(', ');
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
        this.memory!.flush();
        const moduleName = this.stringToJs(moduleNamePtr);
        const importTable = this._imports[moduleName];
        if (!importTable) {
            throw new Error(`unknown import module '${moduleName}'`);
        }
        const importName = this.stringToJs(importNamePtr);
        const importFunction = this.resolveImport(moduleName, importTable, importName);
        this._boundRawImportList.push(importFunction);
        const functionSpec = this.functionSpecToJs(functionSpecPtr);
        const importIndex = this._boundImportList.length;
        const boundImportFunction = this.createImportBinding(importFunction, functionSpec, importIndex);
        this._boundImportList.push(boundImportFunction);
        this._boundImportSymbolList.push({ fullName: `${moduleName}::${importName}`, functionSpec });
        ++this._numImportBinds;
        // console.log(this.stringifyImportBindingForDisplay(importIndex));
        return importIndex;
    }

    private js_invoke_import(importIndex: number, paramsBufferPtr: number): number {
        const boundImportFunction = this._boundImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        return boundImportFunction(paramsBufferPtr);
    }

    private js_release_object_reference(objectHandle: number): void {
        this._objects.releaseObjectHandle(objectHandle);
    }

    private js_set_name(nameIndex: number, valuePtr: number): void {
        this.memory!.flush();
        const value = this.stringToJs(valuePtr);
        this._nameList[nameIndex] = value;
        this._nameTable[value] = nameIndex;
    }

    private js_define_struct(numFields: number, fieldsPtr: number): number {
        this.memory!.flush();
        const spec: StructSpec = {
            fieldSpecs: [],
        };
        spec.fieldSpecs.length = numFields;
        try {
            this._memory!.enterConstrainedRange(fieldsPtr, numFields * 8);
            for (let i = 0; i < numFields; ++i) {
                const fieldName = this.stringToJs(this._memory!.readI32(fieldsPtr));
                fieldsPtr += 4;
                const paramSpec = this.paramSpecToJs(fieldsPtr);
                fieldsPtr += 4;
                spec.fieldSpecs[i] = { fieldName, paramSpec };
            }
            return this._structList.push(spec) - 1;
        } finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private js_invoke_i_i(importIndex: number, p0: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(p0) as number;
    }

    private js_invoke_i_ii(importIndex: number, p0: number, p1: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(p0, p1) as number;
    }

    private js_invoke_i_iii(importIndex: number, p0: number, p1: number, p2: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(p0, p1, p2) as number;
    }

    private js_invoke_i_o(importIndex: number, p0: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0)) as number;
    }

    private js_invoke_i_oi(importIndex: number, p0: number, p1: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), p1) as number;
    }

    private js_invoke_i_on(importIndex: number, p0: number, p1: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), this._nameList[p1]) as number;
    }

    private js_invoke_i_oii(importIndex: number, p0: number, p1: number, p2: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), p1, p2) as number;
    }

    private js_invoke_i_oo(importIndex: number, p0: number, p1: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), this._objects.getObjectByHandle(p1)) as number;
    }

    private js_invoke_i_ooi(importIndex: number, p0: number, p1: number, p2: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), this._objects.getObjectByHandle(p1), p2) as number;
    }

    private js_invoke_i_ooii(importIndex: number, p0: number, p1: number, p2: number, p3: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction(this._objects.getObjectByHandle(p0), this._objects.getObjectByHandle(p1), p2, p3) as number;
    }

    private js_invoke_d_v(importIndex: number): number {
        const boundImportFunction = this._boundRawImportList[importIndex];
        if (!boundImportFunction) {
            throw new Error(`attempt to invoke invalid import index ${importIndex}`);
        }
        ++this._numBoundImportInvokes;
        this.memory!.flush();
        return boundImportFunction() as number;
    }

    private createImportBinding(importFunction: (...args: unknown[]) => unknown, functionSpec: Readonly<FunctionSpec>, importIndex: number): BoundImportFunction {
        const lines: string[] = [];
        lines.push(`var t0 = this._profileFn();`);
        lines.push(`this._memory.flush();`);
        lines.push(`this._memory.enterConstrainedRange(paramsBufferPtr, ${(functionSpec.paramSpecs.length + 2) * 16});`);
        lines.push(`var returnValPtr = paramsBufferPtr;`);
        lines.push(`var exceptionValPtr = paramsBufferPtr + 16;`);
        lines.push(`var argsPtr = exceptionValPtr + 16;`);
        let paramList: string = '';
        if (functionSpec.paramSpecs.length > 0) {
            const paramListArr: string[] = [];
            for (let i = 0; i < functionSpec.paramSpecs.length; ++i) {
                lines.push(`var arg${i};`);
                paramListArr.push(`arg${i}`);
            }
            paramList = paramListArr.join(', ');
            lines.push(`try {`);
            for (let i = 0; i < functionSpec.paramSpecs.length; ++i) {
                lines.push(`  arg${i} = this.marshalToJs(argsPtr, functionSpec.paramSpecs[${i}]);`)
                lines.push(`  argsPtr += 16;`);
            }
            lines.push(`} catch (err) {`);
            lines.push(`  this._memory.exitConstrainedRange();`);
            lines.push(`  throw new Error(this.stringifyImportBindingForDisplay(${importIndex}) + ': ' + err.stack);`);
            lines.push(`}`);
        }
        lines.push(`var t1 = this._profileFn();`);
        lines.push(`this._timeInInterop += (t1 - t0);`);
        lines.push(`var returnVal;`);
        lines.push(`try {`);
        lines.push(`  returnVal = importFunction(${paramList});`);
        lines.push(`  this._memory.flush();`);
        lines.push(`  this.marshalToClr(returnValPtr, functionSpec.returnSpec, returnVal);`);
        lines.push(`  return 1;`);
        lines.push(`} catch (err) {`);
        lines.push(`  this.marshalToClr(exceptionValPtr, scope.EXCEPTION_PARAM_SPEC, err.stack);`);
        lines.push(`} finally {`);
        lines.push(`  var t2 = this._profileFn();`);
        lines.push(`  this._timeInJsUserCode += (t2 - t1);`);
        lines.push(`  this._memory.exitConstrainedRange();`);
        lines.push(`}`);
        const compiler = (Function(`return function import_binding_${importIndex}(scope, importFunction, functionSpec, paramsBufferPtr) {\n${lines.join('\n')}\n};`) as () => ((scope: typeof IMPORT_BINDING_SCOPE, importFunction: (...args: unknown[]) => unknown, functionSpec: Readonly<FunctionSpec>, paramsBufferPtr: number) => number));
        return compiler().bind(this, IMPORT_BINDING_SCOPE, importFunction, functionSpec);
    }

    private marshalToJs(valuePtr: number, paramSpec: Readonly<ParamSpec>): unknown {
        const valueType: InteropValueType = this._memory!.readU8(valuePtr + 12);
        if (valueType === InteropValueType.Void && paramSpec.nullable) {
            return paramSpec.nullAsUndefined ? undefined : null;
        }
        if (paramSpec.type === InteropValueType.Array && paramSpec.elementSpec?.type === InteropValueType.String && valueType === InteropValueType.Array) {
            return this.stringArrayToJs(this._memory!.readI32(valuePtr), this._memory!.readI32(valuePtr + 8), paramSpec.elementSpec);
        }
        if (paramSpec.type === InteropValueType.I32 && valueType === InteropValueType.Pointer) {
            return this._memory!.readI32(valuePtr);
        }
        if (valueType !== paramSpec.type) {
            throw new Error(`failed to marshal ${stringifyParamSpec(paramSpec)} from '${INTEROP_VALUE_TYPE_NAMES[valueType] ?? 'unknown'}'`);
        }
        switch (paramSpec.type) {
            case InteropValueType.Void: return undefined;
            case InteropValueType.U1: return this._memory!.readU8(valuePtr) !== 0;
            case InteropValueType.U8: return this._memory!.readU8(valuePtr);
            case InteropValueType.I8: return this._memory!.readI8(valuePtr);
            case InteropValueType.U16: return this._memory!.readU16(valuePtr);
            case InteropValueType.I16: return this._memory!.readI16(valuePtr);
            case InteropValueType.U32: return this._memory!.readU32(valuePtr);
            case InteropValueType.I32: return this._memory!.readI32(valuePtr);
            case InteropValueType.U64: return this._memory!.readU64(valuePtr);
            case InteropValueType.I64: return this._memory!.readI64(valuePtr);
            case InteropValueType.F32: return this._memory!.readF32(valuePtr);
            case InteropValueType.F64: return this._memory!.readF64(valuePtr);
            case InteropValueType.Pointer: return this._memory!.getDataView(this._memory!.readI32(valuePtr), this._memory!.readI32(valuePtr + 8));
            case InteropValueType.String: return this.stringToJs(this._memory!.readI32(valuePtr));
            case InteropValueType.Object: return this._objects.getObjectByHandle(this._memory!.readI32(valuePtr + 4));
            case InteropValueType.Array:
                if (paramSpec.elementSpec == null) {
                    throw new Error(`malformed param spec (array with no element spec)`);
                }
                return this.arrayToJs(this._memory!.readI32(valuePtr), this._memory!.readI32(valuePtr + 8), paramSpec.elementSpec);
            case InteropValueType.Name: return this._nameList[this._memory!.readI32(valuePtr)];
            case InteropValueType.Struct: return this.structToJs(this._memory!.readI32(valuePtr), this._memory!.readI32((valuePtr + 4)), this._memory!.readI32(valuePtr + 8));
            default: throw new Error(`failed to marshal ${stringifyParamSpec(paramSpec)} from '${INTEROP_VALUE_TYPE_NAMES[valueType] ?? 'unknown'}'`);
        }
    }

    private marshalToClr(valuePtr: number, paramSpec: Readonly<ParamSpec>, value: unknown): void {
        if (value == null) {
            if (paramSpec.nullable || paramSpec.type === InteropValueType.Void) {
                this._memory!.writeU8(valuePtr, InteropValueType.Void);
                return;
            }
            throw new Error(`failed to marshal null as '${stringifyParamSpec(paramSpec)}'`);
        }
        switch (paramSpec.type) {
            case InteropValueType.Void:
                this._memory!.writeU8(valuePtr + 12, InteropValueType.Void);
                break;
            case InteropValueType.U1:
                this._memory!.writeU8(valuePtr, value ? 1 : 0);
                this._memory!.writeU8(valuePtr + 12, InteropValueType.U1);
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
            case InteropValueType.String:
                this._memory!.writeI32(valuePtr, this.stringToClr(typeof value === 'string' ? value : `${value}`));
                this._memory!.writeU8(valuePtr + 12, InteropValueType.String);
                break;
            case InteropValueType.Object:
                if (typeof value !== 'object' && typeof value !== 'function') {
                    throw new Error(`failed to marshal ${typeof value} as '${stringifyParamSpec(paramSpec)}' (not an object)`);
                }
                this._memory!.writeI32(valuePtr + 4, this._objects.getOrAssignObjectHandle(value));
                this._memory!.writeU8(valuePtr + 12, InteropValueType.Object);
                break;
            case InteropValueType.Array:
                if (paramSpec.elementSpec == null) {
                    throw new Error(`malformed param spec (array with no element spec)`);
                }
                if (!Array.isArray(value)) {
                    //value = [value];
                    // TODO: We could have a param spec flag that wraps single values in arrays in case we need to support apis that sometimes returns an array and sometimes a single value
                    throw new Error(`failed to marshal ${typeof value} as '${stringifyParamSpec(paramSpec)}' (not an array)`);
                }
                if (paramSpec.elementSpec.type === InteropValueType.String) {
                    this._memory!.writeI32(valuePtr, this.stringArrayToClr(value, paramSpec.elementSpec));
                } else {
                    this._memory!.writeI32(valuePtr, this.arrayToClr(value, paramSpec.elementSpec));
                }
                this._memory!.writeI32(valuePtr + 8, value.length);
                this._memory!.writeU8(valuePtr + 12, InteropValueType.Array);
                break;
            case InteropValueType.Name:
                const valueAsStr = typeof value === 'string' ? value : `${value}`;
                const nameIndex = this._nameTable[valueAsStr];
                if (nameIndex == null) {
                    this._memory!.writeI32(valuePtr, this.stringToClr(valueAsStr));
                    this._memory!.writeU8(valuePtr + 12, InteropValueType.String);
                } else {
                    this._memory!.writeI32(valuePtr, nameIndex);
                    this._memory!.writeU8(valuePtr + 12, InteropValueType.Name);
                }
                break;
            case InteropValueType.Struct:
                if (typeof value !== 'object' && typeof value !== 'function') {
                    throw new Error(`failed to marshal ${typeof value} as '${stringifyParamSpec(paramSpec)}' (not an object)`);
                }
                if (this._memory!.readU8(valuePtr + 12) !== InteropValueType.Struct) {
                    throw new Error(`failed to marshal ${typeof value} as '${stringifyParamSpec(paramSpec)}' (return InteropValue was not initialised correctly))`);
                }
                this.structToClr(this._memory!.readI32(valuePtr + 4), this._memory!.readI32(valuePtr), this._memory!.readI32(valuePtr + 8), value as Record<string, unknown>);
                break;
            default:
                throw new Error(`failed to marshal '${typeof value}' as '${stringifyParamSpec(paramSpec)}' (not yet implemented)`);
        }
    }

    private marshalNumericToClr(valuePtr: number, paramSpec: Readonly<ParamSpec>, value: number): void {
        switch (paramSpec.type) {
            case InteropValueType.U8: this._memory!.writeU8(valuePtr, value); break;
            case InteropValueType.I8: this._memory!.writeI8(valuePtr, value); break;
            case InteropValueType.U16: this._memory!.writeU16(valuePtr, value); break;
            case InteropValueType.I16: this._memory!.writeI16(valuePtr, value); break;
            case InteropValueType.U32: this._memory!.writeU32(valuePtr, value); break;
            case InteropValueType.I32: this._memory!.writeI32(valuePtr, value); break;
            // case InteropValueType.U64: break;
            // case InteropValueType.I64: break;
            case InteropValueType.F32: this._memory!.writeF32(valuePtr, value); break;
            case InteropValueType.F64: this._memory!.writeF64(valuePtr, value); break;
            default: throw new Error(`failed to marshal numeric as '${stringifyParamSpec(paramSpec)}' (not yet implemented)`);
        }
        this._memory!.writeU8(valuePtr + 12, paramSpec.type);
    }

    private stringToJs(stringPtr: number): string {
        try {
            this._memory!.enterConstrainedRange(stringPtr, 2 * 2 * 1024 * 1024); // assuming they will never try and copy a string larger than 2m characters to JS
            return this._memory!.readNullTerminatedString(stringPtr);
        } finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private stringToClr(str: string): number {
        const strPtr = this.allocateWasm((str.length + 1) * 2);
        try {
            this._memory!.flush();
            this._memory!.enterConstrainedRange(strPtr, (str.length + 1) * 2);
            this._memory!.writeString(strPtr, str, true);
            return strPtr;
        } finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private arrayToJs(arrayPtr: number, arrayLen: number, elementSpec: Readonly<ParamSpec>): unknown[] {
        try {
            this._memory!.enterConstrainedRange(arrayPtr, arrayLen * 16);
            const result: unknown[] = [];
            result.length = arrayLen;
            for (let i = 0; i < arrayLen; ++i) {
                result[i] = this.marshalToJs(arrayPtr, elementSpec);
                arrayPtr += 16;
            }
            return result;
        } finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private arrayToClr(value: unknown[], elementSpec: Readonly<ParamSpec>): number {
        const arrPtr = this.allocateWasm(value.length * 16);
        try {
            this._memory!.enterConstrainedRange(arrPtr, value.length * 16);
            this._memory!.flush();
            let elPtr = arrPtr;
            for (let i = 0; i < value.length; ++i) {
                this.marshalToClr(elPtr, elementSpec, value[i]);
                elPtr += 16;
            }
            return arrPtr;
        } finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private stringArrayToJs(arrayPtr: number, arrayLen: number, elementSpec: Readonly<ParamSpec>): (string | null | undefined)[] {
        try {
            this._memory!.enterConstrainedRange(arrayPtr, 2 * 2 * 1024 * 1024);
            const result: (string | null | undefined)[] = [];
            result.length = arrayLen;
            for (let i = 0; i < arrayLen; ++i) {
                let code: number;
                if (elementSpec.nullable) {
                    code = this._memory!.readU16(arrayPtr);
                    arrayPtr += 2;
                    if (code === 0) {
                        result[i] = elementSpec.nullAsUndefined ? undefined : null;
                        arrayPtr += 2; // <-- why are we advancing here? we appear to do it in C# too but I'm not sure why - maybe so we align to 4-byte boundary?
                        break;
                    }
                }
                const str = this._memory!.readNullTerminatedString(arrayPtr);
                arrayPtr += (str.length + 1) * 2;
                result[i] = str;
            }
            return result;
        } finally {
            this._memory!.exitConstrainedRange();
        }
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
        const strPtr = this.allocateWasm(bufferSize * 2);
        try {
            this._memory!.flush();
            this._memory!.enterConstrainedRange(strPtr, bufferSize * 2);
            let charPtr = strPtr;
            for (const element of value) {
                if (elementSpec.nullable) {
                    this._memory!.writeU16(charPtr, element != null ? 1 : 0);
                    charPtr += 2;
                }
                const str = typeof element === 'string' ? element : `${element}`;
                this._memory!.writeString(charPtr, str, true);
                charPtr += (str.length + 1) * 2;
            }
            return strPtr;
        }
        finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private structToClr(structIndex: number, fieldPtr: number, fieldCount: number, obj?: Record<string, unknown>): void {
        const structSpec = this._structList[structIndex];
        if (structSpec == null) {
            throw new Error(`failed to marshal struct ${structIndex} (invalid struct index)`);
        }
        const useFieldKeys = fieldCount !== structSpec.fieldSpecs.length;
        const baseFieldPtr = fieldPtr;
        try {
            this._memory!.enterConstrainedRange(fieldPtr, fieldCount * 16);
            for (let i = 0; i < fieldCount; ++i) {
                const fieldKey = useFieldKeys ? this._memory!.readI16(fieldPtr + 14) : i;
                const fieldSpec = structSpec.fieldSpecs[fieldKey];
                if (!fieldSpec) {
                    throw new Error(`failed to marshal struct ${structIndex} field ${i} to clr (field key ${fieldKey} did not refer to a field)`);
                }
                const value = obj != null ? obj[fieldSpec.fieldName] : null;
                this.marshalToClr(fieldPtr, fieldSpec.paramSpec, value);
                fieldPtr += 16;
            }
        } finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private structToJs(structIndex: number, fieldPtr: number, fieldCount: number): Record<string, unknown> {
        const structSpec = this._structList[structIndex];
        if (structSpec == null) {
            throw new Error(`failed to marshal struct ${structIndex} (invalid struct index)`);
        }
        const result: Record<string, unknown> = {};
        const useFieldKeys = fieldCount !== structSpec.fieldSpecs.length;
        try {
            this._memory!.enterConstrainedRange(fieldPtr, fieldCount * 16);
            for (let i = 0; i < fieldCount; ++i) {
                const fieldKey = useFieldKeys ? this._memory!.readI16(fieldPtr + 14) : i;
                const fieldSpec = structSpec.fieldSpecs[fieldKey];
                if (!fieldSpec) {
                    throw new Error(`failed to marshal struct ${structIndex} field ${i} from clr (field key ${fieldKey} did not refer to a field)`);
                }
                result[fieldSpec.fieldName] = this.marshalToJs(fieldPtr, fieldSpec.paramSpec);
                fieldPtr += 16;
            }
            return result;
        } finally {
            this._memory!.exitConstrainedRange();
        }
    }

    private paramSpecToJs(paramSpecPtr: number): ParamSpec {
        const type = this._memory!.readU8(paramSpecPtr);
        const flags = this._memory!.readU8(paramSpecPtr + 1);
        const elementType = this._memory!.readU8(paramSpecPtr + 2);
        const elementFlags = this._memory!.readU8(paramSpecPtr + 3);
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

    private allocateWasm(sz: number): number {
        const ptr = this._malloc!(sz);
        if (ptr === 0) { throw new Error(`failed to allocate - malloc returned nullptr`); }
        return ptr;
    }

    private stringifyValueForDisplay(value: unknown): string {
        if (value === undefined) { return 'undefined'; }
        if (value === null) { return 'null'; }
        if (typeof value === 'string') { return `'${value}'`; }
        if (typeof value === 'number' || typeof value === 'boolean') { return `${value}`; }
        if (Array.isArray(value)) { return `array[#${value.length}, %${this._objects.getObjectHandle(value)}]`; }
        if (typeof value === 'object') { return `object[#${Object.keys(value).length}, %${this._objects.getObjectHandle(value)}]`; }
        return typeof value;
    }

    private stringifyImportBindingForDisplay(importIndex: number): string {
        const boundImportSymbol = this._boundImportSymbolList[importIndex];
        return `${importIndex}: ${stringifyParamSpec(boundImportSymbol.functionSpec.returnSpec)} ${boundImportSymbol.fullName}(${boundImportSymbol.functionSpec.paramSpecs.map(stringifyParamSpec).join(', ')})`;
    }
}
