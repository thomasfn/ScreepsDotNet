using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ScreepsDotNet.API.Arena
{
    /// <summary>
    /// Thrown when accessing a property on an object that does not yet exist.
    /// </summary>
    [Serializable]
    public class NotSpawnedYetException : Exception
    {
        public NotSpawnedYetException() { }
        public NotSpawnedYetException(string message) : base(message) { }
        public NotSpawnedYetException(string message, Exception inner) : base(message, inner) { }
    }

    public interface IPosition
    {
        /// <summary>
        /// The X coordinate in the room
        /// </summary>
        int X { get; }

        /// <summary>
        /// The Y coordinate in the room
        /// </summary>
        int Y { get; }

        /// <summary>
        /// Gets the coordinate in the room
        /// </summary>
        Position Position { get; }
    }

    /// <summary>
    /// Basic prototype for game objects.
    /// All objects and classes are inherited from this class
    /// </summary>
    public interface IGameObject : IPosition, IWithUserData
    {
        /// <summary>
        /// True if this object is live in the game at the moment
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// The unique ID of this object that you can use in GetObjectById
        /// </summary>
        string Id { get; }

        /// <summary>
        /// If defined, then this object will disappear after this number of ticks
        /// </summary>
        int? TicksToDecay { get; }

        /// <summary>
        /// Find a position with the shortest path from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="positions"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        T? FindClosestByPath<T>(IEnumerable<T> positions, FindPathOptions? options) where T : class, IPosition;

        /// <summary>
        /// Find a position with the shortest path from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="positions"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Position? FindClosestByPath(IEnumerable<Position> positions, FindPathOptions? options);

        /// <summary>
        /// Find a position with the shortest linear distance from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="positions"></param>
        /// <returns></returns>
        T? FindClosestByRange<T>(IEnumerable<T> positions) where T : class, IPosition;

        /// <summary>
        /// Find a position with the shortest linear distance from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="positions"></param>
        /// <returns></returns>
        Position? FindClosestByRange(IEnumerable<Position> positions);

        /// <summary>
        /// Find all objects in the specified linear range
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="positions"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        IEnumerable<T> FindInRange<T>(IEnumerable<T> positions, int range) where T : class, IPosition;

        /// <summary>
        /// Find all objects in the specified linear range
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        IEnumerable<Position> FindInRange(IEnumerable<Position> positions, int range);

        /// <summary>
        /// Find a path from this object to the given position
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="options"></param>
        /// <returns>An empty array if no path was found, or an array of the positions along the path</returns>
        ImmutableArray<Position> FindPathTo(IPosition pos, FindPathOptions? options);

        /// <summary>
        /// Find a path from this object to the given position
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="options"></param>
        /// <returns>An empty array if no path was found, or an array of the positions along the path</returns>
        ImmutableArray<Position> FindPathTo(Position pos, FindPathOptions? options);

        /// <summary>
        /// Get linear range between this and target object
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        int GetRangeTo(IPosition pos);

        /// <summary>
        /// Get linear range between this and target object
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        int GetRangeTo(Position pos);
    }
}
