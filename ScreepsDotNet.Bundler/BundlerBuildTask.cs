using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ScreepsDotNet.Bundler
{
    public class BundlerBuildTask : Task
    {
        [Required]
        public string AppBundleDir { get; set; } = null!;

        [Output]
        public string[] BundleFilePaths { get; set; } = null!;

        public override bool Execute()
        {
            var monoConfig = JsonConvert.DeserializeObject<MonoConfig>(File.ReadAllText(Path.Combine(AppBundleDir, "mono-config.json")));
            if (monoConfig == null)
            {
                Log.LogError($"Failed to deserialise mono-config.json");
                return false;
            }
            monoConfig.Assets = monoConfig.Assets.Where(ShouldBundleAsset);

            var bundleFilePath = Path.Combine(AppBundleDir, "bundle.mjs");
            using (var output = File.Open(bundleFilePath, FileMode.OpenOrCreate))
            {
                output.SetLength(0);
                using var writer = new StreamWriter(output);
                writer.WriteLine($"export const manifest = [");
                foreach (var asset in monoConfig.Assets)
                {
                    var localPath = GetAssetLocalPath(asset);
                    var sourcePath = Path.Combine(AppBundleDir, localPath);
                    using var memoryStream = new MemoryStream();
                    using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                    {
                        using var fileStream = File.OpenRead(sourcePath);
                        fileStream.CopyTo(deflateStream);
                    }
                    var b64 = Convert.ToBase64String(memoryStream.ToArray());
                    writer.Write($"  {{ path: './{localPath.Replace('\\', '/')}', compressed: true, b64: '");
                    writer.Write(b64);
                    writer.WriteLine($"' }},");
                }
                writer.WriteLine($"];");
                writer.Write($"export const config = ");
                writer.Write(JsonConvert.SerializeObject(monoConfig));
                writer.WriteLine($";");
            }

            File.WriteAllBytes(Path.Combine(AppBundleDir, "bootloader.mjs"), BundleStaticAssets.bootloader_mjs);
            File.WriteAllBytes(Path.Combine(AppBundleDir, "bootloader.d.ts"), BundleStaticAssets.bootloader_dts);
            File.WriteAllBytes(Path.Combine(AppBundleDir, "main.mjs"), BundleStaticAssets.main_mjs);

            BundleFilePaths = new string[]
            {
                bundleFilePath,
                Path.Combine(AppBundleDir, "bootloader.mjs"),
                Path.Combine(AppBundleDir, "bootloader.d.ts"),
                Path.Combine(AppBundleDir, "main.mjs")
            };

            return !Log.HasLoggedErrors;
        }

        private static bool ShouldBundleAsset(MonoAsset monoAsset)
            => (monoAsset.Behavior == "assembly" && Path.GetExtension(monoAsset.Name) == ".dll") || monoAsset.Behavior == "dotnetwasm";

        private static string GetAssetLocalPath(MonoAsset monoAsset)
            => monoAsset.Behavior == "assembly" ? Path.Combine("managed", monoAsset.Name) : monoAsset.Name;
    }
}
