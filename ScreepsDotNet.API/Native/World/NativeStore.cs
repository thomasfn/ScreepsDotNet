using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    internal static class ResourceTypeExtensions
    {
        private static readonly ImmutableArray<Name> resourceToName =
        [
            Names.Energy,
            Names.Power,
            Name.CreateNew("H"),
            Name.CreateNew("O"),
            Name.CreateNew("U"),
            Name.CreateNew("L"),
            Name.CreateNew("K"),
            Name.CreateNew("Z"),
            Name.CreateNew("X"),
            Name.CreateNew("G"),
            Name.CreateNew("silicon"),
            Name.CreateNew("metal"),
            Name.CreateNew("biomass"),
            Name.CreateNew("mist"),
            Name.CreateNew("OH"),
            Name.CreateNew("ZK"),
            Name.CreateNew("UL"),
            Name.CreateNew("UH"),
            Name.CreateNew("UO"),
            Name.CreateNew("KH"),
            Name.CreateNew("KO"),
            Name.CreateNew("LH"),
            Name.CreateNew("LO"),
            Name.CreateNew("ZH"),
            Name.CreateNew("ZO"),
            Name.CreateNew("GH"),
            Name.CreateNew("GO"),
            Name.CreateNew("UH2O"),
            Name.CreateNew("UHO2"),
            Name.CreateNew("KH2O"),
            Name.CreateNew("KHO2"),
            Name.CreateNew("LH2O"),
            Name.CreateNew("LHO2"),
            Name.CreateNew("ZH2O"),
            Name.CreateNew("ZHO2"),
            Name.CreateNew("GH2O"),
            Name.CreateNew("GHO2"),
            Name.CreateNew("XUH2O"),
            Name.CreateNew("XUHO2"),
            Name.CreateNew("XKH2O"),
            Name.CreateNew("XKHO2"),
            Name.CreateNew("XLH2O"),
            Name.CreateNew("XLHO2"),
            Name.CreateNew("XZH2O"),
            Name.CreateNew("XZHO2"),
            Name.CreateNew("XGH2O"),
            Name.CreateNew("XGHO2"),
            Name.CreateNew("ops"),
            Name.CreateNew("utrium_bar"),
            Name.CreateNew("lemergium_bar"),
            Name.CreateNew("zynthium_bar"),
            Name.CreateNew("keanium_bar"),
            Name.CreateNew("ghodium_melt"),
            Name.CreateNew("oxidant"),
            Name.CreateNew("reductant"),
            Name.CreateNew("purifier"),
            Name.CreateNew("battery"),
            Name.CreateNew("composite"),
            Name.CreateNew("crystal"),
            Name.CreateNew("liquid"),
            Name.CreateNew("wire"),
            Name.CreateNew("switch"),
            Name.CreateNew("transistor"),
            Name.CreateNew("microchip"),
            Name.CreateNew("circuit"),
            Name.CreateNew("device"),
            Name.CreateNew("cell"),
            Name.CreateNew("phlegm"),
            Name.CreateNew("tissue"),
            Name.CreateNew("muscle"),
            Name.CreateNew("organoid"),
            Name.CreateNew("organism"),
            Name.CreateNew("alloy"),
            Name.CreateNew("tube"),
            Name.CreateNew("fixtures"),
            Name.CreateNew("frame"),
            Name.CreateNew("hydraulics"),
            Name.CreateNew("machine"),
            Name.CreateNew("condensate"),
            Name.CreateNew("concentrate"),
            Name.CreateNew("extract"),
            Name.CreateNew("spirit"),
            Name.CreateNew("emanation"),
            Name.CreateNew("essence"),
            Name.CreateNew("season"),
            Name.CreateNew("unknown")
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
            => nameToResource.TryGetValue(str, out var resourceType) ? resourceType : ResourceType.Unknown;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStore : IStore, IDisposable
    {
        #region Imports

        [JSImport("Store.getCapacity", "game/prototypes/wrapped")]
        
        internal static partial int? Native_GetCapacity(JSObject proxyObject, Name? resourceType);

        [JSImport("Store.getFreeCapacity", "game/prototypes/wrapped")]
        
        internal static partial int? Native_GetFreeCapacity(JSObject proxyObject, Name? resourceType);

        [JSImport("Store.getUsedCapacity", "game/prototypes/wrapped")]
        
        internal static partial int? Native_GetUsedCapacity(JSObject proxyObject, Name? resourceType);

        #endregion

        private const int ResourceCount = (int)ResourceType.Unknown + 1;

        internal readonly JSObject? ProxyObject;

        private int[]? resourceCache;
        private ImmutableArray<ResourceType>? containedResourceTypesCache;
        private bool disposedValue;

        public IEnumerable<ResourceType> ContainedResourceTypes
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return containedResourceTypesCache ??= (ProxyObject?.GetPropertyNamesAsNames() ?? []).Select(static x => x.ParseResourceType()).ToImmutableArray();
            }
        }

        public int this[ResourceType resourceType]
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                if (resourceCache == null)
                {
                    resourceCache = new int[ResourceCount];
                    resourceCache.AsSpan().Fill(-1);
                }
                ref int amount = ref resourceCache[(int)resourceType];
                if (amount < 0) { amount = ProxyObject?.TryGetPropertyAsInt32(resourceType.ToJS()) ?? 0; }
                return amount;
            }
            set
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                if (resourceCache == null)
                {
                    resourceCache = new int[ResourceCount];
                    resourceCache.AsSpan().Fill(-1);
                }
                resourceCache[(int)resourceType] = Math.Max(0, value);
            }
        }

        public NativeStore(JSObject? proxyObject)
        {
            ProxyObject = proxyObject;
        }

        public int? GetCapacity(ResourceType? resourceType = null)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return ProxyObject != null ? Native_GetCapacity(ProxyObject, resourceType?.ToJS()) : null;
        }

        public int? GetFreeCapacity(ResourceType? resourceType = null)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return ProxyObject != null ? Native_GetFreeCapacity(ProxyObject, resourceType?.ToJS()) : null;
        }

        public int? GetUsedCapacity(ResourceType? resourceType = null)
        {
            ObjectDisposedException.ThrowIf(disposedValue, this);
            return ProxyObject != null ? Native_GetUsedCapacity(ProxyObject, resourceType?.ToJS()) : null;
        }

        #region Disposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    resourceCache = null;
                }
                ProxyObject?.Dispose();
                disposedValue = true;
            }
        }

        ~NativeStore()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
