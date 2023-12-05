using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class VoidMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        public override bool CanMarshalToJS(ITypeSymbol paramTypeSymbol) => false;

        public override void BeginMarshalToJS(ITypeSymbol typeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override void EndMarshalToJS(ITypeSymbol typeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol) => returnTypeSymbol.ToDisplayString() == "void";

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string paramName, SourceEmitter emitter)
        {
            
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol) => new(InteropValueType.Void, InteropValueFlags.None);

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
