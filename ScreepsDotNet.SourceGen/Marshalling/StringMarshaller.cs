using Microsoft.CodeAnalysis;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class StringMarshaller : BaseMarshaller
    {
        public override bool Unsafe => true;

        public override bool CanMarshalToJS(IParameterSymbol paramSymbol)
        {
            var type = paramSymbol.Type.ToDisplayString();
            return type == "string" || type == "string?";
        }

        public override void BeginMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter)
        {
            emitter.WriteLine($"fixed (char* {paramSymbol.Name}Ptr = {paramSymbol.Name})");
            emitter.OpenScope();
            emitter.WriteLine($"{paramName} = new({paramSymbol.Name}Ptr);");
        }

        public override void EndMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter)
        {
            emitter.CloseScope();
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
        {
            var type = returnTypeSymbol.ToDisplayString();
            return type == "string" || type == "string?";
        }

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string paramName, SourceEmitter emitter)
        {
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"var retObj = {paramName}.AsString();");
                emitter.WriteLine($"if (retObj == null) {{ throw new NullReferenceException($\"Expecting string, got null\"); }}");
                emitter.WriteLine($"return retObj;");
                return;
            }
            emitter.WriteLine($"return {paramName}.AsString();");
        }

        public override string GenerateParamSpec(IParameterSymbol paramSymbol) => $"new(InteropValueType.Str, InteropValueFlags.{(paramSymbol.NullableAnnotation == NullableAnnotation.Annotated ? "Nullable" : "None")})";

        public override string GenerateReturnParamSpec(ITypeSymbol returnType) => $"new(InteropValueType.Str, InteropValueFlags.{(returnType.NullableAnnotation == NullableAnnotation.Annotated ? "Nullable" : "None")})";
    }
}
