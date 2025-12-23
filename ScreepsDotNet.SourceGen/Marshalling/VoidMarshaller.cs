using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class VoidMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        public override MarshalMode CanMarshalToJS(ITypeSymbol paramTypeSymbol) => MarshalMode.Unsupported;

        public override void BeginMarshalToJS(ITypeSymbol typeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override void EndMarshalToJS(ITypeSymbol typeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override MarshalMode CanMarshalFromJS(ITypeSymbol returnTypeSymbol) => returnTypeSymbol.ToDisplayString() == "void" ? MarshalMode.Trivial : MarshalMode.Unsupported;

        public override void BeginMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string paramName, SourceEmitter emitter)
        {

        }

        public override void EndMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string paramName, SourceEmitter emitter)
        {
            
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol) => new(InteropValueType.Void, InteropValueFlags.None);

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
