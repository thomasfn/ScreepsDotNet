using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native.World
{
    internal static class ResourceTypeExtensions
    {
        public static string ToJS(this ResourceType resourceType)
            => resourceType switch
            {
                ResourceType.Energy => "energy",
                ResourceType.Power => "power",
                ResourceType.Hydrogen => "H",
                ResourceType.Oxygen => "O",
                ResourceType.Utrium => "U",
                ResourceType.Lemergium => "L",
                ResourceType.Keanium => "K",
                ResourceType.Zynthium => "Z",
                ResourceType.Catalyst => "X",
                ResourceType.Ghodium => "G",
                ResourceType.Silicon => "silicon",
                ResourceType.Metal => "metal",
                ResourceType.Biomass => "biomass",
                ResourceType.Mist => "mist",
                _ => throw new NotImplementedException($"Unknown resource type '{resourceType}'"),
            };

        public static ResourceType ParseResourceType(this string str)
            => str switch
            {
                "energy" => ResourceType.Energy,
                "power" => ResourceType.Power,
                "H" => ResourceType.Hydrogen,
                "O" => ResourceType.Oxygen,
                "U" => ResourceType.Utrium,
                "L" => ResourceType.Lemergium,
                "K" => ResourceType.Keanium,
                "Z" => ResourceType.Zynthium,
                "X" => ResourceType.Catalyst,
                "G" => ResourceType.Ghodium,
                "silicon" => ResourceType.Silicon,
                "metal" => ResourceType.Metal,
                "biomass" => ResourceType.Biomass,
                "mist" => ResourceType.Mist,
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
            => ProxyObject != null ? Native_GetCapacity(ProxyObject, resourceType?.ToJS()) : null;

        public int? GetFreeCapacity(ResourceType? resourceType = null)
            => ProxyObject != null ? Native_GetFreeCapacity(ProxyObject, resourceType?.ToJS()) : null;

        public int? GetUsedCapacity(ResourceType? resourceType = null)
            => ProxyObject != null ? Native_GetUsedCapacity(ProxyObject, resourceType?.ToJS()) : null;
    }
}
