using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class NumericMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        private static readonly Dictionary<string, (InteropValueType, string)> primitiveTypeToInteropData = new Dictionary<string, (InteropValueType, string)>
        {
            { "bool", (InteropValueType.U1, "AsBool") },
            { "byte", (InteropValueType.U8, "AsByte") },
            { "sbyte", (InteropValueType.I8, "AsSByte") },
            { "char", (InteropValueType.U16, "AsChar") },
            { "ushort", (InteropValueType.U16, "AsUInt16") },
            { "short", (InteropValueType.I16, "AsInt16") },
            { "uint", (InteropValueType.U32, "AsUInt32") },
            { "int", (InteropValueType.I32, "AsInt32") },
            { "ulong", (InteropValueType.U64, "AsUInt64") },
            { "long", (InteropValueType.I64, "AsInt64") },
            { "float", (InteropValueType.F32, "AsSingle") },
            { "double", (InteropValueType.F64, "AsDouble") },
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
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"return {jsParamName}.{interopData.Item2}();");
            }
            else
            {
                emitter.WriteLine($"return {jsParamName}.{interopData.Item2}Nullable();");
            }
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol) => new(primitiveTypeToInteropData[paramTypeSymbol.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString()].Item1, paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated ? InteropValueFlags.Nullable : InteropValueFlags.None);

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
