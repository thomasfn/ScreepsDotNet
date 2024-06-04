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
            var arenaFilePaths = BuildArena(Path.Combine(AppBundleDir, "arena"), wasmData, CompressWasm, originalWasmSize, mainJsReplacements);
            var worldFilePaths = BuildWorld(Path.Combine(AppBundleDir, "world"), wasmData, mainJsReplacements);

            // Collect all emitted files
            BundleFilePaths = Enumerable.Empty<string>()
                .Concat(arenaFilePaths)
                .Concat(worldFilePaths)
                .ToArray();

            return !Log.HasLoggedErrors;
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

            File.WriteAllBytes(Path.Combine(path, "bootloader.mjs"), BundleStaticAssets.arena_bootloader_mjs);
            File.WriteAllBytes(Path.Combine(path, "bootloader.d.ts"), BundleStaticAssets.arena_bootloader_dts);

            var mainJs = System.Text.Encoding.UTF8.GetString(BundleStaticAssets.arena_main_mjs);
            File.WriteAllText(Path.Combine(path, "main.mjs"), ProcessMainJsReplacements(mainJs, mainJsReplacements));

            return new string[]
            {
                bundleFilePath,
                Path.Combine(path, "bootloader.mjs"),
                Path.Combine(path, "bootloader.d.ts"),
                Path.Combine(path, "main.mjs")
            };
        }

        private IEnumerable<string> BuildWorld(string path, byte[] wasmData, IReadOnlyDictionary<string, string> mainJsReplacements)
        {
            Directory.CreateDirectory(path);

            File.WriteAllBytes(Path.Combine(path, "ScreepsDotNet.wasm"), wasmData);
            File.WriteAllBytes(Path.Combine(path, "bootloader.js"), BundleStaticAssets.world_bootloader_js);

            var mainJs = System.Text.Encoding.UTF8.GetString(BundleStaticAssets.world_main_js);
            File.WriteAllText(Path.Combine(path, "main.js"), ProcessMainJsReplacements(mainJs, mainJsReplacements), System.Text.Encoding.UTF8);

            return new string[]
            {
                Path.Combine(path, "ScreepsDotNet.wasm"),
                Path.Combine(path, "bootloader.js"),
                Path.Combine(path, "main.js")
            };
        }

        private string ProcessMainJsReplacements(string mainJs, IReadOnlyDictionary<string, string> replacements)
        {
            foreach (var pair in replacements)
            {
                mainJs = mainJs.Replace($"/*{pair.Key}*/", pair.Value);
            }
            return mainJs;
        }
    }
}
