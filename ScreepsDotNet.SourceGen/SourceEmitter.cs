using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Text;

namespace ScreepsDotNet.SourceGen
{
    internal static class SourceEmitterExtensions
    {
        public static string ToCS(this Accessibility accessibility)
            => accessibility switch
            {
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "protected internal",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.Public => "public",
                _ => string.Empty,
            };
    }

    internal class SourceEmitter
    {
        private enum CursorPosition
        {
            StartOfLine,
            StartOfStatement,
            AfterGrammar,
            AfterGrammarSpaceNeeded,
            EndOfLine
        }

        private readonly StringBuilder sb = new();
        private int indentLevel = 0;
        private string indent = string.Empty;
        private CursorPosition cursorPos = CursorPosition.StartOfLine;

        private void MoveCursor(CursorPosition newCursorPos)
        {
            if (cursorPos == newCursorPos) { return; }
            if (cursorPos == CursorPosition.EndOfLine)
            {
                sb.AppendLine();
                cursorPos = CursorPosition.StartOfLine;
            }
            if (cursorPos == newCursorPos) { return; }
            if (cursorPos == CursorPosition.StartOfLine)
            {
                sb.Append(indent);
                cursorPos = CursorPosition.StartOfStatement;
            }
            if (cursorPos == newCursorPos) { return; }
            
        }

        public void WriteLine(string text)
        {
            sb.Append(indent);
            sb.AppendLine(text);
        }

        

        public void WriteTypeReference(ITypeSymbol typeSymbol)
        {
            sb.Append(typeSymbol.ToDisplayString());
            sb.Append(" ");
        }

        public void IncrementIndent()
        {
            ++indentLevel;
            indent = string.Empty.PadLeft(indentLevel * 4, ' ');
        }

        public void DecrementIndent()
        {
            indentLevel = Math.Max(0, indentLevel - 1);
            indent = string.Empty.PadLeft(indentLevel * 4, ' ');
        }

        public void OpenScope()
        {
            WriteLine("{");
            IncrementIndent();
        }

        public void CloseScope()
        {
            DecrementIndent();
            WriteLine("}");
        }

        public void EnterSymbolNamespace(ISymbol symbol, Action innerFunc)
        {
            List<string> namespaceChain = new();
            INamespaceSymbol? ns = symbol.ContainingNamespace;
            while (ns != null && !string.IsNullOrEmpty(ns.Name))
            {
                namespaceChain.Add(ns.Name);
                ns = ns.ContainingNamespace;
            }
            namespaceChain.Reverse();
            EnterScope($"namespace {string.Join(".", namespaceChain)}", innerFunc);
        }

        public void EnterScope(string header, Action innerFunc)
        {
            WriteLine(header);
            OpenScope();
            innerFunc();
            CloseScope();
        }

        public override string ToString() => sb.ToString();
    }
}
