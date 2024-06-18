using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using ScreepsDotNet.Bundler.Wasm;
using ScreepsDotNet.Bundler.Wasm.Sections;

namespace ScreepsDotNet.Bundler
{
    public class LowerBulkMemoryTask : Task
    {
        [Required]
        public string InWasmFileName { get; set; } = null!;

        [Required]
        public string OutWasmFileName { get; set; } = null!;

        public override bool Execute()
        {
            //byte[] inWasm = File.ReadAllBytes(InWasmFileName);
            //WasmBinary wasmBinary;
            //using (var inStrm = new MemoryStream(inWasm))
            //{
            //     wasmBinary = new WasmBinary(inStrm);
            //}
            WasmBinary inWasmBinary = new WasmBinary(InWasmFileName);
            WasmBinary spliceWasmBinary;
            using (var spliceWasmStream = new MemoryStream(BundleStaticAssets.BulkMemorySplice))
            {
                spliceWasmBinary = new WasmBinary(spliceWasmStream);
            }
            SpliceWasmFunctions(inWasmBinary, spliceWasmBinary);
            inWasmBinary.Write(OutWasmFileName);
            //byte[] outWasm;
            //using (var outStrm = new MemoryStream())
            //{
            //    wasmBinary.Write(outStrm);
            //    outWasm = outStrm.ToArray();
            //}
            //for (int i = 0; i < Math.Min(inWasm.Length, outWasm.Length); ++i)
            //{
            //    if (inWasm[i] != outWasm[i])
            //    {
            //        throw new Exception($"Byte mismatch at {i} ({inWasm[i]} vs {outWasm[i]})");
            //    }
            //}
            //if (inWasm.Length != outWasm.Length)
            //{
            //    throw new Exception($"Length mismatch ({inWasm.Length} vs {outWasm.Length})");
            //}
            //File.WriteAllBytes(OutWasmFileName, outWasm);


            return !Log.HasLoggedErrors;
        }

        private void SpliceWasmFunctions(WasmBinary inWasmBinary, WasmBinary spliceWasmBinary)
        {
            // Fetch input wasm sections
            var typeSection = inWasmBinary.TypeSection;
            if (typeSection == null)
            {
                Log.LogError("Unable to find wasm type section");
                return;
            }
            var functionSection = inWasmBinary.FunctionSection;
            if (functionSection == null)
            {
                Log.LogError("Unable to find wasm function section");
                return;
            }
            var importSection = inWasmBinary.ImportSection;
            if (importSection == null)
            {
                Log.LogError("Unable to find wasm import section");
                return;
            }
            var codeSection = inWasmBinary.CodeSection;
            if (codeSection == null)
            {
                Log.LogError("Unable to find wasm code section");
                return;
            }
            var nameSection = inWasmBinary.Sections.OfType<NameSection>().SingleOrDefault();
            if (nameSection == null)
            {
                Log.LogError("Unable to find wasm name section");
                return;
            }
            var functionNameSubsection = nameSection.Subsections.OfType<FunctionNameSubsection>().SingleOrDefault();
            if (functionNameSubsection == null)
            {
                Log.LogError("Unable to find wasm function name subsection");
                return;
            }
            uint firstInternalFunctionIndex = (uint)importSection.Imports.OfType<FunctionImport>().Count();

            // Fetch splice wasm subsections
            var spliceTypeSection = spliceWasmBinary.TypeSection;
            if (spliceTypeSection == null)
            {
                Log.LogError("Unable to find splice wasm type section");
                return;
            }
            var spliceFunctionSection = spliceWasmBinary.FunctionSection;
            if (spliceFunctionSection == null)
            {
                Log.LogError("Unable to find splice wasm function section");
                return;
            }
            var spliceExportSection = spliceWasmBinary.ExportSection;
            if (spliceExportSection == null)
            {
                Log.LogError("Unable to find splice wasm export section");
                return;
            }
            var spliceCodeSection = spliceWasmBinary.CodeSection;
            if (spliceCodeSection == null)
            {
                Log.LogError("Unable to find splice wasm code section");
                return;
            }

            // Enumerate exported functions from splice wasm
            foreach (var export in spliceExportSection.Exports.Where(x => x.Tag == ExportTag.Func))
            {
                // Lookup the function in the input wasm
                var functionIndexMaybe = functionNameSubsection.NameMap
                    .Select(x => new KeyValuePair<uint, string>?(x))
                    .Single(x => x!.Value.Value == export.Name)
                    ?.Key;
                if (functionIndexMaybe == null)
                {
                    Log.LogWarning($"Unable to find function '{export.Name}' in input wasm for splice");
                    continue;
                }
                uint functionIndex = functionIndexMaybe.Value;
                var functionType = functionSection.Types[(int)(functionIndex - firstInternalFunctionIndex)];
                var functionCode = codeSection.Codes[(int)(functionIndex - firstInternalFunctionIndex)];

                // Lookup the function in the splice wasm and verify signature compatibility
                uint spliceFunctionIndex = export.Index;
                var spliceFunctionType = spliceFunctionSection.Types[(int)spliceFunctionIndex];
                if (spliceFunctionType == null || spliceFunctionType != functionType)
                {
                    Log.LogError($"Function signature mismatch between input and splice wasm for '{export.Name}'");
                    continue;
                }
                var spliceFunctionCode = spliceCodeSection.Codes[(int)spliceFunctionIndex];

                // Copy splice function code to input function code
                functionCode.Locals.Clear();
                functionCode.Locals.AddRange(spliceFunctionCode.Locals);
                functionCode.ExpressionByteCode = spliceFunctionCode.ExpressionByteCode;
            }
        }


    }
}
