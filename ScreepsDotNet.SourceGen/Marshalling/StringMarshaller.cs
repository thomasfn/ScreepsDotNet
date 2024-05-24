using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class StringMarshaller : BaseMarshaller
    {
        public override bool Unsafe => true;

        public override bool CanMarshalToJS(ITypeSymbol paramTypeSymbol)
        {
            var type = paramTypeSymbol.ToDisplayString();
            return type == "string" || type == "string?";
        }

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            emitter.WriteLine($"fixed (char* {clrParamName}Ptr = {clrParamName})");
            emitter.OpenScope();
            emitter.WriteLine($"{jsParamName} = new({clrParamName}Ptr);");
        }

        public override void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            emitter.CloseScope();
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
        {
            var type = returnTypeSymbol.ToDisplayString();
            return type == "string" || type == "string?";
        }

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string jsParamName, SourceEmitter emitter)
        {
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"var retObj = {jsParamName}.AsString();");
                emitter.WriteLine($"if (retObj == null) {{ throw new NullReferenceException($\"Expecting string, got null\"); }}");
                emitter.WriteLine($"return retObj;");
                return;
            }
            emitter.WriteLine($"return {jsParamName}.AsString();");
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol) => new(InteropValueType.Str, paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated ? InteropValueFlags.Nullable : InteropValueFlags.None);

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
