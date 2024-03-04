using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    internal static class ResourceTypeExtensions
    {
        private static readonly ImmutableArray<Name> resourceToName =
        [
            Names.Energy,
            Name.CreateNew("score"),
            Name.CreateNew("score_x"),
            Name.CreateNew("score_y"),
            Name.CreateNew("score_z"),
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

        public NativeStore(JSObject? proxyObject)
        {
            ProxyObject = proxyObject;
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
                if (amount < 0) { amount = ProxyObject?.GetPropertyAsInt32(resourceType.ToJS()) ?? 0; }
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ProxyObject?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
