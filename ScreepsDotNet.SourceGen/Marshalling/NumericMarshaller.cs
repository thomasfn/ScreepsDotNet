using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class NumericMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        private static readonly Dictionary<string, (string, string)> primitiveTypeToInteropData = new Dictionary<string, (string, string)>
        {
            { "bool", ("U1", "AsBool") },
            { "byte", ("U8", "AsByte") },
            { "sbyte", ("I8", "AsSByte") },
            { "char", ("U16", "AsChar") },
            { "ushort", ("U16", "AsUInt16") },
            { "short", ("I16", "AsInt16") },
            { "uint", ("U32", "AsUInt32") },
            { "int", ("I32", "AsInt32") },
            { "ulong", ("U64", "AsUInt64") },
            { "long", ("I64", "AsInt64") },
            { "float", ("F32", "AsSingle") },
            { "double", ("F64", "AsDouble") },
        };

        public override bool CanMarshalToJS(IParameterSymbol paramSymbol) => primitiveTypeToInteropData.ContainsKey(paramSymbol.Type.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString());

        public override void BeginMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter)
        {
            if (paramSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                emitter.WriteLine($"{paramName} = {paramSymbol.Name} != null ? new({paramSymbol.Name}.Value) : InteropValue.Void;");
            }
            else
            {
                emitter.WriteLine($"{paramName} = new({paramSymbol.Name});");
            }
        }

        public override void EndMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter)
        {
            
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol) => primitiveTypeToInteropData.ContainsKey(returnTypeSymbol.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString());

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string paramName, SourceEmitter emitter)
        {
            var interopData = primitiveTypeToInteropData[returnTypeSymbol.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString()];
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"return {paramName}.{interopData.Item2}();");
            }
            else
            {
                emitter.WriteLine($"return {paramName}.{interopData.Item2}Nullable();");
            }
        }

        public override string GenerateParamSpec(IParameterSymbol paramSymbol) => $"new(InteropValueType.{primitiveTypeToInteropData[paramSymbol.Type.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString()].Item1}, InteropValueFlags.{(paramSymbol.NullableAnnotation == NullableAnnotation.Annotated ? "Nullable" : "None")})";

        public override string GenerateReturnParamSpec(ITypeSymbol returnType) => $"new(InteropValueType.{primitiveTypeToInteropData[returnType.WithNullableAnnotation(NullableAnnotation.None).ToDisplayString()].Item1}, InteropValueFlags.{(returnType.NullableAnnotation == NullableAnnotation.Annotated ? "Nullable" : "None")})";
    }
}
