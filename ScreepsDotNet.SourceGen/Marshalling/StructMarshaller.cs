using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class StructMarshaller : BaseMarshaller
    {
        private readonly struct MarshallingData
        {
            public readonly INamedTypeSymbol WrapperType;
            public readonly INamedTypeSymbol StructType;
            public readonly bool IsNullable;

            public MarshallingData(INamedTypeSymbol wrapperType, INamedTypeSymbol structType, bool isNullable)
            {
                WrapperType = wrapperType;
                StructType = structType;
                IsNullable = isNullable;
            }
        }

        private const string JSStructAttribute = "ScreepsDotNet.Interop.JSStructAttribute";

        private readonly ImmutableArray<BaseMarshaller> fieldMarshallers;

        public override bool Unsafe => true;

        public StructMarshaller(ImmutableArray<BaseMarshaller> fieldMarshallers)
        {
            this.fieldMarshallers = fieldMarshallers;
        }

        public override MarshalMode CanMarshalToJS(ITypeSymbol paramTypeSymbol)
            => IsMarshallableStruct(paramTypeSymbol, out _) ? MarshalMode.Scoped : MarshalMode.Unsupported;

        private bool IsMarshallableStruct(ITypeSymbol typeSymbol, out MarshallingData marshallingData)
        {
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            {
                marshallingData = default;
                return false;
            }
            if (namedTypeSymbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == JSStructAttribute))
            {
                marshallingData = new(namedTypeSymbol, namedTypeSymbol, isNullable: false);
                return true;
            }
            if (namedTypeSymbol.IsGenericType && namedTypeSymbol.ConstructedFrom.Name == "Nullable" && namedTypeSymbol.TypeArguments.Length == 1 && namedTypeSymbol.TypeArguments[0] is INamedTypeSymbol innerType)
            {
                if (innerType.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == JSStructAttribute))
                {
                    marshallingData = new(namedTypeSymbol, innerType, isNullable: true);
                    return true;
                }
            }
            marshallingData = default;
            return false;
        }

        public override void BeginMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            var structTypeSymbol = (paramTypeSymbol as INamedTypeSymbol)!;
            var structFullName = structTypeSymbol.ToDisplayString(NullableFlowState.NotNull);

            var fields = JSStructGenerator.GetStructMarshallableFieldsToJS(structTypeSymbol, fieldMarshallers, allowScoped: true);

            emitter.WriteLine($"Span<InteropValue> {clrParamName}StructFields = stackalloc InteropValue[{fields.Length}];");

            int i = 0;
            foreach (var (field, _, marshaller) in fields)
            {
                marshaller.BeginMarshalToJS(field.Type, $"{clrParamName}.{field.Name}", $"{clrParamName}StructField[i]", emitter);
                ++i;
            }

            emitter.WriteLine($"fixed (InteropValue* {clrParamName}StructFieldsPtr = {clrParamName}StructFields)");
            emitter.OpenScope();
            emitter.WriteLine($"{jsParamName} = new({clrParamName}StructFieldsPtr, {structFullName}.__struct_index, {clrParamName}StructFields.Length);");
        }

        public override void EndMarshalToJS(ITypeSymbol paramTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            emitter.CloseScope();
        }

        public override MarshalMode CanMarshalFromJS(ITypeSymbol returnTypeSymbol)
            => IsMarshallableStruct(returnTypeSymbol, out _) ? MarshalMode.Scoped : MarshalMode.Unsupported;

        public override void BeginMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (!IsMarshallableStruct(returnTypeSymbol, out var marshallingData)) { return; }

            var structFullName = marshallingData.StructType.ToDisplayString(NullableFlowState.NotNull);

            var fields = JSStructGenerator.GetStructMarshallableFieldsFromJS(marshallingData.StructType, fieldMarshallers, allowScoped: true);

            emitter.WriteLine($"Span<InteropValue> {clrParamName}StructFields = stackalloc InteropValue[{fields.Length}];");

            int i = 0;
            foreach (var (field, _, marshaller) in fields)
            {
                marshaller.BeginMarshalFromJS(field.Type, $"{clrParamName}.{field.Name}", $"{clrParamName}StructField[i]", emitter);
                ++i;
            }

            emitter.WriteLine($"fixed (InteropValue* {clrParamName}StructFieldsPtr = {clrParamName}StructFields)");
            emitter.OpenScope();
            emitter.WriteLine($"{jsParamName} = new({clrParamName}StructFieldsPtr, {structFullName}.__struct_index, {clrParamName}StructFields.Length);");
        }

        public override void EndMarshalFromJS(ITypeSymbol returnTypeSymbol, string clrParamName, string jsParamName, SourceEmitter emitter)
        {
            if (!IsMarshallableStruct(returnTypeSymbol, out var marshallingData)) { return; }

            if (marshallingData.IsNullable)
            {
                emitter.WriteLine($"if ({jsParamName}.IsVoid())");
                emitter.OpenScope();
                emitter.WriteLine($"{clrParamName} = null;");
                emitter.CloseScope();
                emitter.WriteLine($"else");
                emitter.OpenScope();
            }

            emitter.WriteLine($"System.Diagnostics.Debug.Assert({jsParamName}.IsStruct());");

            var structFullName = marshallingData.StructType.ToDisplayString(NullableFlowState.NotNull);

            var fields = JSStructGenerator.GetStructMarshallableFieldsFromJS(marshallingData.StructType, fieldMarshallers, allowScoped: true);

            if (marshallingData.IsNullable)
            {
                emitter.WriteLine($"var {clrParamName}Data = new {structFullName}();");

                int i = fields.Length - 1;
                foreach (var (field, _, marshaller) in fields.Reverse())
                {
                    marshaller.EndMarshalFromJS(field.Type, $"{clrParamName}Data.{field.Name}", $"{clrParamName}StructFields[{i}]", emitter);
                    --i;
                }

                emitter.WriteLine($"{clrParamName} = new {structFullName}?({clrParamName}Data);");

                emitter.CloseScope();
            }
            else
            {
                emitter.WriteLine($"{clrParamName} = new {structFullName}();");

                int i = fields.Length - 1;
                foreach (var (field, _, marshaller) in fields.Reverse())
                {
                    marshaller.EndMarshalFromJS(field.Type, $"{clrParamName}.{field.Name}", $"{clrParamName}StructFields[{i}]", emitter);
                    --i;
                }
            }

            emitter.CloseScope();

        }

        public override ParamSpec GenerateParamSpec(ITypeSymbol paramTypeSymbol)
        {
            if (!IsMarshallableStruct(paramTypeSymbol, out var marshallingData)) { return new(InteropValueType.Void, InteropValueFlags.None); }
            return new(InteropValueType.Struct, marshallingData.IsNullable ? InteropValueFlags.Nullable : InteropValueFlags.None);
        }

        public override ParamSpec GenerateReturnParamSpec(ITypeSymbol returnTypeSymbol) => GenerateParamSpec(returnTypeSymbol);
    }
}
