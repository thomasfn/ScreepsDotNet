using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class ScreepsDotNet_Interop
{
    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "bind-import")]
    public static unsafe extern int BindImport(char* moduleNamePtr, char* importNamePtr, uint returnParamSpec, uint* paramSpecsPtr, int paramSpecsCount);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-import")]
    public static unsafe extern int InvokeImport(int importIndex, ScreepsDotNet.Interop.InteropValue* paramsBufferPtr);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "release-object-reference")]
    public static unsafe extern void ReleaseObjectReference(IntPtr jsHandle);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "set-name")]
    public static unsafe extern void SetName(int nameIndex, char* namePtr);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "define-struct")]
    public static unsafe extern int DefineStruct(int numFields, uint* encodedFieldsPtr);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-i")]
    public static extern int InvokeImport_i_i(int importIndex, int paramA);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-ii")]
    public static extern int InvokeImport_i_ii(int importIndex, int paramA, int paramB);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-iii")]
    public static extern int InvokeImport_i_iii(int importIndex, int paramA, int paramB, int paramC);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-o")]
    public static extern int InvokeImport_i_o(int importIndex, int paramA);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-oi")]
    public static extern int InvokeImport_i_oi(int importIndex, int paramA, int paramB);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-on")]
    public static extern int InvokeImport_i_on(int importIndex, int paramA, int paramB);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-oii")]
    public static extern int InvokeImport_i_oii(int importIndex, int paramA, int paramB, int paramC);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-oo")]
    public static extern int InvokeImport_i_oo(int importIndex, int paramA, int paramB);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-ooi")]
    public static extern int InvokeImport_i_ooi(int importIndex, int paramA, int paramB, int paramC);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-i-ooii")]
    public static extern int InvokeImport_i_ooii(int importIndex, int paramA, int paramB, int paramC, int paramD);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/js-bindings", EntryPoint = "invoke-d-v")]
    public static extern double InvokeImport_d_v(int importIndex);
}

namespace ScreepsDotNet.Interop
{
    [Serializable]
    public class JSException : Exception
    {
        public JSException() { }
        public JSException(string message) : base(message) { }
        public JSException(string message, Exception inner) : base(message, inner) { }
    }

