using System.Linq;

using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class StringArrayMarshaller : BaseMarshaller
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
            public readonly WrapperType WrapperType;
            public readonly bool WrapperTypeNullable;
            public readonly bool ElementTypeNullable;

            public MarshallingData(WrapperType wrapperType, bool wrapperTypeNullable, bool elementTypeNullable)
            {
                WrapperType = wrapperType;
                WrapperTypeNullable = wrapperTypeNullable;
                ElementTypeNullable = elementTypeNullable;
            }
        }

        public override bool Unsafe => true;

        private bool CanMarshalAsStringArray(ITypeSymbol typeSymbol, out MarshallingData outMarshallingData)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol && arrayTypeSymbol.ElementType.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString() == "string")
            {
                outMarshallingData = new(WrapperType.Array, typeSymbol.NullableAnnotation == NullableAnnotation.Annotated, arrayTypeSymbol.ElementType.NullableAnnotation == NullableAnnotation.Annotated);
                return true;
            }
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType && (namedTypeSymbol.Name == "Span" || namedTypeSymbol.Name == "ReadOnlySpan" || namedTypeSymbol.Name == "ImmutableArray") && namedTypeSymbol.TypeArguments.Length == 1 && namedTypeSymbol.TypeArguments[0].WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString() == "string")
            {
                outMarshallingData = new(namedTypeSymbol.Name switch { "Span" => WrapperType.Span, "ReadOnlySpan" => WrapperType.ReadOnlySpan, "ImmutableArray" => WrapperType.ImmutableArray, _ => 0 }, false, namedTypeSymbol.TypeArguments[0].NullableAnnotation == NullableAnnotation.Annotated);
                return true;
            }
            outMarshallingData = default;
            return false;
        }

        public override bool CanMarshalToJS(ITypeSymbol paramTypeSymbol) => CanMarshalAsStringArray(paramTypeSymbol, out _);

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (!CanMarshalAsStringArray(paramTypeSymbol, out var marshallingData)) { return; }

            var packedStringArrayBufferLengthName = $"{clrParamName}PackedStringArrayBufferLength";
            var packedStringArrayBufferName = $"{clrParamName}PackedStringArrayBuffer";
            var headName = $"{clrParamName}Head";

            emitter.WriteLine($"int {packedStringArrayBufferLengthName} = 0;");
            if (marshallingData.WrapperTypeNullable)
            {
                emitter.WriteLine($"if ({clrParamName} != null)");
                emitter.OpenScope();
            }
            emitter.WriteLine($"foreach (var str in {clrParamName})");
            emitter.OpenScope();
            emitter.WriteLine($"{packedStringArrayBufferLengthName} += (str?.Length ?? 0) + {(marshallingData.ElementTypeNullable ? 2 : 1)};");
            emitter.CloseScope();
            if (marshallingData.WrapperTypeNullable)
            {
                emitter.CloseScope();
                emitter.WriteLine($"else");
                emitter.OpenScope();
                emitter.WriteLine($"{packedStringArrayBufferLengthName} = {(marshallingData.ElementTypeNullable ? 2 : 1)};");
                emitter.CloseScope();
            }

            // TODO: Switch to managed allocation if array length is too big to avoid stack overflow
            emitter.WriteLine($"Span<char> {packedStringArrayBufferName} = stackalloc char[{packedStringArrayBufferLengthName}];");
            if (marshallingData.WrapperTypeNullable)
            {
                emitter.WriteLine($"if ({clrParamName} != null)");
                emitter.OpenScope();
            }
            emitter.WriteLine($"int {headName} = 0;");
            emitter.WriteLine($"foreach (var {clrParamName}Str in {clrParamName})");
            emitter.OpenScope();
            if (marshallingData.ElementTypeNullable)
            {
                emitter.WriteLine($"if ({clrParamName}Str == null)");
                emitter.OpenScope();
                emitter.WriteLine($"{packedStringArrayBufferName}[{headName}] = (char)0;");
                emitter.WriteLine($"++{headName};");
                emitter.CloseScope();
                emitter.WriteLine($"else");
                emitter.OpenScope();
                emitter.WriteLine($"{packedStringArrayBufferName}[{headName}] = (char)1;");
                emitter.WriteLine($"++{headName};");
            }
            emitter.WriteLine($"{clrParamName}Str.AsSpan().CopyTo({packedStringArrayBufferName}[{headName}..]);");
            emitter.WriteLine($"{headName} += {clrParamName}Str.Length;");
            if (marshallingData.ElementTypeNullable)
            {
                emitter.CloseScope();
            }
            emitter.WriteLine($"{packedStringArrayBufferName}[{headName}] = '\\0';");
            emitter.WriteLine($"++{headName};");
            emitter.CloseScope();
            if (marshallingData.WrapperTypeNullable)
            {
                emitter.CloseScope();
                emitter.WriteLine($"else");
                emitter.OpenScope();
                emitter.WriteLine($"{packedStringArrayBufferName}[0] = '\\0';");
                emitter.CloseScope();
            }
            emitter.WriteLine($"fixed (char* {packedStringArrayBufferName}Ptr = {packedStringArrayBufferName})");
            emitter.OpenScope();
            if (marshallingData.WrapperTypeNullable)
            {
                emitter.WriteLine($"{jsParamName} = {clrParamName} == null ? InteropValue.Void : new({packedStringArrayBufferName}Ptr, {clrParamName}.Length);");
            }
            else
            {
                emitter.WriteLine($"{jsParamName} = new({packedStringArrayBufferName}Ptr, {clrParamName}.Length);");
            }
        }

        public override void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            emitter.CloseScope();
        }

        public override bool CanMarshalFromJS(ITypeSymbol returnTypeSymbol) => CanMarshalAsStringArray(returnTypeSymbol, out _);

        public override void MarshalFromJS(ITypeSymbol returnTypeSymbol, string jsParamName, SourceEmitter emitter)
        {
            if (!CanMarshalAsStringArray(returnTypeSymbol, out var marshallingData)) { return; }

            if (marshallingData.ElementTypeNullable)
            {
                emitter.WriteLine($"var retArr = {jsParamName}.AsNullableStringArray();");
            }
            else
            {
                emitter.WriteLine($"var retArr = {jsParamName}.AsStringArray();");
            }
            if (!marshallingData.WrapperTypeNullable)
            {
                emitter.WriteLine($"if (retArr == null) {{ throw new NullReferenceException($\"Expecting array, got null\"); }}");
            }
            if (marshallingData.WrapperType == WrapperType.ImmutableArray)
            {
                emitter.WriteLine($"return retArr.ToImmutableArray();");
            }
            else
            {
                emitter.WriteLine($"return retArr;");
            }
        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol)
        {
            if (!CanMarshalAsStringArray(paramTypeSymbol, out var marshallingData)) { return new ParamSpec(InteropValueType.Arr, InteropValueFlags.None); }
            return new(InteropValueType.Arr, marshallingData.WrapperTypeNullable ? InteropValueFlags.Nullable : InteropValueFlags.None, InteropValueType.Str, marshallingData.ElementTypeNullable ? InteropValueFlags.Nullable : InteropValueFlags.None);
        }

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
