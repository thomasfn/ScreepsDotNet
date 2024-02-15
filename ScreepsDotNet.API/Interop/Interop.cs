using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class ScreepsDotNet_Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static unsafe extern int BindImport(char* moduleNamePtr, char* importNamePtr, ScreepsDotNet.Interop.FunctionSpec* importSpecPtr);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static unsafe extern int InvokeImport(int importIndex, ScreepsDotNet.Interop.InteropValue* paramsBufferPtr);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static unsafe extern int ReleaseObjectReference(IntPtr jsHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static unsafe extern void SetName(int nameIndex, char* namePtr);
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

    [InlineArray(8)]
    public struct FunctionParamsSpec
    {
        private ParamSpec P0;
        public Span<ParamSpec> Params => MemoryMarshal.CreateSpan(ref P0, 8);
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 36)]
    public struct FunctionSpec
    {
        [FieldOffset(0)]
        public ParamSpec ReturnParamSpec;
        [FieldOffset(4)]
        public FunctionParamsSpec ParamSpecs;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    public struct InteropValue
    {
        public static readonly InteropValue Void = new InteropValue { Slot = new InteropValueImpl { Type = InteropValueType.Void } };

        internal InteropValueImpl Slot;

        [StructLayout(LayoutKind.Explicit, Pack = 16, Size = 16)]
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

            [FieldOffset(8)]
            internal int Length;

            [FieldOffset(12)]
            internal InteropValueType Type;
            [FieldOffset(13)]
            internal InteropValueType ElementType;
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
        public InteropValue(IntPtr value) => Slot = new InteropValueImpl { IntPtrValue = value, Type = InteropValueType.Ptr };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe InteropValue(char* value) => Slot = new InteropValueImpl { IntPtrValue = (IntPtr)value, Type = InteropValueType.Str };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe InteropValue(char* value, int arrayLength) => Slot = new InteropValueImpl { IntPtrValue = (IntPtr)value, Type = InteropValueType.Arr, Length = arrayLength };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(JSObject value) => Slot = new InteropValueImpl { JSHandle = value.JSHandle, Type = InteropValueType.Obj };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe InteropValue(InteropValue* value, int length) => Slot = new InteropValueImpl { IntPtrValue = (IntPtr)value, Type = InteropValueType.Arr, Length = length };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe InteropValue(void* value, int length) => Slot = new InteropValueImpl { IntPtrValue = (IntPtr)value, Type = InteropValueType.Ptr, Length = length };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InteropValue(Name name) => Slot = new InteropValueImpl { Int32Value = name.NameIndex, Type = InteropValueType.Nme };

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
            Debug.Assert(Slot.Type == InteropValueType.Void || Slot.Type == InteropValueType.Ptr);
            return Slot.Type == InteropValueType.Void ? IntPtr.Zero : Slot.IntPtrValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe string? AsString(bool freeMem = true)
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.Str || Slot.Type == InteropValueType.Nme);
            if (Slot.Type == InteropValueType.Nme) { return Name.GetNameValue(Slot.Int32Value); }
            var str = new string((char*)Slot.IntPtrValue);
            if (freeMem) { Marshal.FreeHGlobal(Slot.IntPtrValue); }
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe T[]? AsArray<T>(Func<InteropValue, T> elementMarshaller, bool freeMem = true)
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.Arr);
            ReadOnlySpan<InteropValue> valueArr = new((void*)Slot.IntPtrValue, Slot.Length);
            T[] arr = new T[Slot.Length];
            for (int i = 0; i < Slot.Length; ++i)
            {
                arr[i] = elementMarshaller(valueArr[i]);
            }
            if (freeMem) { Marshal.FreeHGlobal(Slot.IntPtrValue); }
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe Name AsName(bool freeMem = true)
        {
            Debug.Assert(Slot.Type == InteropValueType.Str || Slot.Type == InteropValueType.Nme);
            if (Slot.Type == InteropValueType.Str)
            {
                var str = new string((char*)Slot.IntPtrValue);
                if (freeMem) { Marshal.FreeHGlobal(Slot.IntPtrValue); }
                return Name.Create(str);
            }
            return new Name(Slot.Int32Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe Name? AsNameNullable() => Slot.Type == InteropValueType.Void ? null : AsName();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe string[]? AsStringArray(bool freeMem = true)
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.Arr);
            string[] arr = new string[Slot.Length];
            char* head = (char*)Slot.IntPtrValue;
            for (int i = 0; i < Slot.Length; ++i)
            {
                var str = new string(head);
                head += str.Length + 1;
                arr[i] = str;
            }
            if (freeMem) { Marshal.FreeHGlobal(Slot.IntPtrValue); }
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly unsafe string?[]? AsNullableStringArray(bool freeMem = true)
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.Arr);
            string?[] arr = new string[Slot.Length];
            char* head = (char*)Slot.IntPtrValue;
            for (int i = 0; i < Slot.Length; ++i)
            {
                char nullCode = head[0];
                ++head;
                if (nullCode == 0)
                {
                    arr[i] = null;
                    ++head;
                    continue;
                }
                var str = new string(head);
                head += str.Length + 1;
                arr[i] = str;
            }
            if (freeMem) { Marshal.FreeHGlobal(Slot.IntPtrValue); }
            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly JSObject? AsObject()
        {
            if (Slot.Type == InteropValueType.Void) { return null; }
            Debug.Assert(Slot.Type == InteropValueType.Obj);
            return Native.GetJSObject(Slot.JSHandle);
        }

        #endregion
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    public static class Native
    {
        private static readonly Dictionary<IntPtr, WeakReference<JSObject>> trackedJSObjects = new();
        
        internal static JSObject GetJSObject(IntPtr jsHandle)
        {
            if (trackedJSObjects.TryGetValue(jsHandle, out var objRef))
            {
                if (objRef.TryGetTarget(out var obj) && obj != null) { return obj; }
                obj = new(jsHandle);
                objRef.SetTarget(obj);
                return obj;
            }
            else
            {
                var obj = new JSObject(jsHandle);
                trackedJSObjects.Add(jsHandle, new WeakReference<JSObject>(obj));
                return obj;
            }
        }

        internal static void ReleaseJSObject(IntPtr jsHandle)
        {
            trackedJSObjects.Remove(jsHandle);
            ScreepsDotNet_Interop.ReleaseObjectReference(jsHandle);
        }

        public static unsafe int BindImport(string importName, string moduleName, FunctionSpec functionSpec)
        {
            int result;
            fixed (char* importNamePtr = importName)
            {
                fixed (char* moduleNamePtr = moduleName)
                {
                    result = ScreepsDotNet_Interop.BindImport(moduleNamePtr, importNamePtr, &functionSpec);
                }
            }
            if (result < 0) { throw new InvalidOperationException($"Failed to bind import"); }
            return result;
        }

        public static unsafe InteropValue InvokeImport(int importIndex, Span<InteropValue> args)
        {
            Span<InteropValue> paramsBuffer = stackalloc InteropValue[args.Length + 2];
            if (args.Length > 0) { args.CopyTo(paramsBuffer[2..]); }

            ref InteropValue returnVal = ref paramsBuffer[0];
            ref InteropValue exceptionVal = ref paramsBuffer[1];

            int result;
            fixed (InteropValue* paramsBufferPtr = paramsBuffer)
            {
                result = ScreepsDotNet_Interop.InvokeImport(importIndex, paramsBufferPtr);
            }
            if (result == 0)
            {
                if (exceptionVal.Slot.Type == InteropValueType.Str)
                {
                    string? errorText = exceptionVal.AsString();
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        throw new JSException(errorText);
                    }
                }
                throw new JSException($"Imported method threw an error");
            }
            else
            {
                return returnVal;
            }

        }
    }
}