using Microsoft.CodeAnalysis;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class JSObjectMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        public override bool CanMarshalToJS(IParameterSymbol paramSymbol)
        {
            var type = paramSymbol.Type.ToDisplayString();
            return type == "ScreepsDotNet.Interop.JSObject" || type == "ScreepsDotNet.Interop.JSObject?";
        }

        public override void BeginMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter)
        {
            if (paramSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                emitter.WriteLine($"{paramName} = {paramSymbol.Name} != null ? new({paramSymbol.Name}) : InteropValue.Void;");
            }
            else
            {
                emitter.WriteLine($"{paramName} = new({paramSymbol.Name});");
            }
        }

        public override void EndMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter)
        {
            
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
        {
            var type = returnTypeSymbol.ToDisplayString();
            return type == "ScreepsDotNet.Interop.JSObject" || type == "ScreepsDotNet.Interop.JSObject?";
        }

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string paramName, SourceEmitter emitter)
        {
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"var retObj = {paramName}.AsObject();");
                emitter.WriteLine($"if (retObj == null) {{ throw new NullReferenceException($\"Expecting JSObject, got null\"); }}");
                emitter.WriteLine($"return retObj;");
                return;
            }
            emitter.WriteLine($"return {paramName}.AsObject();");
        }

        public override string GenerateParamSpec(IParameterSymbol paramSymbol) => $"new(InteropValueType.Obj, InteropValueFlags.{(paramSymbol.NullableAnnotation == NullableAnnotation.Annotated ? "Nullable" : "None")})";

        public override string GenerateReturnParamSpec(ITypeSymbol returnType) => $"new(InteropValueType.Obj, InteropValueFlags.{(returnType.NullableAnnotation == NullableAnnotation.Annotated ? "Nullable" : "None")})";
    }
}
