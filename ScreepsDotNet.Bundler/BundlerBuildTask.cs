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

        private readonly struct TextEncodedData
        {
            public readonly string Encoding;
            public readonly string Encoded;

            public TextEncodedData(string encoding, string encoded)
            {
                Encoding = encoding;
                Encoded = encoded;
            }
        }

        private readonly struct BinaryEncodedData
        {
            public readonly int Offset;
            public readonly int Length;

            public BinaryEncodedData(int offset, int length)
            {
                Offset = offset;
                Length = length;
            }
        }

        private readonly struct BundledAsset
        {
            public readonly string Path;
            public readonly int OriginalSize;
            public readonly bool Compressed;
            public readonly TextEncodedData? TextEncodedData;
            public readonly BinaryEncodedData? BinaryEncodedData;

            public BundledAsset(string path, int originalSize, bool compressed, TextEncodedData? textEncodedData, BinaryEncodedData? binaryEncodedData)
            {
                Path = path;
                OriginalSize = originalSize;
                Compressed = compressed;
                TextEncodedData = textEncodedData;
                BinaryEncodedData = binaryEncodedData;
            }

            public BundledAsset(string path, int originalSize, bool compressed, TextEncodedData textEncodedData)
            {
                Path = path;
                OriginalSize = originalSize;
                Compressed = compressed;
                TextEncodedData = textEncodedData;
                BinaryEncodedData = null;
            }

            public BundledAsset(string path, int originalSize, bool compressed, BinaryEncodedData binaryEncodedData)
            {
                Path = path;
                OriginalSize = originalSize;
                Compressed = compressed;
                TextEncodedData = null;
                BinaryEncodedData = binaryEncodedData;
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

            var bundledAssets = new List<BundledAsset>();
            var rawDatas = new List<byte[]>();
            int rawDataHead = 0;
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
                    byte[] rawData = memoryStream.ToArray();
                    if (Encoding == "bin")
                    {
                        bundledAssets.Add(new BundledAsset(localPath.Replace('\\', '/'), originalSize, true, new BinaryEncodedData(rawDataHead, rawData.Length)));
                        rawDatas.Add(rawData);
                        rawDataHead += rawData.Length;
                    }
                    else
                    {
                        bundledAssets.Add(new BundledAsset(localPath.Replace('\\', '/'), originalSize, true, new TextEncodedData(Encoding, Encode(rawData))));
                    }
                }
                else
                {
                    byte[] rawData = File.ReadAllBytes(sourcePath);
                    if (Encoding == "bin")
                    {
                        bundledAssets.Add(new BundledAsset(localPath.Replace('\\', '/'), rawData.Length, false, new BinaryEncodedData(rawDataHead, rawData.Length)));
                        rawDatas.Add(rawData);
                        rawDataHead += rawData.Length;
                    }
                    else
                    {
                        bundledAssets.Add(new BundledAsset(localPath.Replace('\\', '/'), rawData.Length, false, new TextEncodedData(Encoding, Encode(rawData))));
                    }
                }
            }

            var arenaFilePaths = BuildArena(Path.Combine(AppBundleDir, "arena"), monoConfig, bundledAssets);
            var worldFilePaths = BuildWorld(Path.Combine(AppBundleDir, "world"), monoConfig, bundledAssets);

            if (Encoding == "bin")
            {
                byte[] binaryData = new byte[rawDataHead];
                rawDataHead = 0;
                foreach (byte[] rawData in rawDatas)
                {
                    Array.Copy(rawData, 0, binaryData, rawDataHead, rawData.Length);
                    rawDataHead += rawData.Length;
                }
                var arenaBundleBinFilename = Path.Combine(Path.Combine(AppBundleDir, "arena"), "bundle-bin.wasm");
                File.WriteAllBytes(arenaBundleBinFilename, binaryData);
                arenaFilePaths = arenaFilePaths.Append(arenaBundleBinFilename);
                var worldBundleBinFilename = Path.Combine(Path.Combine(AppBundleDir, "world"), "bundle-bin.wasm");
                File.WriteAllBytes(worldBundleBinFilename, binaryData);
                worldFilePaths = worldFilePaths.Append(worldBundleBinFilename);
            }

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
            "bin" => "",
            _ => throw new InvalidOperationException($"Encoding '{Encoding}' is unsupported")
        };

        private Encoding GetOutputEncoding() => Encoding switch
        {
            "b64" => System.Text.Encoding.UTF8,
            "bin" => System.Text.Encoding.UTF8,
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
                    writer.Write($"  {{ path: './{bundledAsset.Path}', originalSize: {bundledAsset.OriginalSize}, compressed: {(bundledAsset.Compressed ? "true" : "false")}");
                    if (bundledAsset.TextEncodedData != null)
                    {
                        writer.Write($", {bundledAsset.TextEncodedData.Value.Encoding}: '");
                        writer.Write(bundledAsset.TextEncodedData.Value.Encoded);
                        writer.Write("'");
                    }
                    if (bundledAsset.BinaryEncodedData != null)
                    {
                        writer.Write($", offset: {bundledAsset.BinaryEncodedData.Value.Offset}");
                        writer.Write($", length: {bundledAsset.BinaryEncodedData.Value.Length}");
                    }
                    writer.WriteLine($" }},");
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
                    writer.Write($"  {{ path: './{bundledAsset.Path}', originalSize: {bundledAsset.OriginalSize}, compressed: {(bundledAsset.Compressed ? "true" : "false")}");
                    if (bundledAsset.TextEncodedData != null)
                    {
                        writer.Write($", {bundledAsset.TextEncodedData.Value.Encoding}: '");
                        writer.Write(bundledAsset.TextEncodedData.Value.Encoded);
                        writer.Write("'");
                    }
                    if (bundledAsset.BinaryEncodedData != null)
                    {
                        writer.Write($", offset: {bundledAsset.BinaryEncodedData.Value.Offset}");
                        writer.Write($", length: {bundledAsset.BinaryEncodedData.Value.Length}");
                    }
                    writer.WriteLine($" }},");
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