    public readonly struct FunctionSpec
    {
        public const int MaxParams = 8;

        [InlineArray(MaxParams)]
        private struct ParamsData
        {
            private ParamSpec firstElement;

            public Span<ParamSpec> AsSpan => MemoryMarshal.CreateSpan(ref firstElement, MaxParams);
        }

        public readonly ParamSpec ReturnParam;
        private readonly ParamsData @params;

        public readonly ReadOnlySpan<ParamSpec> Params => @params.AsSpan;

        public FunctionSpec(ParamSpec returnParam, ReadOnlySpan<ParamSpec> @params)
        {
            ReturnParam = returnParam;
            @params.CopyTo(this.@params.AsSpan);
            this.@params[@params.Length..].Fill(new ParamSpec(InteropValueType.Void, InteropValueFlags.None));
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct InteropValue
    {
        public static readonly InteropValue Void = new() { Slot = new InteropValueImpl { Type = InteropValueType.Void } };

        [FieldOffset(0)]
        internal InteropValueImpl Slot;

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        internal struct InteropValueImpl
        {
            [FieldOffset(0)]
            internal bool BooleanValue;
            [FieldOffset(0)]
            internal byte ByteValue;
            [FieldOffset(0)]
            internal sbyte SByteValue;
            [FieldOffset(0)]
            internal char CharValue;
            [FieldOffset(0)]
            internal ushort UInt16Value;
            [FieldOffset(0)]
            internal short Int16Value;
            [FieldOffset(0)]
            internal uint UInt32Value;
            [FieldOffset(0)]
            internal int Int32Value;
            [FieldOffset(0)]
            internal ulong UInt64Value; // must be aligned to 8 because of HEAPU64 view alignment
            [FieldOffset(0)]
            internal long Int64Value; // must be aligned to 8 because of HEAPI64 view alignment
            [FieldOffset(0)]
            internal float SingleValue;
            [FieldOffset(0)]
            internal double DoubleValue; // must be aligned to 8 because of HEAPF64 view alignment
            [FieldOffset(0)]
            internal IntPtr IntPtrValue;

            [FieldOffset(4)]
            internal IntPtr JSHandle;
            [FieldOffset(4)]
            internal IntPtr GCHandle;
            [FieldOffset(4)]
            internal int StructIndex;

            [FieldOffset(8)]
            internal int Length;

            [FieldOffset(12)]
            internal InteropValueType Type;
            [FieldOffset(13)]
            internal InteropValueType ElementType;
            [FieldOffset(14)]
            internal ushort FieldKey;
        }

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(bool value) => Slot = new InteropValueImpl { BooleanValue = value, Type = InteropValueType.U1 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(byte value) => Slot = new InteropValueImpl { ByteValue = value, Type = InteropValueType.U8 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(sbyte value) => Slot = new InteropValueImpl { SByteValue = value, Type = InteropValueType.I8 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(char value) => Slot = new InteropValueImpl { CharValue = value, Type = InteropValueType.U16 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(ushort value) => Slot = new InteropValueImpl { UInt16Value = value, Type = InteropValueType.U16 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(short value) => Slot = new InteropValueImpl { Int16Value = value, Type = InteropValueType.I16 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(uint value) => Slot = new InteropValueImpl { UInt32Value = value, Type = InteropValueType.U32 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(int value) => Slot = new InteropValueImpl { Int32Value = value, Type = InteropValueType.I32 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(ulong value) => Slot = new InteropValueImpl { UInt64Value = value, Type = InteropValueType.U64 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(long value) => Slot = new InteropValueImpl { Int64Value = value, Type = InteropValueType.I64 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(float value) => Slot = new InteropValueImpl { SingleValue = value, Type = InteropValueType.F32 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(double value) => Slot = new InteropValueImpl { DoubleValue = value, Type = InteropValueType.F64 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(IntPtr value) => Slot = new InteropValueImpl { IntPtrValue = value, Type = InteropValueType.Pointer };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe InteropValue(char* value, int length, bool isArray) => Slot = new InteropValueImpl { IntPtrValue = (IntPtr)value, Type = isArray ? InteropValueType.Array : InteropValueType.String, Length = length, ElementType = isArray ? InteropValueType.String : InteropValueType.U16 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(JSObject value) => Slot = new InteropValueImpl { JSHandle = value.JSHandle, Type = InteropValueType.Object };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe InteropValue(InteropValue* value, int length) => Slot = new InteropValueImpl { IntPtrValue = (IntPtr)value, Type = InteropValueType.Array, Length = length };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe InteropValue(void* value, int length) => Slot = new InteropValueImpl { IntPtrValue = (IntPtr)value, Type = InteropValueType.Pointer, Length = length };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe InteropValue(InteropValue* fields, int structIndex, int fieldCount) => Slot = new InteropValueImpl { IntPtrValue = (IntPtr)fields, StructIndex = structIndex, Type = InteropValueType.Struct, Length = fieldCount };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(Name name) => Slot = new InteropValueImpl { Int32Value = name.NameIndex, Type = InteropValueType.Name };

        #endregion

        #region Accessors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AsBool()
        {
            Debug.Assert(Slot.Type == InteropValueType.U1);
            return Slot.BooleanValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool? AsBoolNullable() => Slot.Type == InteropValueType.Void ? null : AsBool();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly byte AsByte()
        {
            Debug.Assert(Slot.Type == InteropValueType.U8);
            return Slot.ByteValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly byte? AsByteNullable() => Slot.Type == InteropValueType.Void ? null : AsByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly sbyte AsSByte()
        {
            Debug.Assert(Slot.Type == InteropValueType.I8);
            return Slot.SByteValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly sbyte? AsSByteNullable() => Slot.Type == InteropValueType.Void ? null : AsSByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly char AsChar()
        {
            Debug.Assert(Slot.Type == InteropValueType.U16);
            return Slot.CharValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly char? AsCharNullable() => Slot.Type == InteropValueType.Void ? null : AsChar();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ushort AsUInt16()
        {
            Debug.Assert(Slot.Type == InteropValueType.U16);
            return Slot.UInt16Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ushort? AsUInt16Nullable() => Slot.Type == InteropValueType.Void ? null : AsUInt16();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly short AsInt16()
        {
            Debug.Assert(Slot.Type == InteropValueType.I16);
            return Slot.Int16Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly short? AsInt16Nullable() => Slot.Type == InteropValueType.Void ? null : AsInt16();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly uint AsUInt32()
        {
            Debug.Assert(Slot.Type == InteropValueType.U32);
            return Slot.UInt32Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly uint? AsUInt32Nullable() => Slot.Type == InteropValueType.Void ? null : AsUInt32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int AsInt32()
        {
            Debug.Assert(Slot.Type == InteropValueType.I32);
            return Slot.Int32Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int? AsInt32Nullable() => Slot.Type == InteropValueType.Void ? null : AsInt32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ulong AsUInt64()
        {
            Debug.Assert(Slot.Type == InteropValueType.U64);
            return Slot.UInt64Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ulong? AsUInt64Nullable() => Slot.Type == InteropValueType.Void ? null : AsUInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly long AsInt64()
        {
            Debug.Assert(Slot.Type == InteropValueType.I64);
            return Slot.Int64Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly long? AsInt64Nullable() => Slot.Type == InteropValueType.Void ? null : AsInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float AsSingle()
        {
            Debug.Assert(Slot.Type == InteropValueType.F32);
            return Slot.SingleValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float? AsSingleNullable() => Slot.Type == InteropValueType.Void ? null : AsSingle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly double AsDouble()
        {
            Debug.Assert(Slot.Type == InteropValueType.F64);
            return Slot.DoubleValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly double? AsDoubleNullable() => Slot.Type == InteropValueType.Void ? null : AsDouble();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly IntPtr AsIntPtr()
        {
            Debug.Assert(Slot.Type == InteropValueType.Void || Slot.Type == InteropValueType.Pointer);
            return Slot.Type == InteropValueType.Void ? IntPtr.Zero : Slot.IntPtrValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe string? AsString()
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.String || Slot.Type == InteropValueType.Name);
            if (Slot.Type == InteropValueType.Name) { return Name.GetNameValue(Slot.Int32Value); }
            return new string((char*)Slot.IntPtrValue, 0, Slot.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe bool AsArray(out ReadOnlySpan<InteropValue> arrayData)
        {
            if (Slot.Type == InteropValueType.Void)
            {
                arrayData = default;
                return false;
            }
            Debug.Assert(Slot.Type == InteropValueType.Array);
            arrayData = new((void*)Slot.IntPtrValue, Slot.Length);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe Name AsName()
        {
            Debug.Assert(Slot.Type == InteropValueType.String || Slot.Type == InteropValueType.Name);
            if (Slot.Type == InteropValueType.String)
            {
                var str = new string((char*)Slot.IntPtrValue, 0, Slot.Length);
                var name = Name.Create(str);
                Name.CopyIfNeeded(name.NameIndex);
                return name;
            }
            return new Name(Slot.Int32Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe Name? AsNameNullable() => Slot.Type == InteropValueType.Void ? null : AsName();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe string[]? AsStringArray()
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.Array);
            Debug.Assert(Slot.ElementType == InteropValueType.String);
            string[] arr = new string[Slot.Length];
            char* head = (char*)Slot.IntPtrValue;
            for (int i = 0; i < Slot.Length; ++i)
            {
                var str = new string(head);
                head += str.Length + 1;
                arr[i] = str;
            }
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe string?[]? AsNullableStringArray()
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.Array);
            Debug.Assert(Slot.ElementType == InteropValueType.String);
            string?[] arr = new string[Slot.Length];
            char* head = (char*)Slot.IntPtrValue;
            for (int i = 0; i < Slot.Length; ++i)
            {
                char nullCode = head[0];
                ++head;
                if (nullCode == 0)
                {
                    arr[i] = null;
                    continue;
                }
                var str = new string(head);
                head += str.Length + 1;
                arr[i] = str;
            }
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly JSObject? AsObject()
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.Object);
            return Native.GetJSObject(Slot.JSHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsVoid() => Slot.Type == InteropValueType.Struct;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsStruct() => Slot.Type == InteropValueType.Struct;

        #endregion
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    public static class Native
    {
        public const int InvokeImportReturnValArgIndex = 0;
        public const int InvokeImportExceptionArgIndex = 1;
        public const int InvokeImportArgsReserveCount = 2;

        private const int MaxJSHandles = 1024 * 1024;

        private struct JSObjectSlot
        {
            public GCHandle GCHandle;
            public int Generation;
            public bool Allocated;
        }

        private static JSObjectSlot[] jsObjectSlots = new JSObjectSlot[1024];
        private static readonly List<(IntPtr jsHandle, int generation)> pendingFreeNative = new(1024);

        internal static JSObject GetJSObject(IntPtr jsHandle)
        {
            if (jsHandle < 0 || jsHandle >= MaxJSHandles) { throw new ArgumentOutOfRangeException(nameof(jsHandle)); }
            if (jsHandle >= jsObjectSlots.Length)
            {
                uint newCapacity = BitOperations.RoundUpToPowerOf2((uint)(jsHandle + 1));
                Array.Resize(ref jsObjectSlots, (int)newCapacity);
            }
            ref var slot = ref jsObjectSlots[jsHandle];
            if (slot.Allocated && slot.GCHandle.IsAllocated && slot.GCHandle.Target is JSObject existingObj && existingObj.Generation == slot.Generation) { return existingObj; }
            if (slot.GCHandle.IsAllocated) { slot.GCHandle.Free(); }
            ++slot.Generation;
            var newObj = new JSObject(jsHandle, slot.Generation);
            slot.GCHandle = GCHandle.Alloc(newObj, GCHandleType.Weak);
            slot.Allocated = true;
            return newObj;
        }

        internal static void ReleaseJSObject(IntPtr jsHandle, int generation)
        {
            if (jsHandle < 0 || jsHandle >= jsObjectSlots.Length) { throw new ArgumentOutOfRangeException(nameof(jsHandle)); }
            ref var slot = ref jsObjectSlots[jsHandle];
            if (!slot.Allocated || slot.Generation != generation) { return; }
            if (slot.GCHandle.IsAllocated) { slot.GCHandle.Free(); }
            slot.Allocated = false;
            pendingFreeNative.Add((jsHandle, generation));
        }

        internal static void ReleasePendingJSObjects()
        {
            foreach (var (jsHandle, generation) in pendingFreeNative)
            {
                if (jsHandle < 0 || jsHandle >= jsObjectSlots.Length) { continue; }
                ref var slot = ref jsObjectSlots[jsHandle];
                if (generation != slot.Generation) { continue; }
                ScreepsDotNet_Interop.ReleaseObjectReference(jsHandle);
            }
            pendingFreeNative.Clear();
        }

        internal static void PruneJSObjects()
        {
            int numAllocated = 0, numCollected = 0, numUnused = 0;
            for (int i = 0; i < jsObjectSlots.Length; ++i)
            {
                ref var slot = ref jsObjectSlots[i];
                if (slot.Allocated)
                {
                    if (slot.GCHandle.Target is JSObject obj && obj.Generation == slot.Generation)
                    {
                        ++numAllocated;
                        continue;
                    }
                    if (slot.GCHandle.IsAllocated) { slot.GCHandle.Free(); }
                    ScreepsDotNet_Interop.ReleaseObjectReference(i);
                    slot.Allocated = false;
                    ++numCollected;
                    continue;
                }
                else if (slot.Generation > 0)
                {
                    ++numUnused;
                }
            }
            // Console.WriteLine($"Interop: pruned {numCollected} js handle slots ({numAllocated} allocated, {numUnused} available, {jsObjectSlots.Length} total)");
        }

        public static unsafe int DefineStruct(ReadOnlySpan<StructFieldSpec> fieldSpecs)
        {
            Span<uint> encodedFieldSpecs = stackalloc uint[fieldSpecs.Length * 2];
            for (int i = 0; i < fieldSpecs.Length; ++i)
            {
                encodedFieldSpecs[i * 2 + 0] = (uint)fieldSpecs[i].NamePtr;
                encodedFieldSpecs[i * 2 + 1] = fieldSpecs[i].ParamSpec.Encoded;
            }
            fixed (uint* encodedFieldSpecsPtr = encodedFieldSpecs)
            {
                return ScreepsDotNet_Interop.DefineStruct(fieldSpecs.Length, encodedFieldSpecsPtr);
            }
        }

        public static unsafe int BindImport(string importName, string moduleName, in FunctionSpec functionSpec)
        {
            int result;
            fixed (char* importNamePtr = importName)
            {
                fixed (char* moduleNamePtr = moduleName)
                {
                    Span<uint> encodedParamSpecs = stackalloc uint[FunctionSpec.MaxParams];
                    for (int i = 0; i < FunctionSpec.MaxParams; ++i)
                    {
                        encodedParamSpecs[i] = functionSpec.Params[i].Encoded;
                    }
                    fixed (uint* encodedParamSpecsPtr = encodedParamSpecs)
                    {
                        result = ScreepsDotNet_Interop.BindImport(moduleNamePtr, importNamePtr, functionSpec.ReturnParam.Encoded, encodedParamSpecsPtr, FunctionSpec.MaxParams);
                    }
                }
            }
            if (result < 0) { throw new InvalidOperationException($"Failed to bind import"); }
            return result;
        }

        public static unsafe InteropValue InvokeImport(int importIndex, Span<InteropValue> args)
        {
            int result;
            fixed (InteropValue* paramsBufferPtr = args)
            {
                result = ScreepsDotNet_Interop.InvokeImport(importIndex, paramsBufferPtr);
            }
            if (result == 0)
            {
                if (args[InvokeImportExceptionArgIndex].Slot.Type == InteropValueType.String)
                {
                    string? errorText = args[InvokeImportExceptionArgIndex].AsString();
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        throw new JSException(errorText);
                    }
                }
                throw new JSException($"Imported method threw an error");
            }
            else
            {
                return args[InvokeImportReturnValArgIndex];
            }
        }

        public static unsafe IntPtr StringToJs(string value)
        {
            fixed (char* valuePtr = value)
            {
                return ScreepsDotNet_Interop.StringToJs(valuePtr, value.Length);
            }
        }

        public static unsafe string StringFromJs(IntPtr handle, int length)
        {
            if (length == 0) { return ""; }
            var buffer = length <= 1024 ? stackalloc char[length] : new char[length];
            int actualLength;
            fixed (char* bufferPtr = buffer)
            {
                actualLength = ScreepsDotNet_Interop.StringFromJs(handle, bufferPtr);
                if (actualLength > length) { throw new InvalidOperationException($"StringFromJs returned length larger than buffer size, heap corruption likely"); }
            }
            return new string(buffer[..actualLength]);
        }
    }
}