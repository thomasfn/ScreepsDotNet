using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum ExitDirection
    {
        Top = 1,
        Right = 3,
        Bottom = 5,
        Left = 7
    }

    public readonly struct RoomExits
    {
        private readonly string? top;
        private readonly string? right;
        private readonly string? bottom;
        private readonly string? left;

        public string? this[Direction direction] => direction switch
        {
            Direction.Top => top,
            Direction.Right => right,
            Direction.Bottom => bottom,
            Direction.Left => left,
            _ => null,
        };

        public string? this[ExitDirection direction] => direction switch
        {
            ExitDirection.Top => top,
            ExitDirection.Right => right,
            ExitDirection.Bottom => bottom,
            ExitDirection.Left => left,
            _ => null,
        };

        public RoomExits(string? top, string? right, string? bottom, string? left)
        {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }

        public override string ToString()
            => $"RoomExits[Top='{top}', Right='{right}', Bottom='{bottom}', Left='{left}']";
    }

    public enum MapFindExitResult
    {
        /// <summary>
        /// Success
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Path can not be found.
        /// </summary>
        NoPath = -2,
        /// <summary>
        /// The location is incorrect.
        /// </summary>
        InvalidArgs = -10
    }

    public enum MapFindRouteResult
    {
        /// <summary>
        /// Success
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Path can not be found.
        /// </summary>
        NoPath = -2
    }

    public readonly struct MapFindRouteOptions
    {
        /// <summary>
        /// This callback accepts two arguments: function(roomName, fromRoomName).
        /// It can be used to calculate the cost of entering that room.
        /// You can use this to do things like prioritize your own rooms, or avoid some rooms.
        /// You can return a floating point cost or Infinity to block the room.
        /// </summary>
        public readonly Func<string, string, double>? RouteCallback;

        public MapFindRouteOptions(Func<string, string, double>? routeCallback = null)
        {
            RouteCallback = routeCallback;
        }
    }

    public readonly struct MapFindRouteStep
    {
        public readonly ExitDirection Exit;
        public readonly string RoomName;

        public MapFindRouteStep(ExitDirection exit, string roomName)
        {
            Exit = exit;
            RoomName = roomName;
        }
    }

    public enum RoomStatusType
    {
        /// <summary>
        /// The room has no restrictions
        /// </summary>
        Normal,
        /// <summary>
        /// The room is not available
        /// </summary>
        Closed,
        /// <summary>
        /// The room is part of a novice area
        /// </summary>
        Novice,
        /// <summary>
        /// The room is part of a respawn area
        /// </summary>
        Respawn
    }

    public readonly struct RoomStatus
    {
        /// <summary>
        /// Room status type
        /// </summary>
        public readonly RoomStatusType Status;
        /// <summary>
        /// Status expiration time in milliseconds since UNIX epoch time. This property is null if the status is permanent.
        /// </summary>
        public readonly DateTime? Timestamp;

        public RoomStatus(RoomStatusType status, DateTime? timestamp)
        {
            Status = status;
            Timestamp = timestamp;
        }
    }

    public interface IMap
    {
        // IMapVisual Visual { get; }

        /// <summary>
        /// List all exits available from the room with the given name.
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        RoomExits DescribeExits(string roomName);

        /// <summary>
        /// Find the exit direction from the given room en route to another room.
        /// </summary>
        /// <param name="fromRoomName"></param>
        /// <param name="toRoomName"></param>
        /// <param name="exitDirection"></param>
        /// <returns></returns>
        MapFindExitResult FindExit(string fromRoomName, string toRoomName, out ExitDirection exitDirection, MapFindRouteOptions? opts = null);

        /// <summary>
        /// Find the exit direction from the given room en route to another room.
        /// </summary>
        /// <param name="fromRoom"></param>
        /// <param name="toRoom"></param>
        /// <param name="exitDirection"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        MapFindExitResult FindExit(IRoom fromRoom, IRoom toRoom, out ExitDirection exitDirection, MapFindRouteOptions? opts = null);

        /// <summary>
        /// Find route from the given room to another room.
        /// </summary>
        /// <param name="fromRoomName"></param>
        /// <param name="toRoomName"></param>
        /// <param name="steps"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        MapFindRouteResult FindRoute(string fromRoomName, string toRoomName, out IEnumerable<MapFindRouteStep> steps, MapFindRouteOptions? opts = null);

        /// <summary>
        /// Find route from the given room to another room.
        /// </summary>
        /// <param name="fromRoom"></param>
        /// <param name="toRoom"></param>
        /// <param name="steps"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        MapFindRouteResult FindRoute(IRoom fromRoom, IRoom toRoom, out IEnumerable<MapFindRouteStep> steps, MapFindRouteOptions? opts = null);

        /// <summary>
        /// Get the linear distance (in rooms) between two rooms.
        /// You can use this function to estimate the energy cost of sending resources through terminals, or using observers and nukes.
        /// </summary>
        /// <param name="roomName1"></param>
        /// <param name="roomName2"></param>
        /// <param name="continuous"></param>
        /// <returns></returns>
        int GetRoomLinearDistance(string roomName1, string roomName2, bool continuous = false);

        /// <summary>
        /// Get a Room.Terrain object which provides fast access to static terrain data.
        /// This method works for any room in the world even if you have no access to it.
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        IRoomTerrain GetRoomTerrain(string roomName);

        /// <summary>
        /// Returns the world size as a number of rooms between world corners.
        /// For example, for a world with rooms from W50N50 to E50S50 this method will return 102.
        /// </summary>
        /// <returns></returns>
        int GetWorldSize();

        /// <summary>
        /// Gets availablity status of the room with the specified name.
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        RoomStatus GetRoomStatus(string roomName);
    }
}
