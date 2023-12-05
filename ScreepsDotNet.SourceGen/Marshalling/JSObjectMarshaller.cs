using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class JSObjectMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        public override bool CanMarshalToJS(ITypeSymbol paramTypeSymbol)
        {
            var type = paramTypeSymbol.ToDisplayString();
            return type == "ScreepsDotNet.Interop.JSObject" || type == "ScreepsDotNet.Interop.JSObject?";
        }

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                emitter.WriteLine($"{jsParamName} = {clrParamName} != null ? new({clrParamName}) : InteropValue.Void;");
            }
            else
            {
                emitter.WriteLine($"{jsParamName} = new({clrParamName});");
            }
        }

        public override void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
        {
            var type = returnTypeSymbol.ToDisplayString();
            return type == "ScreepsDotNet.Interop.JSObject" || type == "ScreepsDotNet.Interop.JSObject?";
        }

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string jsParamName, SourceEmitter emitter)
        {
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"var retObj = {jsParamName}.AsObject();");
                emitter.WriteLine($"if (retObj == null) {{ throw new NullReferenceException($\"Expecting JSObject, got null\"); }}");
                emitter.WriteLine($"return retObj;");
                return;
            }
            emitter.WriteLine($"return {jsParamName}.AsObject();");
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol) => new(InteropValueType.Obj, paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated ? InteropValueFlags.Nullable : InteropValueFlags.None);

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
