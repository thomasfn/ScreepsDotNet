using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreepsDotNet.API
{
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
    public interface IGameObject : IPosition
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
        T? FindClosestByPath<T>(IEnumerable<T> positions, object? options) where T : class, IPosition;

        /// <summary>
        /// Find a position with the shortest path from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="positions"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Position? FindClosestByPath(IEnumerable<Position> positions, object? options);
    }
}
