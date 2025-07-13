using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Kzrnm.Convert.Base32768;

namespace ScreepsDotNet.Bundler
{
    public class BundlerBuildTask : Task
    {
        [Required]
        public string AppBundleDir { get; set; } = null!;

        [Required]
        public string WasmFileName { get; set; } = null!;

        [Required]
        public string Configuration { get; set; } = null!;

        [Required]
        public bool CompressWasm { get; set; } = false;

        [Required]
        public string Encoding { get; set; } = null!;

        public string? CustomInitExportNames { get; set; } = null;

        [Required]
        public string WorldJsFiles { get; set; } = null!;

        [Required]
        public string WorldStartup { get; set; } = null!;

        [Required]
        public string WorldLoop { get; set; } = null!;

        [Required]
        public string ArenaJsFiles { get; set; } = null!;

        [Required]
        public string ArenaStartup { get; set; } = null!;

        [Required]
        public string ArenaLoop { get; set; } = null!;

        [Output]
        public string[] BundleFilePaths { get; set; } = null!;

        public override bool Execute()
        {
            // Build main.(m)js replacements
            var mainJsReplacements = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(CustomInitExportNames))
            {
                mainJsReplacements.Add("CUSTOM_INIT_EXPORT_NAMES", "");
            }
            else
            {
                var parsedCustomInitExportNames = CustomInitExportNames.Split(',');
                mainJsReplacements.Add("CUSTOM_INIT_EXPORT_NAMES", string.Join(",", parsedCustomInitExportNames.Select(x => $"'{x}'")));
            }

            // Load wasm and compress if requested
            byte[] wasmData;
            int originalWasmSize;
            if (CompressWasm)
            {
                using var memoryStream = new MemoryStream();
                int originalSize;
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                {
                    using var fileStream = File.OpenRead(WasmFileName);
                    originalWasmSize = (int)fileStream.Length;
                    fileStream.CopyTo(deflateStream);
                    originalSize = (int)fileStream.Position;
                }
                wasmData = memoryStream.ToArray();
                mainJsReplacements.Add("WASM_COMPRESSED", "true");
            }
            else
            {
                wasmData = File.ReadAllBytes(WasmFileName);
                originalWasmSize = wasmData.Length;
                mainJsReplacements.Add("WASM_COMPRESSED", "false");
            }
            mainJsReplacements.Add("ORIGINAL_WASM_SIZE", $"{originalWasmSize}");

            // Build bundles for arena and world
            var mainJsWorldReplacements = new Dictionary<string, string>(mainJsReplacements)
            {
                { "STARTUP", GatherJs(WorldStartup, "") },
                { "LOOP", GatherJs(WorldLoop, "    ") }
            };
            var arenaJsWorldReplacements = new Dictionary<string, string>(mainJsReplacements)
            {
                { "STARTUP", GatherJs(ArenaStartup, "") },
                { "LOOP", GatherJs(ArenaLoop, "    ") }
            };
            var arenaFilePaths = BuildArena(Path.Combine(AppBundleDir, "arena"), wasmData, CompressWasm, originalWasmSize, arenaJsWorldReplacements);
            var worldFilePaths = BuildWorld(Path.Combine(AppBundleDir, "world"), wasmData, mainJsWorldReplacements);

            // Collect all emitted files
            BundleFilePaths = Enumerable.Empty<string>()
                .Concat(arenaFilePaths)
                .Concat(worldFilePaths)
                .ToArray();

            return !Log.HasLoggedErrors;
        }

        private static string GatherJs(string pathStr, string linePrefix)
        {
            var lines = new List<string>();
            var paths = pathStr.Split(',');
            foreach (var path in paths)
            {
                var text = File.ReadAllText(path);
                foreach (var line in text.Split('\n'))
                {
                    lines.Add($"{linePrefix}{line}");
                }
            }
            return string.Join("\n", lines);
        }

        private string Encode(byte[] data) => Encoding switch
        {
            "b64" => Convert.ToBase64String(data),
            "b32768" => Base32768.Encode(data),
            _ => throw new InvalidOperationException($"Encoding '{Encoding}' is unsupported")
        };

