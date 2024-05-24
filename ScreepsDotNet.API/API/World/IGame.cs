using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Thrown when attempting to use an object retrieved from a previous tick that no longer exists.
    /// </summary>
    [Serializable]
    public class NativeObjectNoLongerExistsException : Exception
    {
        public NativeObjectNoLongerExistsException() { }
        public NativeObjectNoLongerExistsException(string message) : base(message) { }
        public NativeObjectNoLongerExistsException(string message, Exception inner) : base(message, inner) { }
        protected NativeObjectNoLongerExistsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public readonly struct ShardInfo
    {
        /// <summary>
        /// The name of the shard.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Currently always equals to normal.
        /// </summary>
        public readonly string Type;
        /// <summary>
        /// Whether this shard belongs to the PTR.
        /// </summary>
        public readonly bool PTR;

        public ShardInfo(string name, string type, bool ptr)
        {
            Name = name;
            Type = type;
            PTR = ptr;
        }
    }

    public enum PowerCreepCreateResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// A power creep with the specified name already exists.
        /// </summary>
        NameExists = -3,
        /// <summary>
        /// You don't have free Power Levels in your account.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The provided power creep name is exceeds the limit, or the power creep class is invalid.
        /// </summary>
        InvalidArgs = -10,
    }

    public interface IGame
    {
        /// <summary>
        /// An object containing information about your CPU usage.
        /// </summary>
        ICpu Cpu { get; }

        /// <summary>
        /// <para>
        /// InterShardMemory object provides an interface for communicating between shards.
        /// Your script is executed separatedly on each shard, and their Memory objects are isolated from each other.
        /// In order to pass messages and data between shards, you need to use InterShardMemory instead.
        /// </para>
        /// <para>
        /// Every shard can have its own 100 KB of data in string format that can be accessed by all other shards.
        /// A shard can write only to its own data, other shards' data is read-only.
        /// </para>
        /// <para>
        /// This data has nothing to do with Memory contents, it's a separate data container.
        /// </para>
        /// </summary>
        IInterShardMemory InterShardMemory { get; }

        /// <summary>
        /// A global object representing world map. Use it to navigate between rooms.
        /// </summary>
        IMap Map { get; }

        /// <summary>
        /// A global object representing the in-game market. You can use this object to track resource transactions to/from your terminals, and your buy/sell orders.
        /// </summary>
        IMarket Market { get; }

        /// <summary>
        /// A global object providing path finding functionality.
        /// </summary>
        IPathFinder PathFinder { get; }

        /// <summary>
        /// A global object containing game constants.
        /// </summary>
        IConstants Constants { get; }

        /// <summary>
        /// A global object containing memory that persists between ticks.
        /// </summary>
        IMemoryObject Memory { get; }

        /// <summary>
        /// A global object providing raw memory functionality.
        /// </summary>
        IRawMemory RawMemory { get; }

        /// <summary>
        /// A hash containing all your creeps with creep names as hash keys.
        /// </summary>
        IReadOnlyDictionary<string, ICreep> Creeps { get; }

        /// <summary>
        /// A hash containing all your flags with flag names as hash keys.
        /// </summary>
        IReadOnlyDictionary<string, IFlag> Flags { get; }

        /// <summary>
        /// A hash containing all your power creeps with their names as hash keys. Even power creeps not spawned in the world can be accessed here.
        /// </summary>
        IReadOnlyDictionary<string, IPowerCreep> PowerCreeps { get; }

        /// <summary>
        /// A hash containing all the rooms available to you with room names as hash keys. A room is visible if you have a creep or an owned structure in it.
        /// </summary>
        IReadOnlyDictionary<string, IRoom> Rooms { get; }

        /// <summary>
        /// An object describing the world shard where your script is currently being executed in.
        /// </summary>
        ShardInfo Shard { get; }

        /// <summary>
        /// A hash containing all your spawns with spawn names as hash keys.
        /// </summary>
        IReadOnlyDictionary<string, IStructureSpawn> Spawns { get; }

        /// <summary>
        /// A hash containing all your structures with structure id as hash keys.
        /// </summary>
        IReadOnlyDictionary<string, IStructure> Structures { get; }

        /// <summary>
        /// System game tick counter. It is automatically incremented on every tick.
        /// </summary>
        long Time { get; }

        /// <summary>
        /// Call once at the beginning of every game tick.
        /// Refreshes game state.
        /// </summary>
        void Tick();

        /// <summary>
        /// Get an object with the specified unique ID. It may be a game object of any type. Only objects from the rooms which are visible to you can be accessed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T? GetObjectById<T>(string id) where T : class, IRoomObject;

        /// <summary>
        /// Get an object with the specified unique ID. It may be a game object of any type. Only objects from the rooms which are visible to you can be accessed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T? GetObjectById<T>(ObjectId id) where T : class, IRoomObject;

        /// <summary>
        /// Send a custom message at your profile email.
        /// This way, you can set up notifications to yourself on any occasion within the game.
        /// You can schedule up to 20 notifications during one game tick. Not available in the Simulation Room.
        /// </summary>
        /// <param name="message">Custom text which will be sent in the message. Maximum length is 1000 characters.</param>
        /// <param name="groupInterval">If set to 0 (default), the notification will be scheduled immediately. Otherwise, it will be grouped with other notifications and mailed out later using the specified time in minutes.</param>
        void Notify(string message, int groupInterval = 0);

        /// <summary>
        /// Create a new empty cost matrix.
        /// </summary>
        /// <returns></returns>
        ICostMatrix CreateCostMatrix();

        /// <summary>
        /// Create a new room visual. If the room name is not provided, the visual will be posted to all rooms simultaneously.
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        IRoomVisual CreateRoomVisual(string? roomName = null);

        /// <summary>
        /// Create a new memory object. This can be populated and passed to IStructureSpawn.SpawnCreep via SpawnCreepOptions.
        /// </summary>
        /// <returns></returns>
        IMemoryObject CreateMemoryObject();

        /// <summary>
        /// Efficiently "renew" a set of room objects. This will refresh any stale references to the underlying js objects while minimising interop calls.
        /// This is not required for normal usage of any room objects, but if done strategically can reduce the overall cpu usage due to js interop.
        /// </summary>
        /// <param name="roomObjects"></param>
        void BatchRenewObjects(IEnumerable<IRoomObject> roomObjects);

        /// <summary>
        /// Efficiently fetch the room positions for a set of room objects. This is more efficient than invidually fetching each room position as it's done in a single interop call.
        /// </summary>
        /// <param name="roomObjects"></param>
        /// <param name="outRoomPositions"></param>
        // void BatchFetchRoomPositions(IEnumerable<IRoomObject> roomObjects, Span<RoomPosition> outRoomPositions);

        /// <summary>
        /// A static method to create new Power Creep instance in your account.
        /// It will be added in an unspawned state, use spawn method to spawn it in the world.
        /// You need one free Power Level in your account to perform this action.
        /// </summary>
        /// <param name="name">The name of the new power creep. The name length limit is 100 characters.</param>
        /// <param name="class">The class of the new power creep.</param>
        PowerCreepCreateResult CreatePowerCreep(string name, PowerCreepClass @class);
    }
}
