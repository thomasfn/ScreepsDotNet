using System;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A remnant of dead creeps. This is a walkable object.
    /// </summary>
    public interface ITombstone : IRoomObject, IWithId, IWithStore
    {
        /// <summary>
        /// An object containing the deceased creep or power creep.
        /// </summary>
        ICreep? Creep { get; }

        /// <summary>
        /// Time of death.
        /// </summary>
        int DeathTime { get; }

        /// <summary>
        /// The amount of game ticks before this tombstone decays.
        /// </summary>
        int TicksToDecay {  get; }
    }
}