        private Encoding GetOutputEncoding() => Encoding switch
        {
            "b64" => System.Text.Encoding.UTF8,
            "b32768" => System.Text.Encoding.Unicode,
            _ => throw new InvalidOperationException($"Encoding '{Encoding}' is unsupported")
        };

        private IEnumerable<string> BuildArena(string path, byte[] wasmData, bool wasmDataCompressed, int originalWasmSize, IReadOnlyDictionary<string, string> mainJsReplacements)
        {
            Directory.CreateDirectory(path);

            var outputs = new List<string>();

            var bundleFilePath = Path.Combine(path, "bundle.mjs");
            using (var output = File.Open(bundleFilePath, FileMode.OpenOrCreate))
            {
                output.SetLength(0);
                using var writer = new StreamWriter(output, GetOutputEncoding());
                if (wasmDataCompressed)
                {
                    writer.WriteLine("import { decompressWasm, decodeWasm } from './bootloader';");
                    writer.Write($"const encodedWasm = '");
                    writer.Write(Encode(wasmData));
                    writer.WriteLine("';");
                    writer.WriteLine($"export const WasmBytes = decompressWasm(decodeWasm(encodedWasm, {wasmData.Length}, '{Encoding}'), {originalWasmSize});");
                }
                else
                {
                    writer.WriteLine("import { decodeWasm } from './bootloader';");
                    writer.Write($"const encodedWasm = '");
                    writer.Write(Encode(wasmData));
                    writer.WriteLine("';");
                    writer.WriteLine($"export const WasmBytes = decodeWasm(encodedWasm, {wasmData.Length}, '{Encoding}');");
                }
            }
            outputs.Add(bundleFilePath);

            foreach (var jsPath in ArenaJsFiles.Split(','))
            {
                var outputPath = Path.Combine(path, Path.GetFileName(jsPath));
                File.WriteAllBytes(outputPath, File.ReadAllBytes(jsPath));
                outputs.Add(outputPath);
            }

            var mainJs = System.Text.Encoding.UTF8.GetString(BundleStaticAssets.arena_main_mjs);
            var mainOutputPath = Path.Combine(path, "main.mjs");
            File.WriteAllText(mainOutputPath, ProcessReplacements(mainJs, mainJsReplacements));
            outputs.Add(mainOutputPath);

            return outputs;
        }

        private IEnumerable<string> BuildWorld(string path, byte[] wasmData, IReadOnlyDictionary<string, string> mainJsReplacements)
        {
            Directory.CreateDirectory(path);

            var outputs = new List<string>();

            var wasmFilePath = Path.Combine(path, "ScreepsDotNet.wasm");
            File.WriteAllBytes(wasmFilePath, wasmData);
            outputs.Add(wasmFilePath);

            foreach (var jsPath in WorldJsFiles.Split(','))
            {
                var outputPath = Path.Combine(path, Path.GetFileName(jsPath));
                File.WriteAllBytes(outputPath, File.ReadAllBytes(jsPath));
                outputs.Add(outputPath);
            }

            var mainJs = System.Text.Encoding.UTF8.GetString(BundleStaticAssets.world_main_js);
            var mainOutputPath = Path.Combine(path, "main.js");
            File.WriteAllText(mainOutputPath, ProcessReplacements(mainJs, mainJsReplacements), System.Text.Encoding.UTF8);
            outputs.Add(mainOutputPath);

            return outputs;
        }

        private static string ProcessReplacements(string mainJs, IReadOnlyDictionary<string, string> replacements)
        {
            const int maxDepth = 10;
            bool didReplace;
            int depth = 0;
            do
            {
                didReplace = false;
                foreach (var pair in replacements)
                {
                    var newMainJs = mainJs.Replace($"/*{pair.Key}*/", pair.Value);
                    if (newMainJs != mainJs)
                    {
                        mainJs = newMainJs;
                        didReplace = true;
                    }
                }
                ++depth;
            }
            while (didReplace && depth < maxDepth);
            return mainJs;
        }
    }
}
