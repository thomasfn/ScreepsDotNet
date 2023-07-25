using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace ScreepsDotNet.Bundler
{
    internal class MonoAsset
    {
        [JsonProperty("behavior")]
        public string Behavior { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("virtualPath")]
        public string? VirtualPath { get; set; } = null;
    }

    internal class MonoConfig
    {
        [JsonProperty("mainAssemblyName")]
        public string MainAssemblyName { get; set; } = "";

        [JsonProperty("assemblyRootFolder")]
        public string AssemblyRootFolder { get; set; } = "";

        [JsonProperty("debugLevel")]
        public int DebugLevel { get; set; } = -1;

        [JsonProperty("assets")]
        public IEnumerable<MonoAsset> Assets { get; set; } = Enumerable.Empty<MonoAsset>();

        [JsonProperty("remoteSources")]
        public IEnumerable<string> RemoteSources { get; set; } = Enumerable.Empty<string>();

        [JsonProperty("pthreadPoolSize")]
        public int PThreadPoolSize { get; set; } = 0;
    }
}
