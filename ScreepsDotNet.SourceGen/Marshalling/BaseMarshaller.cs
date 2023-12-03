using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal abstract class BaseMarshaller
    {
        public abstract bool Unsafe { get; }

        public abstract bool CanMarshalToJS(IParameterSymbol paramSymbol);

        public abstract void BeginMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter);

        public abstract void EndMarshalToJS(IParameterSymbol paramSymbol, string paramName, SourceEmitter emitter);

        public abstract bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol);

        public abstract void MarshalFromJS(ITypeSymbol returnTypeSymbol, string paramName, SourceEmitter emitter);

        public abstract string GenerateParamSpec(IParameterSymbol paramSymbol);

        public abstract string GenerateReturnParamSpec(ITypeSymbol returnType);
    }
}
