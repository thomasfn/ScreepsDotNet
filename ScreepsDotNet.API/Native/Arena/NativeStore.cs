using System;
using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API;

namespace ScreepsDotNet.Native.Arena
{
    internal static class ResourceTypeExtensions
    {
        public static string ToJS(this ResourceType resourceType)
            => resourceType switch
            {
                ResourceType.Energy => "energy",
                _ => throw new NotImplementedException($"Unknown resource type '{resourceType}'"),
            };

        public static string? ToJS(this ResourceType? resourceType)
            => resourceType switch
            {
                null => null,
                ResourceType.Energy => "energy",
                _ => throw new NotImplementedException($"Unknown resource type '{resourceType}'"),
            };

        public static ResourceType ParseResourceType(this string str)
            => str switch
            {
                "energy" => ResourceType.Energy,
                _ => throw new NotImplementedException($"Unknown resource type '{str}'"),
            };
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

        private const int ResourceCount = 1;

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
