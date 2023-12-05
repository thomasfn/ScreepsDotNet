using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class NumericMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        private static readonly Dictionary<string, (InteropValueType, InteropValueFlags, string)> primitiveTypeToInteropData = new()
        {
            { "bool", (InteropValueType.U1, InteropValueFlags.None, "AsBool") },
            { "byte", (InteropValueType.U8, InteropValueFlags.None, "AsByte") },
            { "sbyte", (InteropValueType.I8, InteropValueFlags.None, "AsSByte") },
            { "char", (InteropValueType.U16, InteropValueFlags.None, "AsChar") },
            { "ushort", (InteropValueType.U16, InteropValueFlags.None, "AsUInt16") },
            { "short", (InteropValueType.I16, InteropValueFlags.None, "AsInt16") },
            { "uint", (InteropValueType.U32, InteropValueFlags.None, "AsUInt32") },
            { "int", (InteropValueType.I32, InteropValueFlags.None, "AsInt32") },
            { "ulong", (InteropValueType.U64, InteropValueFlags.None, "AsUInt64") },
            { "long", (InteropValueType.I64, InteropValueFlags.None, "AsInt64") },
            { "float", (InteropValueType.F32, InteropValueFlags.None, "AsSingle") },
            { "double", (InteropValueType.F64, InteropValueFlags.None, "AsDouble") },
            { "bool?", (InteropValueType.U1, InteropValueFlags.Nullable, "AsBoolNullable") },
            { "byte?", (InteropValueType.U8, InteropValueFlags.Nullable, "AsByteNullable") },
            { "sbyte?", (InteropValueType.I8, InteropValueFlags.Nullable, "AsSByteNullable") },
            { "char?", (InteropValueType.U16, InteropValueFlags.Nullable, "AsCharNullable") },
            { "ushort?", (InteropValueType.U16, InteropValueFlags.Nullable, "AsUInt16Nullable") },
            { "short?", (InteropValueType.I16, InteropValueFlags.Nullable, "AsInt16Nullable") },
            { "uint?", (InteropValueType.U32, InteropValueFlags.Nullable, "AsUInt32Nullable") },
            { "int?", (InteropValueType.I32, InteropValueFlags.Nullable, "AsInt32Nullable") },
            { "ulong?", (InteropValueType.U64, InteropValueFlags.Nullable, "AsUInt64Nullable") },
            { "long?", (InteropValueType.I64, InteropValueFlags.Nullable, "AsInt64Nullable") },
            { "float?", (InteropValueType.F32, InteropValueFlags.Nullable, "AsSingleNullable") },
            { "double?", (InteropValueType.F64, InteropValueFlags.Nullable, "AsDoubleNullable") },
        };

        public override bool CanMarshalToJS(ITypeSymbol paramTypeSymbol) => primitiveTypeToInteropData.ContainsKey(paramTypeSymbol.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString());

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                emitter.WriteLine($"{jsParamName} = {clrParamName} != null ? new({clrParamName}.Value) : InteropValue.Void;");
            }
            else
            {
                emitter.WriteLine($"{jsParamName} = new({clrParamName});");
            }
        }

        public override void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol) => primitiveTypeToInteropData.ContainsKey(returnTypeSymbol.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString());

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string jsParamName, SourceEmitter emitter)
        {
            var interopData = primitiveTypeToInteropData[returnTypeSymbol.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString()];
            emitter.WriteLine($"return {jsParamName}.{interopData.Item3}();");
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol)
        {
            var interopData = primitiveTypeToInteropData[paramTypeSymbol.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString()];
            return new(interopData.Item1, interopData.Item2);
        }

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
