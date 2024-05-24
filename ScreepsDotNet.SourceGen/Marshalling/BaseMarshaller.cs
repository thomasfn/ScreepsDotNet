using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal abstract class BaseMarshaller
    {
        public abstract bool Unsafe { get; }

        public virtual bool CanMarshalToJS(IParameterSymbol paramSymbol) => CanMarshalToJS(paramSymbol.Type);

        public abstract bool CanMarshalToJS(ITypeSymbol paramTypeSymbol);

        public abstract void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter);

        public abstract void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter);

        public abstract bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol);

        public abstract void MarshalFromJS(ITypeSymbol returnTypeSymbol, string jsParamName, SourceEmitter emitter);

        public virtual ParamSpec GenerateParamSpec(IParameterSymbol paramSymbol) => GenerateParamSpec(paramSymbol.Type);

        public abstract ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol);

        public abstract ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol);
    }
}
