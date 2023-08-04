using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static partial class NativeMapExtensions
    {
        [JSImport("set", "object")]
        internal static partial void SetRouteCallbackOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key, [JSMarshalAs<JSType.Function<JSType.String, JSType.String, JSType.Number>>] Func<string, string, double> func);

        public static JSObject ToJS(this MapFindRouteOptions mapFindRouteOptions)
        {
            var obj = NativeRoomObjectUtils.CreateObject(null);
            if (mapFindRouteOptions.RouteCallback != null) { SetRouteCallbackOnObject(obj, "routeCallback", mapFindRouteOptions.RouteCallback); }
            return obj;
        }

        public static MapFindRouteStep ToMapFindRouteStep(this JSObject obj)
            => new((ExitDirection)obj.GetPropertyAsInt32("exit"), obj.GetPropertyAsString("room")!);

        public static RoomStatusType ParseRoomStatusType(this string str) => str switch
        {
            "normal" => RoomStatusType.Normal,
            "closed" => RoomStatusType.Closed,
            "novice" => RoomStatusType.Novice,
            "respawn" => RoomStatusType.Respawn,
            _ => throw new InvalidOperationException($"Unknown room status type '{str}'"),
        };
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeMap : IMap
    {
        #region Imports

        [JSImport("map.describeExits", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_DescribeExits([JSMarshalAs<JSType.String>] string roomName);

        [JSImport("map.findExit", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_FindExit([JSMarshalAs<JSType.String>] string fromRoom, [JSMarshalAs<JSType.String>] string toRoom, [JSMarshalAs<JSType.Object>] JSObject? opts);

        [JSImport("map.findRoute", "game")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[]? Native_FindRoute([JSMarshalAs<JSType.String>] string fromRoom, [JSMarshalAs<JSType.String>] string toRoom, [JSMarshalAs<JSType.Object>] JSObject? opts);

        [JSImport("map.getRoomLinearDistance", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetRoomLinearDistance([JSMarshalAs<JSType.String>] string roomName1, [JSMarshalAs<JSType.String>] string roomName2, [JSMarshalAs<JSType.Boolean>] bool continuous);

        [JSImport("map.getRoomTerrain", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetRoomTerrain([JSMarshalAs<JSType.String>] string roomName);

        [JSImport("map.getWorldSize", "game")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetWorldSize();

        [JSImport("map.getRoomStatus", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetRoomStatus([JSMarshalAs<JSType.String>] string roomName);

        #endregion

        public RoomExits DescribeExits(string roomName)
        {
            var obj = Native_DescribeExits(roomName);
            return new(obj.GetPropertyAsString("1"), obj.GetPropertyAsString("3"), obj.GetPropertyAsString("5"), obj.GetPropertyAsString("7"));
        }

        public MapFindExitResult FindExit(string fromRoomName, string toRoomName, out ExitDirection exitDirection, MapFindRouteOptions? opts = null)
        {
            using var optsJs = opts?.ToJS();
            var value = Native_FindExit(fromRoomName, toRoomName, optsJs);
            if (value < 0)
            {
                exitDirection = default;
                return (MapFindExitResult)value;
            }
            exitDirection = (ExitDirection)value;
            return MapFindExitResult.Ok;
        }

        public MapFindExitResult FindExit(IRoom fromRoom, IRoom toRoom, out ExitDirection exitDirection, MapFindRouteOptions? opts = null)
            => FindExit(fromRoom.Name, toRoom.Name, out exitDirection, opts);

        public MapFindRouteResult FindRoute(string fromRoomName, string toRoomName, out IEnumerable<MapFindRouteStep> steps, MapFindRouteOptions? opts = null)
        {
            using var optsJs = opts?.ToJS();
            var arr = Native_FindRoute(fromRoomName, toRoomName, optsJs);
            if (arr == null)
            {
                steps = Enumerable.Empty<MapFindRouteStep>();
                return MapFindRouteResult.NoPath;
            }
            steps = arr.Select(x => x.ToMapFindRouteStep()).ToArray();
            return MapFindRouteResult.Ok;
        }

        public MapFindRouteResult FindRoute(IRoom fromRoom, IRoom toRoom, out IEnumerable<MapFindRouteStep> steps, MapFindRouteOptions? opts = null)
            => FindRoute(fromRoom.Name, toRoom.Name, out steps, opts);

        public int GetRoomLinearDistance(string roomName1, string roomName2, bool continuous = false)
            => Native_GetRoomLinearDistance(roomName1, roomName2, continuous);

        public IRoomTerrain GetRoomTerrain(string roomName)
            => new NativeRoomTerrain(Native_GetRoomTerrain(roomName));

        public int GetWorldSize()
            => Native_GetWorldSize();

        public RoomStatus GetRoomStatus(string roomName)
        {
            var obj = Native_GetRoomStatus(roomName);
            double timestamp = obj.GetPropertyAsDouble("timestamp");
            return new(obj.GetPropertyAsString("status")!.ParseRoomStatusType(), timestamp > 0.0 ? DateTime.UnixEpoch + TimeSpan.FromMilliseconds(timestamp) : null);
        }
    }
}
