using Microsoft.CodeAnalysis;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class VoidMarshaller : BaseMarshaller
    {
        public override bool Unsafe => false;

        public override bool CanMarshalToJS(IParameterSymbol paramSymbol) => false;

        public override void BeginMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter)
        {
            
        }

        public override void EndMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter)
        {
            
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol) => returnTypeSymbol.ToDisplayString() == "void";

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string paramName, SourceEmitter emitter)
        {
            
        }

        public override string GenerateParamSpec(IParameterSymbol paramSymbol) => $"new(InteropValueType.Void, InteropValueFlags.None)";

        public override string GenerateReturnParamSpec(ITypeSymbol returnType) => $"new(InteropValueType.Void, InteropValueFlags.None)";
    }
}
