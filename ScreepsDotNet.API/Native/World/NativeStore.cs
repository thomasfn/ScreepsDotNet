using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native.World
{
    internal static class ResourceTypeExtensions
    {
        private static readonly ImmutableArray<string> resourceStrings = new string[]
        {
            "energy", "power",
            "H", "O", "U", "L", "K", "Z", "X", "G",
            "silicon", "metal", "biomass", "mist",
            "OH", "ZK", "UL", "UH", "UO", "KH", "KO", "LH", "LO", "ZH", "ZO", "GH", "GO",
            "UH2O", "UHO2", "KH2O", "KHO2", "LH2O", "LHO2", "ZH2O", "ZHO2", "GH2O", "GHO2",
            "XUH2O", "XUHO2", "XKH2O", "XKHO2", "XLH2O", "XLHO2", "XZH2O", "XZHO2", "XGH2O", "XGHO2",
            "ops",
            "utrium_bar", "lemergium_bar", "zynthium_bar", "keanium_bar", "ghodium_melt", "oxidant", "reductant", "purifier", "battery",
            "composite", "crystal", "liquid",
            "wire", "switch", "transistor", "microchip", "circuit", "device",
            "cell", "phlegm", "tissue", "muscle", "organoid", "organism",
            "alloy", "tube", "fixtures", "frame", "hydraulics", "machine",
            "condensate", "concentrate", "extract", "spirit", "emanation", "essence",
            ""
        }.ToImmutableArray();

        private static readonly Dictionary<string, ResourceType> stringResources = new();

        static ResourceTypeExtensions()
        {
            for (int i = 0; i < resourceStrings.Length; ++i)
            {
                stringResources.Add(resourceStrings[i], (ResourceType)i);
            }
        }

        public static string ToJS(this ResourceType resourceType)
            => resourceStrings[(int)resourceType];

        public static ResourceType ParseResourceType(this string str)
            => stringResources.TryGetValue(str, out var resourceType) ? resourceType : ResourceType.Unknown;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStore : IStore
    {
        #region Imports

        [JSImport("Store.getCapacity", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int? Native_GetCapacity([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string? resourceType);

        [JSImport("Store.getFreeCapacity", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int? Native_GetFreeCapacity([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string? resourceType);

        [JSImport("Store.getUsedCapacity", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int? Native_GetUsedCapacity([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string? resourceType);

        #endregion

        private const int ResourceCount = (int)ResourceType.Unknown + 1;

        internal readonly JSObject? ProxyObject;

        private int[]? resourceCache;

        public NativeStore(JSObject? proxyObject)
        {
            ProxyObject = proxyObject;
        }

        public int this[ResourceType resourceType]
        {
            get
            {
                if (resourceCache == null)
                {
                    resourceCache = new int[ResourceCount];
                    resourceCache.AsSpan().Fill(-1);
                }
                ref int amount = ref resourceCache[(int)resourceType];
                if (amount < 0) { amount = ProxyObject?.GetPropertyAsInt32(resourceType.ToJS()) ?? 0; }
                return amount;
            }
            set
            {
                if (resourceCache == null)
                {
                    resourceCache = new int[ResourceCount];
                    resourceCache.AsSpan().Fill(-1);
                }
                resourceCache[(int)resourceType] = Math.Max(0, value);
            }
        }

        public int? GetCapacity(ResourceType? resourceType = null)
            => ProxyObject != null ? Native_GetCapacity(ProxyObject, resourceType?.ToJS()) : null;

        public int? GetFreeCapacity(ResourceType? resourceType = null)
            => ProxyObject != null ? Native_GetFreeCapacity(ProxyObject, resourceType?.ToJS()) : null;

        public int? GetUsedCapacity(ResourceType? resourceType = null)
            => ProxyObject != null ? Native_GetUsedCapacity(ProxyObject, resourceType?.ToJS()) : null;
    }
}
