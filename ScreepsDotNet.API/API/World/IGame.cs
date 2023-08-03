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
        /// A hash containing all the rooms available to you with room names as hash keys. A room is visible if you have a creep or an owned structure in it.
        /// </summary>
        IEnumerable<IRoom> Rooms { get; }

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
    }
}
