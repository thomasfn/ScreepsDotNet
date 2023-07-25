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
        bool Exists { get; }
        string Id { get; }
        int? TicksToDecay { get; }

        T? FindClosestByPath<T>(IEnumerable<T> positions, object? options) where T : IPosition;
    }
}
