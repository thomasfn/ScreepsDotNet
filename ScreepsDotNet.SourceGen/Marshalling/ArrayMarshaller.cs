using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class ArrayMarshaller : BaseMarshaller
    {
        private enum WrapperType
        {
            Array,
            ImmutableArray,
            Span,
            ReadOnlySpan,
        }

        private readonly struct MarshallingData
        {
            public readonly ITypeSymbol ElementType;
            public readonly WrapperType WrapperType;
            public readonly bool WrapperTypeNullable;

            public MarshallingData(ITypeSymbol elementType, WrapperType wrapperType, bool wrapperTypeNullable)
            {
                ElementType = elementType;
                WrapperType = wrapperType;
                WrapperTypeNullable = wrapperTypeNullable;
            }
        }

        private readonly ImmutableArray<BaseMarshaller> elementMarshallers;

        public override bool Unsafe => true;

        public ArrayMarshaller(ImmutableArray<BaseMarshaller> elementMarshallers)
        {
            this.elementMarshallers = elementMarshallers;
        }

        private bool CanMarshalAsArray(ITypeSymbol typeSymbol, out MarshallingData outMarshallingData)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                outMarshallingData = new(arrayTypeSymbol.ElementType, WrapperType.Array, typeSymbol.NullableAnnotation == NullableAnnotation.Annotated);
                return true;
            }
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType && (namedTypeSymbol.Name == "Span" || namedTypeSymbol.Name == "ReadOnlySpan" || namedTypeSymbol.Name == "ImmutableArray") && namedTypeSymbol.TypeArguments.Length == 1)
            {
                outMarshallingData = new(namedTypeSymbol.TypeArguments[0], namedTypeSymbol.Name switch { "Span" => WrapperType.Span, "ReadOnlySpan" => WrapperType.ReadOnlySpan, "ImmutableArray" => WrapperType.ImmutableArray, _ => 0 }, typeSymbol.NullableAnnotation == NullableAnnotation.Annotated);
                return true;
            }
            outMarshallingData = default;
            return false;
        }

        public override MarshalMode CanMarshalToJS(ITypeSymbol paramTypeSymbol)
        {
            if (!CanMarshalAsArray(paramTypeSymbol, out var marshallingData)) { return MarshalMode.Unsupported; }
            return elementMarshallers.Any(x => x.CanMarshalToJS(marshallingData.ElementType) == MarshalMode.Trivial) ? MarshalMode.Scoped : MarshalMode.Unsupported;
        }

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (!CanMarshalAsArray(paramTypeSymbol, out var marshallingData)) { return; }
            var elementMarshaller = elementMarshallers.First(x => x.CanMarshalToJS(marshallingData.ElementType) == MarshalMode.Trivial);
            if (marshallingData.WrapperTypeNullable)
            {
                // TODO: Switch to managed allocation if array length is too big to avoid stack overflow
                emitter.WriteLine($"Span<InteropValue> {clrParamName}Buffer = {clrParamName} == null ? Span<InteropValue>.Empty : stackalloc InteropValue[{clrParamName}.Length];");
                emitter.WriteLine($"if ({clrParamName} != null)");
                emitter.OpenScope();
            }
            else
            {
                emitter.WriteLine($"Span<InteropValue> {clrParamName}Buffer = stackalloc InteropValue[{clrParamName}.Length];");
            }
            emitter.WriteLine($"for (int i = 0; i < {clrParamName}.Length; ++i)");
            emitter.OpenScope();
            elementMarshaller.BeginMarshalToJS(marshallingData.ElementType, $"{clrParamName}[i]", $"{clrParamName}Buffer[i]", emitter);
            elementMarshaller.EndMarshalToJS(marshallingData.ElementType, $"{clrParamName}[i]", $"{clrParamName}Buffer[i]", emitter);
            emitter.CloseScope();
            if (marshallingData.WrapperTypeNullable)
            {
                emitter.CloseScope();
            }
            emitter.WriteLine($"fixed (InteropValue* {clrParamName}BufferPtr = {clrParamName}Buffer)");
            emitter.OpenScope();
            if (marshallingData.WrapperTypeNullable)
            {
                emitter.WriteLine($"{jsParamName} = {clrParamName} == null ? InteropValue.Void : new({clrParamName}BufferPtr, {clrParamName}.Length);");
            }
            else
            {
                emitter.WriteLine($"{jsParamName} = new({clrParamName}BufferPtr, {clrParamName}.Length);");
            }
        }

        public override void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            emitter.CloseScope();
        }

        public override MarshalMode CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
        {
            if (!CanMarshalAsArray(returnTypeSymbol, out var marshallingData)) { return MarshalMode.Unsupported; }
            return elementMarshallers.Any(x => x.CanMarshalToJS(marshallingData.ElementType) == MarshalMode.Trivial) ? MarshalMode.Trivial : MarshalMode.Unsupported;
        }

        public override void BeginMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            
        }

        public override void EndMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (!CanMarshalAsArray(returnTypeSymbol, out var marshallingData)) { return; }
            var elementMarshaller = elementMarshallers.First(x => x.CanMarshalToJS(marshallingData.ElementType) == MarshalMode.Trivial);
            emitter.WriteLine($"Func<InteropValue, {marshallingData.ElementType.ToDisplayString()}> elementMarshaller = (interopValue) =>");
            emitter.OpenScope();
            elementMarshaller.EndMarshalFromJS(marshallingData.ElementType, "var elementRetVal", "interopValue", emitter);
            emitter.WriteLine($"return elementRetVal;");
            emitter.DecrementIndent();
            emitter.WriteLine($"}};");
            string retVarName;
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"var retArr = {jsParamName}.AsArray(elementMarshaller);");
                emitter.WriteLine($"if (retArr == null) {{ throw new NullReferenceException($\"Expecting array, got null\"); }}");
                retVarName = "retArr";
            }
            else
            {
                retVarName = $"{jsParamName}.AsArray(elementMarshaller)";
            }
            switch (marshallingData.WrapperType)
            {
                case WrapperType.Array:
                case WrapperType.Span:
                case WrapperType.ReadOnlySpan:
                    emitter.WriteLine($"{clrParamName} = {retVarName};");
                    break;
                case WrapperType.ImmutableArray:
                    emitter.WriteLine($"{clrParamName} = {retVarName}.ToImmutableArray();");
                    break;
            }
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol)
        {
            if (!CanMarshalAsArray(paramTypeSymbol, out var marshallingData)) { return new(InteropValueType.Void, InteropValueFlags.None); }
            var elementMarshaller = elementMarshallers.First(x => x.CanMarshalToJS(marshallingData.ElementType) == MarshalMode.Trivial);
            var elementParamSpec = elementMarshaller.GenerateParamSpec(marshallingData.ElementType);
            return new(InteropValueType.Array, marshallingData.WrapperTypeNullable ? InteropValueFlags.Nullable : InteropValueFlags.None, elementParamSpec.Type, elementParamSpec.Flags);
        }

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
