using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.Arena;

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

        internal readonly JSObject? ProxyObject;

        public NativeStore(JSObject? proxyObject)
        {
            ProxyObject = proxyObject;
        }

        public int this[ResourceType resourceType] => ProxyObject?.GetPropertyAsInt32(resourceType.ToJS()) ?? 0;

        public int? GetCapacity(ResourceType? resourceType = null)
            => ProxyObject != null ? Native_GetCapacity(ProxyObject, resourceType.ToJS()) : null;

        public int? GetFreeCapacity(ResourceType? resourceType = null)
            => ProxyObject != null ? Native_GetFreeCapacity(ProxyObject, resourceType.ToJS()) : null;

        public int? GetUsedCapacity(ResourceType? resourceType = null)
            => ProxyObject != null ? Native_GetUsedCapacity(ProxyObject, resourceType.ToJS()) : null;
    }
}
