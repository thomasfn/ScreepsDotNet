using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class JSObjectMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        public override MarshalMode CanMarshalToJS(ITypeSymbol paramTypeSymbol)
        {
            var type = paramTypeSymbol.ToDisplayString();
            return (type == "ScreepsDotNet.Interop.JSObject" || type == "ScreepsDotNet.Interop.JSObject?") ? MarshalMode.Trivial : MarshalMode.Unsupported;
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

        public override MarshalMode CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
        {
            var type = returnTypeSymbol.ToDisplayString();
            return (type == "ScreepsDotNet.Interop.JSObject" || type == "ScreepsDotNet.Interop.JSObject?") ? MarshalMode.Trivial : MarshalMode.Unsupported;
        }

        public override void BeginMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override void EndMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"var retObj = {jsParamName}.AsObject();");
                emitter.WriteLine($"if (retObj == null) {{ throw new NullReferenceException($\"Expecting JSObject, got null\"); }}");
                emitter.WriteLine($"{clrParamName} = retObj;");
                return;
            }
            emitter.WriteLine($"return {jsParamName}.AsObject();");
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol) => new(InteropValueType.Object, paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated ? InteropValueFlags.Nullable : InteropValueFlags.None);

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);


    }
}
