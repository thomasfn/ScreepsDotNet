using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    internal static class ResourceTypeExtensions
    {
        private static readonly Name[] resourceToName =
        [
            Names.Energy,
            Names.Power,
            Name.Create("H"),
            Name.Create("O"),
            Name.Create("U"),
            Name.Create("L"),
            Name.Create("K"),
            Name.Create("Z"),
            Name.Create("X"),
            Name.Create("G"),
            Name.Create("silicon"),
            Name.Create("metal"),
            Name.Create("biomass"),
            Name.Create("mist"),
            Name.Create("OH"),
            Name.Create("ZK"),
            Name.Create("UL"),
            Name.Create("UH"),
            Name.Create("UO"),
            Name.Create("KH"),
            Name.Create("KO"),
            Name.Create("LH"),
            Name.Create("LO"),
            Name.Create("ZH"),
            Name.Create("ZO"),
            Name.Create("GH"),
            Name.Create("GO"),
            Name.Create("UH2O"),
            Name.Create("UHO2"),
            Name.Create("KH2O"),
            Name.Create("KHO2"),
            Name.Create("LH2O"),
            Name.Create("LHO2"),
            Name.Create("ZH2O"),
            Name.Create("ZHO2"),
            Name.Create("GH2O"),
            Name.Create("GHO2"),
            Name.Create("XUH2O"),
            Name.Create("XUHO2"),
            Name.Create("XKH2O"),
            Name.Create("XKHO2"),
            Name.Create("XLH2O"),
            Name.Create("XLHO2"),
            Name.Create("XZH2O"),
            Name.Create("XZHO2"),
            Name.Create("XGH2O"),
            Name.Create("XGHO2"),
            Name.Create("ops"),
            Name.Create("utrium_bar"),
            Name.Create("lemergium_bar"),
            Name.Create("zynthium_bar"),
            Name.Create("keanium_bar"),
            Name.Create("ghodium_melt"),
            Name.Create("oxidant"),
            Name.Create("reductant"),
            Name.Create("purifier"),
            Name.Create("battery"),
            Name.Create("composite"),
            Name.Create("crystal"),
            Name.Create("liquid"),
            Name.Create("wire"),
            Name.Create("switch"),
            Name.Create("transistor"),
            Name.Create("microchip"),
            Name.Create("circuit"),
            Name.Create("device"),
            Name.Create("cell"),
            Name.Create("phlegm"),
            Name.Create("tissue"),
            Name.Create("muscle"),
            Name.Create("organoid"),
            Name.Create("organism"),
            Name.Create("alloy"),
            Name.Create("tube"),
            Name.Create("fixtures"),
            Name.Create("frame"),
            Name.Create("hydraulics"),
            Name.Create("machine"),
            Name.Create("condensate"),
            Name.Create("concentrate"),
            Name.Create("extract"),
            Name.Create("spirit"),
            Name.Create("emanation"),
            Name.Create("essence"),
            Name.Create("season"),
            Name.Create("score"),
            Name.Create("unknown")
        ];

        private static readonly Dictionary<Name, ResourceType> nameToResource = [];

        static ResourceTypeExtensions()
        {
            for (int i = 0; i < resourceToName.Length; ++i)
            {
                nameToResource.Add(resourceToName[i], (ResourceType)i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Name ToJS(this ResourceType resourceType)
            => resourceToName[(int)resourceType];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ResourceType ParseResourceType(this Name str)
        {
            if (!nameToResource.TryGetValue(str, out var resourceType))
            {
                Console.WriteLine($"Failed to parse resource type '{str}'");
                return ResourceType.Unknown;
            }
            return resourceType;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStore : IStore
    {
        #region Imports

        [JSImport("RoomObject.getStoreCapacity", "game/prototypes/wrapped")]
        internal static partial int? Native_GetStoreCapacity(JSObject proxyObject, Name? resourceType);

        [JSImport("RoomObject.getStoreFreeCapacity", "game/prototypes/wrapped")]
        internal static partial int? Native_GetStoreFreeCapacity(JSObject proxyObject, Name? resourceType);

        [JSImport("RoomObject.getStoreUsedCapacity", "game/prototypes/wrapped")]
        internal static partial int? Native_GetStoreUsedCapacity(JSObject proxyObject, Name? resourceType);

        [JSImport("RoomObject.getStoreContainedResources", "game/prototypes/wrapped")]
        internal static partial Name[] Native_GetStoreContainedResources(JSObject proxyObject);

        [JSImport("RoomObject.indexStore", "game/prototypes/wrapped")]
        internal static partial int? Native_IndexStore(JSObject proxyObject, Name resourceType);

        #endregion

        private const int ResourceCount = (int)ResourceType.Unknown + 1;

        private readonly INativeRoot nativeRoot;
        private JSObject proxyObject;
        private int proxyObjectValidAsOf;

        private int[]? resourceCache;
        private ResourceType[]? containedResourceTypesCache;

        public IEnumerable<ResourceType> ContainedResourceTypes => containedResourceTypesCache ??= [.. Native_GetStoreContainedResources(ProxyObject).Select(static x => x.ParseResourceType())];

        public event Action? OnRequestNewProxyObject;

        public JSObject ProxyObject
        {
            get
            {
                int tickIndex = nativeRoot.TickIndex;
                if (proxyObjectValidAsOf < tickIndex)
                {
                    OnRequestNewProxyObject?.Invoke();
                }
                if (proxyObjectValidAsOf < tickIndex)
                {
                    throw new NativeObjectNoLongerExistsException();
                }
                return proxyObject;
            }
            set
            {
                proxyObject = value;
                RenewProxyObject();
            }
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
                if (amount < 0) { amount = Native_IndexStore(proxyObject, resourceType.ToJS()) ?? 0; }
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

        public NativeStore(INativeRoot nativeRoot, JSObject proxyObject)
        {
            this.nativeRoot = nativeRoot;
            this.proxyObject = proxyObject;
            proxyObjectValidAsOf = nativeRoot.TickIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RenewProxyObject()
        {
            proxyObjectValidAsOf = nativeRoot.TickIndex;
            ClearNativeCache();
        }

        public void ClearNativeCache()
        {
            resourceCache?.AsSpan().Fill(-1);
            containedResourceTypesCache = null;
        }

        public int? GetCapacity(ResourceType? resourceType = null) => Native_GetStoreCapacity(proxyObject, resourceType?.ToJS());

        public int? GetFreeCapacity(ResourceType? resourceType = null) => Native_GetStoreFreeCapacity(proxyObject, resourceType?.ToJS());

        public int? GetUsedCapacity(ResourceType? resourceType = null) => Native_GetStoreUsedCapacity(proxyObject, resourceType?.ToJS());

    }
}
