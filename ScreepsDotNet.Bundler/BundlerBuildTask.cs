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
        public string Configuration { get; set; } = null!;

        [Required]
        public bool CompressAssemblies { get; set; } = true;

        [Required]
        public bool CompressWasm { get; set; } = true;

        [Required]
        public string Encoding { get; set; } = null!;

        [Output]
        public string[] BundleFilePaths { get; set; } = null!;

        private readonly struct BundledAsset
        {
            public readonly string Path;
            public readonly int OriginalSize;
            public readonly bool Compressed;
            public readonly string Encoding;
            public readonly string Encoded;

            public BundledAsset(string path, int originalSize, bool compressed, string encoding, string encoded)
            {
                Path = path;
                OriginalSize = originalSize;
                Compressed = compressed;
                Encoding = encoding;
                Encoded = encoded;
            }
        }

        public override bool Execute()
        {
            var monoConfig = JsonConvert.DeserializeObject<MonoConfig>(File.ReadAllText(Path.Combine(AppBundleDir, "mono-config.json")));
            if (monoConfig == null)
            {
                Log.LogError($"Failed to deserialise mono-config.json");
                return false;
            }
            monoConfig.Assets = monoConfig.Assets.Where(ShouldBundleAsset);

            IList<BundledAsset> bundledAssets = new List<BundledAsset>();
            foreach (var asset in monoConfig.Assets)
            {
                var localPath = GetAssetLocalPath(asset);
                var sourcePath = Path.Combine(AppBundleDir, localPath);
                if (ShouldCompressAsset(asset))
                {
                    using var memoryStream = new MemoryStream();
                    int originalSize;
                    using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                    {
                        using var fileStream = File.OpenRead(sourcePath);
                        fileStream.CopyTo(deflateStream);
                        originalSize = (int)fileStream.Position;
                    }
                    bundledAssets.Add(new BundledAsset(localPath.Replace('\\', '/'), originalSize, true, Encoding, Encode(memoryStream.ToArray())));
                }
                else
                {
                    byte[] data = File.ReadAllBytes(sourcePath);
                    bundledAssets.Add(new BundledAsset(localPath.Replace('\\', '/'), data.Length, false, Encoding, Encode(data)));
                }
            }

            var arenaFilePaths = BuildArena(Path.Combine(AppBundleDir, "arena"), monoConfig, bundledAssets);
            var worldFilePaths = BuildWorld(Path.Combine(AppBundleDir, "world"), monoConfig, bundledAssets);

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

        private IEnumerable<string> BuildArena(string path, MonoConfig monoConfig, IEnumerable<BundledAsset> bundledAssets)
        {
            Directory.CreateDirectory(path);

            var bundleFilePath = Path.Combine(path, "bundle.mjs");
            using (var output = File.Open(bundleFilePath, FileMode.OpenOrCreate))
            {
                output.SetLength(0);
                using var writer = new StreamWriter(output, GetOutputEncoding());
                writer.WriteLine($"export const manifest = [");
                foreach (var bundledAsset in bundledAssets)
                {
                    writer.Write($"  {{ path: './{bundledAsset.Path}', originalSize: {bundledAsset.OriginalSize}, compressed: {(bundledAsset.Compressed ? "true" : "false")}, {bundledAsset.Encoding}: '");
                    writer.Write(bundledAsset.Encoded);
                    writer.WriteLine($"' }},");
                }
                writer.WriteLine($"];");
                writer.Write($"export const config = ");
                writer.Write(JsonConvert.SerializeObject(monoConfig));
                writer.WriteLine($";");
            }

            File.WriteAllBytes(Path.Combine(path, "bootloader.mjs"), Configuration == "Release" ? BundleStaticAssets.arena_bootloader_release_mjs : BundleStaticAssets.arena_bootloader_debug_mjs);
            File.WriteAllBytes(Path.Combine(path, "bootloader.d.ts"), BundleStaticAssets.arena_bootloader_dts);
            File.WriteAllBytes(Path.Combine(path, "main.mjs"), BundleStaticAssets.arena_main_mjs);

            return new string[]
            {
                bundleFilePath,
                Path.Combine(path, "bootloader.mjs"),
                Path.Combine(path, "bootloader.d.ts"),
                Path.Combine(path, "main.mjs")
            };
        }

        private IEnumerable<string> BuildWorld(string path, MonoConfig monoConfig, IEnumerable<BundledAsset> bundledAssets)
        {
            Directory.CreateDirectory(path);

            var bundleFilePath = Path.Combine(path, "bundle.js");
            using (var output = File.Open(bundleFilePath, FileMode.OpenOrCreate))
            {
                output.SetLength(0);
                using var writer = new StreamWriter(output, GetOutputEncoding());
                writer.WriteLine($"const manifest = [");
                foreach (var bundledAsset in bundledAssets)
                {
                    writer.Write($"  {{ path: './{bundledAsset.Path}', originalSize: {bundledAsset.OriginalSize}, compressed: {(bundledAsset.Compressed ? "true" : "false")}, {bundledAsset.Encoding}: '");
                    writer.Write(bundledAsset.Encoded);
                    writer.WriteLine($"' }},");
                }
                writer.WriteLine($"];");
                writer.Write($"const config = ");
                writer.Write(JsonConvert.SerializeObject(monoConfig));
                writer.WriteLine($";");
                writer.WriteLine($"module.exports = {{ manifest, config }};");
            }

            File.WriteAllBytes(Path.Combine(path, "bootloader.js"), Configuration == "Release" ? BundleStaticAssets.world_bootloader_release_js : BundleStaticAssets.world_bootloader_debug_js);
            File.WriteAllBytes(Path.Combine(path, "main.js"), BundleStaticAssets.world_main_js);

            return new string[]
            {
                bundleFilePath,
                Path.Combine(path, "bootloader.js"),
                Path.Combine(path, "main.js")
            };
        }

        private static bool ShouldBundleAsset(MonoAsset monoAsset)
            => (monoAsset.Behavior == "assembly" && Path.GetExtension(monoAsset.Name) == ".dll") || monoAsset.Behavior == "dotnetwasm";

        private bool ShouldCompressAsset(MonoAsset monoAsset)
            => monoAsset.Behavior == "assembly" ? CompressAssemblies
             : monoAsset.Behavior == "dotnetwasm" ? CompressWasm
             : false;

        private static string GetAssetLocalPath(MonoAsset monoAsset)
            => monoAsset.Behavior == "assembly" ? Path.Combine("managed", monoAsset.Name) : monoAsset.Name;
    }
}
