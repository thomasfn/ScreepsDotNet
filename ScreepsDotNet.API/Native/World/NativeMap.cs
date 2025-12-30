using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.Interop;

using System.Diagnostics.CodeAnalysis;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class NativeMapExtensions
    {
        #region Imports

        [JSImport("map.getRouteCallbackObject", "game")]
        internal static partial JSObject Native_GetRouteCallbackObject();

        #endregion

        private static JSObject? routeCallbackObject;

        private static readonly Dictionary<Name, RoomStatusType> nameToRoomStatusTypeMap = new()
        {
            { Names.Normal, RoomStatusType.Normal },
            { Names.Closed, RoomStatusType.Closed },
            { Names.Novice, RoomStatusType.Novice },
            { Names.Respawn, RoomStatusType.Respawn }
        };

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(NativeMapExtensions))]
        public static JSObject ToJS(this MapFindRouteOptions mapFindRouteOptions)
        {
            var obj = JSObject.Create();
            if (mapFindRouteOptions.RouteCallback != null)
            {
                NativeCallbacks.currentRouteCallbackFunc = mapFindRouteOptions.RouteCallback;
                obj.SetProperty(Names.RouteCallback, routeCallbackObject ??= Native_GetRouteCallbackObject());
            }
            return obj;
        }

        public static MapFindRouteStep ToMapFindRouteStep(this JSObject obj)
            => new((ExitDirection)obj.GetPropertyAsInt32(Names.Exit), new(obj.GetPropertyAsString(Names.Room)!));

        public static RoomStatusType ParseRoomStatusType(this Name name) => nameToRoomStatusTypeMap[name];
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeMap : IMap
    {
        #region Imports

        [JSImport("map.describeExits", "game")]
        internal static partial JSObject? Native_DescribeExits(string roomName);

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

        public RoomExits DescribeExits(RoomCoord roomCoord)
        {
            using var obj = Native_DescribeExits(roomCoord.ToString());
            if (obj == null) { return new(); }
            var topName = obj.GetPropertyAsString("1");
            var rightName = obj.GetPropertyAsString("3");
            var bottomName = obj.GetPropertyAsString("5");
            var leftName = obj.GetPropertyAsString("7");
            return new(
                top: string.IsNullOrEmpty(topName) ? null : new(topName),
                right: string.IsNullOrEmpty(rightName) ? null : new(rightName),
                bottom: string.IsNullOrEmpty(bottomName) ? null : new(bottomName),
                left: string.IsNullOrEmpty(leftName) ? null : new(leftName)
            );
        }

        public MapFindExitResult FindExit(RoomCoord fromRoomCoord, RoomCoord toRoomCoord, out ExitDirection exitDirection, MapFindRouteOptions? opts = null)
        {
            using var optsJs = opts?.ToJS();
            var value = Native_FindExit(fromRoomCoord.ToString(), toRoomCoord.ToString(), optsJs);
            if (value < 0)
            {
                exitDirection = default;
                return (MapFindExitResult)value;
            }
            exitDirection = (ExitDirection)value;
            return MapFindExitResult.Ok;
        }

        public MapFindExitResult FindExit(IRoom fromRoom, IRoom toRoom, out ExitDirection exitDirection, MapFindRouteOptions? opts = null)
            => FindExit(fromRoom.Coord, toRoom.Coord, out exitDirection, opts);

        public MapFindRouteResult FindRoute(RoomCoord fromRoomCoord, RoomCoord toRoomCoord, out IEnumerable<MapFindRouteStep> steps, MapFindRouteOptions? opts = null)
        {
            using var optsJs = opts?.ToJS();
            var arr = Native_FindRoute(fromRoomCoord.ToString(), toRoomCoord.ToString(), optsJs);
            if (arr == null)
            {
                steps = Enumerable.Empty<MapFindRouteStep>();
                return MapFindRouteResult.NoPath;
            }
            steps = arr.Select(x => x.ToMapFindRouteStep()).ToArray();
            return MapFindRouteResult.Ok;
        }

        public MapFindRouteResult FindRoute(IRoom fromRoom, IRoom toRoom, out IEnumerable<MapFindRouteStep> steps, MapFindRouteOptions? opts = null)
            => FindRoute(fromRoom.Coord, toRoom.Coord, out steps, opts);

        public int GetRoomLinearDistance(RoomCoord roomCoord1, RoomCoord roomCoord2, bool continuous = false)
            => Native_GetRoomLinearDistance(roomCoord1.ToString(), roomCoord2.ToString(), continuous);

        public IRoomTerrain GetRoomTerrain(RoomCoord roomCoord)
            => new NativeRoomTerrain(Native_GetRoomTerrain(roomCoord.ToString()));

        public int GetWorldSize()
            => Native_GetWorldSize();

        public RoomStatus GetRoomStatus(RoomCoord roomCoord)
        {
            using var obj = Native_GetRoomStatus(roomCoord.ToString());
            var timestamp = obj.TryGetPropertyAsDouble(Names.Timestamp) ?? 0.0;
            var status = obj.GetPropertyAsName(Names.Status);
            return new(status!.ParseRoomStatusType(), timestamp > 0.0 ? DateTime.UnixEpoch + TimeSpan.FromMilliseconds(timestamp) : null);
        }
    }
}
