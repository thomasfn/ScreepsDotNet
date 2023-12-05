using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class ArrayMarshaller : BaseMarshaller
    {
        private readonly ImmutableArray<BaseMarshaller> elementMarshallers;

        public override bool Unsafe => true;

        public ArrayMarshaller(ImmutableArray<BaseMarshaller> elementMarshallers)
        {
            this.elementMarshallers = elementMarshallers;
        }

        public override bool CanMarshalToJS(ITypeSymbol paramTypeSymbol)
        {
            if (paramTypeSymbol is not IArrayTypeSymbol arrayTypeSymbol) { return false; }
            return elementMarshallers.Any(x => !x.Unsafe && x.CanMarshalToJS(arrayTypeSymbol.ElementType));
        }

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (paramTypeSymbol is not IArrayTypeSymbol arrayTypeSymbol) { return; }
            var elementMarshaller = elementMarshallers.First(x => !x.Unsafe && x.CanMarshalToJS(arrayTypeSymbol.ElementType));
            if (paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
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
            elementMarshaller.BeginMarshalToJS(arrayTypeSymbol.ElementType, $"{clrParamName}[i]", $"{clrParamName}Buffer[i]", emitter);
            elementMarshaller.EndMarshalToJS(arrayTypeSymbol.ElementType, $"{clrParamName}[i]", $"{clrParamName}Buffer[i]", emitter);
            emitter.CloseScope();
            if (paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                emitter.CloseScope();
            }
            emitter.WriteLine($"fixed (InteropValue* {clrParamName}BufferPtr = {clrParamName}Buffer)");
            emitter.OpenScope();
            if (paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
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

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
        {
            if (returnTypeSymbol is not IArrayTypeSymbol arrayTypeSymbol) { return false; }
            return elementMarshallers.Any(x => !x.Unsafe && x.CanMarshalToJS(arrayTypeSymbol.ElementType));
        }

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string jsParamName, SourceEmitter emitter)
        {
            if (returnTypeSymbol is not IArrayTypeSymbol arrayTypeSymbol) { return; }
            var elementMarshaller = elementMarshallers.First(x => !x.Unsafe && x.CanMarshalToJS(arrayTypeSymbol.ElementType));
            emitter.WriteLine($"Func<InteropValue, {arrayTypeSymbol.ElementType.ToDisplayString()}> elementMarshaller = (interopValue) =>");
            emitter.OpenScope();
            elementMarshaller.MarshalFromJS(arrayTypeSymbol.ElementType, "interopValue", emitter);
            emitter.DecrementIndent();
            emitter.WriteLine($"}};");
            if (returnTypeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                emitter.WriteLine($"var retArr = {jsParamName}.AsArray(elementMarshaller);");
                emitter.WriteLine($"if (retArr == null) {{ throw new NullReferenceException($\"Expecting array, got null\"); }}");
                emitter.WriteLine($"return retArr;");
                return;
            }
            emitter.WriteLine($"return {jsParamName}.AsArray(elementMarshaller);");
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol)
        {
            var flags = paramTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated ? InteropValueFlags.Nullable : InteropValueFlags.None;
            if (paramTypeSymbol is not IArrayTypeSymbol arrayTypeSymbol) { return new(InteropValueType.Arr, flags); }
            var elementMarshaller = elementMarshallers.First(x => !x.Unsafe && x.CanMarshalToJS(arrayTypeSymbol.ElementType));
            var elementParamSpec = elementMarshaller.GenerateParamSpec(arrayTypeSymbol.ElementType);
            return new(InteropValueType.Arr, flags, elementParamSpec.Type, elementParamSpec.Flags);
        }

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
