using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class NativeMapExtensions
    {
        [JSImport("set", "object")]
        internal static partial void SetRouteCallbackOnObject(JSObject obj, string key, Func<string, string, double> func);

        public static JSObject ToJS(this MapFindRouteOptions mapFindRouteOptions)
        {
            var obj = JSObject.Create();
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

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeMap : IMap
    {
        #region Imports

        [JSImport("map.describeExits", "game")]
        internal static partial JSObject Native_DescribeExits(string roomName);

        [JSImport("map.findExit", "game")]
        internal static partial int Native_FindExit(string fromRoom, string toRoom, JSObject? opts);

        [JSImport("map.findRoute", "game")]
        internal static partial JSObject[]? Native_FindRoute(string fromRoom, string toRoom, JSObject? opts);

        [JSImport("map.getRoomLinearDistance", "game")]
        internal static partial int Native_GetRoomLinearDistance(string roomName1, string roomName2, bool continuous);

        [JSImport("map.getRoomTerrain", "game")]
        internal static partial JSObject Native_GetRoomTerrain(string roomName);

        [JSImport("map.getWorldSize", "game")]
        internal static partial int Native_GetWorldSize();

        [JSImport("map.getRoomStatus", "game")]
        internal static partial JSObject Native_GetRoomStatus(string roomName);

        #endregion

        private NativeMapVisual? visualCache;

        public IMapVisual Visual => visualCache ??= new NativeMapVisual();

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
