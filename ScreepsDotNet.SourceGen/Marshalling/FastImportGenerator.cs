using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ScreepsDotNet.SourceGen.Marshalling
{
    internal class FastImportGenerator
    {
        private static readonly HashSet<string> primitiveIntegralNumerics = new() { "byte", "sbyte", "char", "ushort", "short", "uint", "int", "nint" };

        private static readonly HashSet<string> primitiveIntegralLongNumerics = new() { "long", "ulong" };

        protected static bool IsJSObject(ITypeSymbol typeSymbol) => typeSymbol.ToDisplayString() == "ScreepsDotNet.Interop.JSObject";

        protected static bool IsIntegralNumeric(ITypeSymbol typeSymbol)
        {
            var displayName = typeSymbol.ToDisplayString();
            if (primitiveIntegralNumerics.Contains(displayName)) { return true; }
            if (typeSymbol.BaseType != null && typeSymbol.BaseType.ToDisplayString() == "System.Enum") { return true; }
            return false;
        }

        protected static bool IsIntegralLongNumeric(ITypeSymbol typeSymbol) => primitiveIntegralLongNumerics.Contains(typeSymbol.ToDisplayString());

        private enum ParamType
        {
            Void, // v
            Int32, // i
            Int64, // l
            JSObject, // o
            Float, // f
            Double, // d
            Name, // n
        }

        private readonly Dictionary<char, ParamType> paramTypeParseTable = new()
        {
            { 'v', ParamType.Void },
            { 'i', ParamType.Int32 },
            { 'l', ParamType.Int64 },
            { 'o', ParamType.JSObject },
            { 'f', ParamType.Float },
            { 'd', ParamType.Double },
            { 'n', ParamType.Name },
        };

        private readonly string signature;
        private readonly ParamType returnParamType;
        private readonly ImmutableArray<ParamType> paramTypes;

        public FastImportGenerator(string signature)
        {
            this.signature = signature;
            var lr = signature.Split('_');
            returnParamType = paramTypeParseTable[lr[0][0]];
            paramTypes = lr[1].Select(x => paramTypeParseTable[x]).ToImmutableArray();
            if (paramTypes.Length == 1 && paramTypes[0] == ParamType.Void) { paramTypes = ImmutableArray<ParamType>.Empty; }
        }

        private static bool IsTypeMatch(ITypeSymbol typeSymbol, ParamType expectedParamType) => expectedParamType switch
        {
            ParamType.Void => typeSymbol.ToDisplayString() == "void",
            ParamType.Int32 => IsIntegralNumeric(typeSymbol),
            ParamType.Int64 => IsIntegralLongNumeric(typeSymbol),
            ParamType.JSObject => IsJSObject(typeSymbol),
            ParamType.Float => typeSymbol.ToDisplayString() == "float",
            ParamType.Double => typeSymbol.ToDisplayString() == "double",
            ParamType.Name => typeSymbol.ToDisplayString() == "ScreepsDotNet.Interop.Name",
            _ => false,
        };

        private static string MarshalToJS(IParameterSymbol paramSymbol, ParamType expectedParamType) => expectedParamType switch
        {
            ParamType.Void => "",
            ParamType.Int32 => $"(int){paramSymbol.Name}",
            ParamType.Int64 => $"(long){paramSymbol.Name}",
            ParamType.JSObject => $"(int){paramSymbol.Name}.JSHandle",
            ParamType.Float => $"(float){paramSymbol.Name}",
            ParamType.Double => $"(double){paramSymbol.Name}",
            ParamType.Name => $"{paramSymbol.Name}.NameIndex",
            _ => paramSymbol.Name,
        };

        private static string MarshalFromJS(string innerExpr, ParamType expectedParamType, ITypeSymbol returnTypeSymbol) => expectedParamType switch
        {
            ParamType.Void => "",
            ParamType.Int32 => $"({returnTypeSymbol.ToDisplayString()})({innerExpr})",
            ParamType.Int64 => $"({returnTypeSymbol.ToDisplayString()})({innerExpr})",
            ParamType.JSObject => $"Native.GetJSObject((IntPtr)({innerExpr}))",
            ParamType.Float => $"({returnTypeSymbol.ToDisplayString()})({innerExpr})",
            ParamType.Double => $"({returnTypeSymbol.ToDisplayString()})({innerExpr})",
            ParamType.Name => $"new({innerExpr})",
            _ => innerExpr,
        };

        public bool IsSignatureSupported(ITypeSymbol returnTypeSymbol, ImmutableArray<IParameterSymbol> paramSymbols)
            => IsSignatureSupported(returnTypeSymbol, paramSymbols.Select(static x => x.Type).ToImmutableArray());

        public bool IsSignatureSupported(ITypeSymbol returnTypeSymbol, ImmutableArray<ITypeSymbol> paramSymbols)
            => paramSymbols.Length == paramTypes.Length && IsTypeMatch(returnTypeSymbol, returnParamType) && paramSymbols.Select(static (x, i) => (x, i)).All(x => IsTypeMatch(x.x, paramTypes[x.i]));

        public void GenerateCallSite(string importIndexFieldName, ITypeSymbol returnTypeSymbol, ImmutableArray<IParameterSymbol> paramSymbols, SourceEmitter emitter)
        {
            foreach (var param in paramSymbols)
            {
                if (param.Type.ToDisplayString() == "ScreepsDotNet.Interop.Name")
                {
                    emitter.WriteLine($"ScreepsDotNet.Interop.Name.CopyIfNeeded({param.Name}.NameIndex);");
                }
            }
            var innerExpr = paramSymbols.Length > 0
                ? $"ScreepsDotNet_Interop.InvokeImport_{signature}({importIndexFieldName}, {string.Join(", ", paramSymbols.Select((x, i) => MarshalToJS(x, paramTypes[i])))})"
                : $"ScreepsDotNet_Interop.InvokeImport_{signature}({importIndexFieldName})";
            if (returnParamType == ParamType.Void)
            {
                emitter.WriteLine($"{innerExpr};");
            }
            else
            {
                emitter.WriteLine($"return {MarshalFromJS(innerExpr, returnParamType, returnTypeSymbol)};");
            }
        }
    }
}
