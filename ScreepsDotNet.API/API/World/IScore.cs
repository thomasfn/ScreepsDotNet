using System;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A score object that appears randomly in rooms during Season 10. Move a creep onto the same tile to automatically collect it — the score value is credited to the creep's owner and the object disappears.
    /// </summary>
    public interface IScore : IRoomObject, IWithId
    {
        /// <summary>
        /// The score value that will be credited to the creep's owner upon collection.
        /// </summary>
        int Score { get; }

        /// <summary>
        /// The number of game ticks remaining before this object disappears.
        /// </summary>
        int TicksToDecay { get; }
    }
}
