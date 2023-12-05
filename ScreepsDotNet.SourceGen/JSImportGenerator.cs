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
    public sealed class JSImportGenerator : IIncrementalGenerator
    {
        private const string JSImportAttribute = "ScreepsDotNet.Interop.JSImportAttribute";

        private static readonly ImmutableArray<BaseMarshaller> allMarshallers;

        static JSImportGenerator()
        {
            var primitiveMarshallers = new BaseMarshaller[] { new VoidMarshaller(), new NumericMarshaller(), new StringMarshaller(), new JSObjectMarshaller() };
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
            var attributedMethods = context.SyntaxProvider
                .ForAttributeWithMetadataName(JSImportAttribute,
                   static (node, ct) => node is MethodDeclarationSyntax,
                   static (context, ct) => (syntax: (MethodDeclarationSyntax)context.TargetNode, symbol: (IMethodSymbol)context.TargetSymbol));

            var compilationAndMethods
                = context.CompilationProvider.Combine(attributedMethods.Collect());

            context.RegisterSourceOutput(compilationAndMethods,
                static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private static void Execute(Compilation compilation, ImmutableArray<(MethodDeclarationSyntax, IMethodSymbol)> methods, SourceProductionContext context)
        {
            var sourceEmitter = new SourceEmitter();
            sourceEmitter.WriteLine("using System;");
            sourceEmitter.WriteLine("using System.Collections.Immutable;");
            sourceEmitter.WriteLine("using ScreepsDotNet.Interop;");

            int uniqueImportIndex = 0;
            foreach (var (methodDeclarationSyntax, methodSymbol) in methods)
            {
                // Namespace
                sourceEmitter.EnterSymbolNamespace(methodSymbol.ContainingType, () =>
                {
                    // Class
                    sourceEmitter.EnterScope($"{methodSymbol.ContainingType.DeclaredAccessibility.ToCS()} {(methodSymbol.ContainingType.IsStatic ? "static " : "")}partial class {methodSymbol.ContainingType.Name}", () =>
                    {
                        // Import index cache
                        string importIndexFieldName = $"__{methodSymbol.Name}_{uniqueImportIndex}_import_index";
                        sourceEmitter.WriteLine($"private static int {importIndexFieldName} = -1;");
                        ++uniqueImportIndex;

                        // Method
                        sourceEmitter.WriteLine($"[global::System.Runtime.Versioning.SupportedOSPlatform(\"wasi\")]");
                        sourceEmitter.WriteLine($"[global::System.Diagnostics.DebuggerNonUserCode]");
                        sourceEmitter.EnterScope($"{methodSymbol.DeclaredAccessibility.ToCS()} {(methodSymbol.IsStatic ? "static " : "")}partial {methodSymbol.ReturnType.ToDisplayString()} {methodSymbol.Name}({string.Join(", ", methodSymbol.Parameters.Select(x => x.ToDisplayString()))})", () =>
                        {
                            GenerateInteropForMethod(sourceEmitter, methodSymbol, importIndexFieldName);
                        });
                    });
                });
            }

            context.AddSource($"ScreepsDotNet.JSImports", SourceText.From(sourceEmitter.ToString(), Encoding.UTF8));
        }

        private static void GenerateInteropForMethod(SourceEmitter sourceEmitter, IMethodSymbol methodSymbol, string importIndexFieldName)
        {
            // Max param count is 8
            if (methodSymbol.Parameters.Length > 8)
            {
                sourceEmitter.WriteLine($"// Too many parameters (max of 8 supported)");
                sourceEmitter.WriteLine($"throw new NotImplementedException();");
                return;
            }

            // Fetch import attr
            var jsImportAttr = methodSymbol.GetAttributes().Single(x => x.AttributeClass != null && x.AttributeClass.ToDisplayString() == JSImportAttribute);
            string? moduleName = jsImportAttr.ConstructorArguments[0].Value as string;
            if (string.IsNullOrEmpty(moduleName))
            {
                sourceEmitter.WriteLine($"// Invalid module name");
                sourceEmitter.WriteLine($"throw new NotImplementedException();");
            }
            string? importName = jsImportAttr.ConstructorArguments[1].Value as string;
            if (string.IsNullOrEmpty(importName))
            {
                sourceEmitter.WriteLine($"// Invalid import name");
                sourceEmitter.WriteLine($"throw new NotImplementedException();");
            }

            // Fetch marshallers for parameters and return value, check if marshalling is possible
            bool isUnsafe = false;
            BaseMarshaller[] marshallers = new BaseMarshaller[methodSymbol.Parameters.Length];
            for (int i = 0; i < methodSymbol.Parameters.Length; ++i)
            {
                bool found = false;
                foreach (var marshaller in allMarshallers)
                {
                    if (marshaller.CanMarshalToJS(methodSymbol.Parameters[i]))
                    {
                        marshallers[i] = marshaller;
                        isUnsafe |= marshaller.Unsafe;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    sourceEmitter.WriteLine($"// Unable to marshal parameter '{methodSymbol.Parameters[i].Name}' ({methodSymbol.Parameters[i].Type.ToDisplayString()})");
                    sourceEmitter.WriteLine($"throw new NotImplementedException();");
                    return;
                }
            }
            BaseMarshaller? retMarshaller = null;
            {
                foreach (var marshaller in allMarshallers)
                {
                    if (marshaller.CanMarshalFromJS(methodSymbol.ReturnType))
                    {
                        retMarshaller = marshaller;
                        isUnsafe |= marshaller.Unsafe;
                        break;
                    }
                }
                if (retMarshaller == null)
                {
                    sourceEmitter.WriteLine($"// Unable to marshal return type ({methodSymbol.ReturnType.ToDisplayString()})");
                    sourceEmitter.WriteLine($"throw new NotImplementedException();");
                    return;
                }
            }

            // Emit import code
            sourceEmitter.EnterScope($"if ({importIndexFieldName} == -1)", () =>
            {
                sourceEmitter.WriteLine($"var functionSpec = new FunctionSpec();");
                for (int i = 0; i < methodSymbol.Parameters.Length; ++i)
                {
                    var marshaller = marshallers[i];
                    sourceEmitter.WriteLine($"functionSpec.ParamSpecs.Params[{i}] = {ParamSpecToCs(marshaller.GenerateParamSpec(methodSymbol.Parameters[i]))};");
                }
                sourceEmitter.WriteLine($"functionSpec.ReturnParamSpec = {ParamSpecToCs(retMarshaller.GenerateReturnParamSpec(methodSymbol.ReturnType))};");
                sourceEmitter.WriteLine($"{importIndexFieldName} = Interop.Native.BindImport(\"{moduleName}\", \"{importName}\", functionSpec);");
            });

            // Emit invoke code
            if (isUnsafe)
            {
                sourceEmitter.WriteLine($"unsafe");
                sourceEmitter.OpenScope();
            }
            if (methodSymbol.Parameters.Length > 0)
            {
                sourceEmitter.WriteLine($"Span<InteropValue> args = stackalloc InteropValue[{methodSymbol.Parameters.Length}];");
                for (int i = 0; i < methodSymbol.Parameters.Length; ++i)
                {
                    marshallers[i].BeginMarshalToJS(methodSymbol.Parameters[i].Type, methodSymbol.Parameters[i].Name, $"args[{i}]", sourceEmitter);
                }
                sourceEmitter.WriteLine($"var returnVal = Interop.Native.InvokeImport({importIndexFieldName}, args);");
                retMarshaller.MarshalFromJS(methodSymbol.ReturnType, "returnVal", sourceEmitter);
                for (int i = methodSymbol.Parameters.Length - 1; i >= 0; --i)
                {
                    marshallers[i].EndMarshalToJS(methodSymbol.Parameters[i].Type, methodSymbol.Parameters[i].Name, $"args[{i}]", sourceEmitter);
                }
            }
            else
            {
                sourceEmitter.WriteLine($"var returnVal = Interop.Native.InvokeImport({importIndexFieldName}, Span<InteropValue>.Empty);");
                retMarshaller.MarshalFromJS(methodSymbol.ReturnType, "returnVal", sourceEmitter);
            }
            if (isUnsafe)
            {
                sourceEmitter.CloseScope();
            }
        }

        private static string ParamSpecToCs(ParamSpec paramSpec)
        {
            var sb = new StringBuilder();
            sb.Append("new(");
            sb.Append("InteropValueType.");
            sb.Append(paramSpec.Type.ToString());
            sb.Append(", ");
            sb.Append("InteropValueFlags.");
            sb.Append(paramSpec.Flags.ToString());
            if (paramSpec.ElementType != InteropValueType.Void)
            {
                sb.Append(", ");
                sb.Append("InteropValueType.");
                sb.Append(paramSpec.ElementType.ToString());
                sb.Append(", ");
                sb.Append("InteropValueFlags.");
                sb.Append(paramSpec.ElementFlags.ToString());
            }
            sb.Append(")");
            return sb.ToString();
        }

    }
}
