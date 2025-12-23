using System;
using System.Collections.Immutable;
using System.Text;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using ScreepsDotNet.SourceGen.Marshalling;
using ScreepsDotNet.Interop;

namespace ScreepsDotNet.SourceGen
{
    [Generator(LanguageNames.CSharp)]
    public sealed class JSStructGenerator : IIncrementalGenerator
    {
        private const string JSStructAttribute = "ScreepsDotNet.Interop.JSStructAttribute";
        private const string JSStructFieldAttribute = "ScreepsDotNet.Interop.JSStructFieldAttribute";

        private static readonly ImmutableArray<BaseMarshaller> allMarshallers;

        static JSStructGenerator()
        {
            var primitiveMarshallers = new BaseMarshaller[] { new VoidMarshaller(), new NumericMarshaller(), new StringMarshaller(), new JSObjectMarshaller(), new NameMarshaller() };
            var unlayerableMarshallers = new BaseMarshaller[] { new DataViewMarshaller(), new StringArrayMarshaller() };
            var layeredMarshallers = new BaseMarshaller[] { new ArrayMarshaller(primitiveMarshallers.ToImmutableArray()) };

            allMarshallers = Enumerable.Empty<BaseMarshaller>()
                .Concat(primitiveMarshallers)
                .Concat(unlayerableMarshallers)
                .Concat(layeredMarshallers)
                .ToImmutableArray();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var attributedStructs = context.SyntaxProvider
                .ForAttributeWithMetadataName(JSStructAttribute,
                   static (node, ct) => node is StructDeclarationSyntax,
                   static (context, ct) => (syntax: (StructDeclarationSyntax)context.TargetNode, symbol: (INamedTypeSymbol)context.TargetSymbol));

            var compilationAndStructs
                = context.CompilationProvider.Combine(attributedStructs.Collect());

            context.RegisterSourceOutput(compilationAndStructs,
                static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private static void Execute(Compilation compilation, ImmutableArray<(StructDeclarationSyntax, INamedTypeSymbol)> structs, SourceProductionContext context)
        {
            var sourceEmitter = new SourceEmitter();
            sourceEmitter.WriteLine("using System;");
            sourceEmitter.WriteLine("using ScreepsDotNet.Interop;");

            foreach (var (structDeclarationSyntax, typeSymbol) in structs)
            {
                // Collect annotated fields
                var jsStructFields = GetStructMarshallableFieldsToJS(typeSymbol, allMarshallers, allowScoped: true);

                // Namespace
                sourceEmitter.EnterSymbolNamespace(typeSymbol, () =>
                {
                    // Struct
                    sourceEmitter.EnterScope($"{typeSymbol.DeclaredAccessibility.ToCS()} partial struct {typeSymbol.Name}", () =>
                    {
                        // Struct index cache
                        const string structIndexFieldName = "__struct_index";
                        sourceEmitter.WriteLine($"internal static readonly int {structIndexFieldName};");

                        // Static ctor
                        sourceEmitter.WriteLine($"[global::System.Runtime.Versioning.SupportedOSPlatform(\"wasi\")]");
                        sourceEmitter.WriteLine($"[global::System.Diagnostics.DebuggerNonUserCode]");
                        sourceEmitter.EnterScope($"static {typeSymbol.Name}()", () =>
                        {
                            var stringBuffer = string.Join("", jsStructFields.Select(x => $"{x.attr!.ConstructorArguments[0].Value}\0"));
                            sourceEmitter.WriteLine($"string fieldNames = \"{stringBuffer}\";");
                            sourceEmitter.EnterScope($"unsafe", () =>
                            {
                                sourceEmitter.EnterScope($"fixed (char* fieldNamesPtr = fieldNames)", () =>
                                {
                                    sourceEmitter.WriteLine($"Span<StructFieldSpec> fields = stackalloc StructFieldSpec[{jsStructFields.Length}];");
                                    int i = 0;
                                    int stringOffset = 0;
                                    foreach (var (field, attr, marshaller) in jsStructFields)
                                    {
                                        sourceEmitter.WriteLine($"fields[{i}].NamePtr = (IntPtr)(fieldNamesPtr + {stringOffset});");
                                        sourceEmitter.WriteLine($"fields[{i}].ParamSpec = {JSImportGenerator.ParamSpecToCs(marshaller!.GenerateParamSpec(field.Type))};");
                                        stringOffset += attr!.ConstructorArguments[0].Value!.ToString().Length + 1;
                                        ++i;
                                    }
                                    sourceEmitter.WriteLine($"{structIndexFieldName} = ScreepsDotNet.Interop.Native.DefineStruct(fields);");
                                });
                            });
                        });
                    });
                });
            }

            context.AddSource($"ScreepsDotNet.JSStructs", SourceText.From(sourceEmitter.ToString(), Encoding.UTF8));
        }

        internal static ImmutableArray<(IFieldSymbol field, AttributeData attr, BaseMarshaller marshaller)> GetStructMarshallableFieldsToJS(INamedTypeSymbol structSymbol, ImmutableArray<BaseMarshaller> availableMarshallers, bool allowScoped)
        {
            // Suppress CS8619 as null types are filtered in where clauses so non-nullability is guaranteed
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return structSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Select(x => (field: x, attr: x.GetAttributes().SingleOrDefault(y => y.AttributeClass != null && y.AttributeClass.ToDisplayString() == JSStructFieldAttribute)))
                .Where(x => x.attr != null)
                .Select(x =>
                {
                    foreach (var marshaller in availableMarshallers)
                    {
                        var mode = marshaller.CanMarshalToJS(x.field.Type);
                        if (mode == MarshalMode.Unsupported) { continue; }
                        if (mode == MarshalMode.Scoped && !allowScoped) { continue; }
                        return (x.field, x.attr, marshaller: (BaseMarshaller?)marshaller);
                    }
                    return (x.field, x.attr, null);
                })
                .Where(x => x.marshaller != null)
                .OrderBy(x => x.field.Name)
                .ToImmutableArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        internal static ImmutableArray<(IFieldSymbol field, AttributeData attr, BaseMarshaller marshaller)> GetStructMarshallableFieldsFromJS(INamedTypeSymbol structSymbol, ImmutableArray<BaseMarshaller> availableMarshallers, bool allowScoped)
        {
            // Suppress CS8619 as null types are filtered in where clauses so non-nullability is guaranteed
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return structSymbol.GetMembers()
                .OfType<IFieldSymbol>()
                .Select(x => (field: x, attr: x.GetAttributes().SingleOrDefault(y => y.AttributeClass != null && y.AttributeClass.ToDisplayString() == JSStructFieldAttribute)))
                .Where(x => x.attr != null)
                .Select(x =>
                {
                    foreach (var marshaller in availableMarshallers)
                    {
                        var mode = marshaller.CanMarshalFromJS(x.field.Type);
                        if (mode == MarshalMode.Unsupported) { continue; }
                        if (mode == MarshalMode.Scoped && !allowScoped) { continue; }
                        return (x.field, x.attr, marshaller: (BaseMarshaller?)marshaller);
                    }
                    return (x.field, x.attr, null);
                })
                .Where(x => x.marshaller != null)
                .OrderBy(x => x.field.Name)
                .ToImmutableArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

    }
}
