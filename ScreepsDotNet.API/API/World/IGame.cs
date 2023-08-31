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

    public interface IGame
    {
        /// <summary>
        /// An object containing information about your CPU usage.
        /// </summary>
        ICpu Cpu { get; }

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
        /// A hash containing all the rooms available to you with room names as hash keys. A room is visible if you have a creep or an owned structure in it.
        /// </summary>
        IReadOnlyDictionary<string, IRoom> Rooms { get; }

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
    }
}
