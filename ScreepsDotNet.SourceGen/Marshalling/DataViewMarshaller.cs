using System.Linq;

using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class DataViewMarshaller : BaseMarshaller
    {
        private enum WrapperType
        {
            Array,
            Span,
            ReadOnlySpan,
        }

        private readonly struct MarshallingData
        {
            public readonly ITypeSymbol ElementType;
            public readonly WrapperType WrapperType;

            public MarshallingData(ITypeSymbol elementType, WrapperType wrapperType, bool wrapperTypeNullable)
            {
                ElementType = elementType;
                WrapperType = wrapperType;
            }
        }

        public override bool Unsafe => true;

        private bool CanMarshalAsDataView(ITypeSymbol typeSymbol, out MarshallingData outMarshallingData)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                outMarshallingData = new(arrayTypeSymbol.ElementType, WrapperType.Array, typeSymbol.NullableAnnotation == NullableAnnotation.Annotated);
                return true;
            }
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType && (namedTypeSymbol.Name == "Span" || namedTypeSymbol.Name == "ReadOnlySpan") && namedTypeSymbol.TypeArguments.Length == 1 && namedTypeSymbol.TypeArguments[0].IsValueType && namedTypeSymbol.TypeArguments[0].IsUnmanagedType)
            {
                outMarshallingData = new(namedTypeSymbol.TypeArguments[0], namedTypeSymbol.Name switch { "Span" => WrapperType.Span, "ReadOnlySpan" => WrapperType.ReadOnlySpan, _ => 0 }, typeSymbol.NullableAnnotation == NullableAnnotation.Annotated);
                return true;
            }
            outMarshallingData = default;
            return false;
        }

        public override bool CanMarshalToJS(IParameterSymbol paramSymbol)
        {
            var attrs = paramSymbol.GetAttributes();
            if (!attrs.Any(x => x.AttributeClass?.ToDisplayString() == "ScreepsDotNet.Interop.JSMarshalAsDataViewAttribute")) { return false; }
            return CanMarshalToJS(paramSymbol.Type);
        }

        public override bool CanMarshalToJS(ITypeSymbol paramTypeSymbol) => CanMarshalAsDataView(paramTypeSymbol, out _);

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (!CanMarshalAsDataView(paramTypeSymbol, out var marshallingData)) { return; }
            emitter.WriteLine($"fixed (void* {clrParamName}Ptr = {clrParamName})");
            emitter.OpenScope();
            emitter.WriteLine($"{jsParamName} = new({clrParamName}Ptr, {clrParamName}.Length * System.Runtime.InteropServices.Marshal.SizeOf<{marshallingData.ElementType.ToDisplayString()}>());");
        }

        public override void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            emitter.CloseScope();
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol) => false;

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol) => new(InteropValueType.Ptr, InteropValueFlags.None);

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
