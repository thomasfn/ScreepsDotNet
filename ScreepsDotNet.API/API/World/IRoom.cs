using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ScreepsDotNet.API.World
{
    public enum RoomCreateConstructionSiteResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// The room is claimed or reserved by a hostile player.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The structure cannot be placed at the specified location.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// You have too many construction sites. The maximum number of construction sites per player is 100.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The location is incorrect.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// Room Controller Level insufficient.
        /// </summary>
        RclNotEnough = -14
    }

    public enum RoomCreateFlagResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// There is a flag with the same name already.
        /// </summary>
        NameExists = -3,
        /// <summary>
        /// You have too many flags. The maximum number of flags per player is 10000.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The location or the name or the color constant is incorrect.
        /// </summary>
        InvalidArgs = -10
    }

    public enum RoomFindExitResult
    {
        /// <summary>
        /// Exit at top of room.
        /// </summary>
        Top = 1,
        /// <summary>
        /// Exit to right of room.
        /// </summary>
        Right = 3,
        /// <summary>
        /// Exit at bottom of room.
        /// </summary>
        Bottom = 5,
        /// <summary>
        /// Exit to left of room.
        /// </summary>
        Left = 7,
        /// <summary>
        /// Path can not be found.
        /// </summary>
        NoPath = -2,
        /// <summary>
        /// The location is incorrect.
        /// </summary>
        InvalidArgs = -10
    }

    public static class RoomExitExtensions
    {
        public static ExitDirection? ToExitDirection(this RoomFindExitResult roomFindExitResult) => roomFindExitResult switch
        {
            RoomFindExitResult.Top => (ExitDirection?)ExitDirection.Top,
            RoomFindExitResult.Right => (ExitDirection?)ExitDirection.Right,
            RoomFindExitResult.Bottom => (ExitDirection?)ExitDirection.Bottom,
            RoomFindExitResult.Left => (ExitDirection?)ExitDirection.Left,
            _ => null,
        };
    }

    /// <summary>
    /// An object representing the room in which your units and structures are in.
    /// It can be used to look around, find paths, etc.
    /// Every RoomObject in the room contains its linked Room instance in the room property.
    /// </summary>
    public interface IRoom : IWithName
    {
        /// <summary>
        /// Gets if this room still exists and is visible.
        /// May return false if a reference to this object is held across ticks and the underlying object is no longer represented in the JS API.
        /// If this returns false, any attempts to access this object's properties or methods will throw an exception.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Gets the coordinate of this room within the world map.
        /// </summary>
        RoomCoord Coord { get; }

        /// <summary>
        /// The Controller structure of this room, if present, otherwise undefined.
        /// </summary>
        IStructureController? Controller { get; }

        /// <summary>
        /// Total amount of energy available in all spawns and extensions in the room.
        /// </summary>
        int EnergyAvailable { get; }

        /// <summary>
        /// Total amount of energyCapacity of all spawns and extensions in the room.
        /// </summary>
        int EnergyCapacityAvailable { get; }

        /// <summary>
        /// A shorthand to Memory.rooms[room.name]. You can use it for quick access the room’s specific memory data object.
        /// </summary>
        IMemoryObject Memory { get; }

        /// <summary>
        /// The Storage structure of this room, if present, otherwise undefined.
        /// </summary>
        IStructureStorage? Storage { get; }

        /// <summary>
        /// The Terminal structure of this room, if present, otherwise undefined.
        /// </summary>
        IStructureTerminal? Terminal { get; }

        /// <summary>
        /// A RoomVisual object for this room. You can use this object to draw simple shapes (lines, circles, text labels) in the room.
        /// </summary>
        IRoomVisual Visual { get; }

        /// <summary>
        /// Create new ConstructionSite at the specified location.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="position"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        RoomCreateConstructionSiteResult CreateConstructionSite<T>(Position position, string? name = null) where T : class, IStructure;

        /// <summary>
        /// Create new ConstructionSite at the specified location.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="structureType"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        RoomCreateConstructionSiteResult CreateConstructionSite(Position position, Type structureType, string? name = null);

        /// <summary>
        /// Create new Flag at the specified location.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="secondaryColor"></param>
        /// <returns></returns>
        RoomCreateFlagResult CreateFlag(Position position, out string newFlagName, string? name = null, FlagColor? color = null, FlagColor? secondaryColor = null);

        /// <summary>
        /// Find all objects of the specified type in the room.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> Find<T>() where T : class, IRoomObject;

        /// <summary>
        /// Find all objects of the specified type in the room.
        /// This overload filters the results on a specific condition, however only certain expressions are supported.
        /// The type T and the filter must be combinable in a way that results in a valid Screeps FIND constant.
        /// For example, <c>room.Find&lt;ICreep&gt;(x =&gt; x.My)</c> is valid as it can be mapped to FIND_MY_CREEPS, however <c>room.Find&lt;IStructure&gt;(x =&gt; x.Hits &lt; x.HitsMax)</c> is not valid as it can't be mapped to a FIND constant.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        // IEnumerable<T> Find<T>(Expression<Func<T, bool>> filter) where T : class, IRoomObject;

        /// <summary>
        /// Find all exits in the room.
        /// </summary>
        /// <param name="exitFilter">Filter by a specific exit direction</param>
        /// <returns></returns>
        IEnumerable<Position> FindExits(ExitDirection? exitFilter = null);

        /// <summary>
        /// Find the exit direction en route to another room. Please note that this method is not required for inter-room movement, you can simply pass the target in another room into Creep.moveTo method.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        RoomFindExitResult FindExitTo(IRoom room);

        /// <summary>
        /// Find the exit direction en route to another room. Please note that this method is not required for inter-room movement, you can simply pass the target in another room into Creep.moveTo method.
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        RoomFindExitResult FindExitTo(string roomName);

        /// <summary>
        /// Find an optimal path inside the room between fromPos and toPos using Jump Point Search algorithm.
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        IEnumerable<PathStep> FindPath(Position fromPos, Position toPos, FindPathOptions? opts = null);

        /// <summary>
        /// Returns an array of events happened on the previous tick in this room in JSON format.
        /// </summary>
        /// <returns></returns>
        string GetRawEventLog();

        /// <summary>
        /// Creates a RoomPosition object at the specified location.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        RoomPosition GetPositionAt(Position position);

        /// <summary>
        /// Get a Room.Terrain object which provides fast access to static terrain data. This method works for any room in the world even if you have no access to it.
        /// </summary>
        /// <returns></returns>
        IRoomTerrain GetTerrain();

        /// <summary>
        /// Get the list of objects at the specified room position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        IEnumerable<IRoomObject> LookAt(Position position);

        /// <summary>
        /// Get the list of objects at the specified room area.
        /// </summary>
        /// <param name="min">Inclusive minimum of the boundary of the area</param>
        /// <param name="max">Inclusive maximum of the boundary of the area</param>
        /// <returns></returns>
        IEnumerable<IRoomObject> LookAtArea(Position min, Position max);

        /// <summary>
        /// Get an object with the given type at the specified room position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="position"></param>
        /// <returns></returns>
        IEnumerable<T> LookForAt<T>(Position position) where T : class, IRoomObject;

        /// <summary>
        /// Get the list of objects with the given type at the specified room area.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        IEnumerable<T> LookForAtArea<T>(Position min, Position max) where T : class, IRoomObject;
    }
}
