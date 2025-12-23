using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    public enum MarshalMode
    {
        /// <summary>
        /// Marshalling the specified type is unsupported by this marshaller.
        /// </summary>
        Unsupported,
        /// <summary>
        /// Marshalling the specified type can be done trivially (single unscoped call to BeginMarshalToJS / EndMarshalFromJS before / after native invoke).
        /// </summary>
        Trivial,
        /// <summary>
        /// Marshalling the specified type can be done non-trivially (scoped calls to BeginMarshal[To/From] EndMarshal[To/From]JS surrounding native invoke).
        /// </summary>
        Scoped
    }

    internal abstract class BaseMarshaller
    {
        public abstract bool Unsafe { get; }

        public virtual MarshalMode CanMarshalToJS(IParameterSymbol paramSymbol) => CanMarshalToJS(paramSymbol.Type);

        public abstract MarshalMode CanMarshalToJS(ITypeSymbol paramTypeSymbol);

        public abstract void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter);

        public abstract void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter);

        public abstract MarshalMode CanMarshalFromJS(ITypeSymbol returnTypeSymbol);

        public abstract void BeginMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter);

        public abstract void EndMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter);

        public virtual ParamSpec GenerateParamSpec(IParameterSymbol paramSymbol) => GenerateParamSpec(paramSymbol.Type);

        public abstract ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol);

        public abstract ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol);
    }
}
