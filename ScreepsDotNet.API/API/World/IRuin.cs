using System;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A destroyed structure. This is a walkable object.
    /// </summary>
    public interface IRuin : IRoomObject, IWithId, IWithStore
    {
        /// <summary>
        /// The time when the structure has been destroyed.
        /// </summary>
        long DestroyTime { get; }

        /// <summary>
        /// An object containing basic data of the destroyed structure.
        /// </summary>
        IStructure? Structure { get; }

        /// <summary>
        /// The amount of game ticks before this ruin decays.
        /// </summary>
        int TicksToDecay { get; }
    }
}
