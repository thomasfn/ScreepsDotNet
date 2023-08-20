using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public readonly struct Effect
    {
        /// <summary>
        /// Effect ID of the applied effect. Can be either natural effect ID or Power ID.
        /// </summary>
        public readonly int EffectId;

        /// <summary>
        /// Power level of the applied effect. Absent if the effect is not a Power effect.
        /// </summary>
        public readonly int? Level;

        /// <summary>
        /// How many ticks will the effect last.
        /// </summary>
        public readonly int TicksRemaining;

        public Effect(int effectId, int? level, int ticksRemaining)
        {
            EffectId = effectId;
            Level = level;
            TicksRemaining = ticksRemaining;
        }
    }

    /// <summary>
    /// Any object with a position in a room. Almost all game objects prototypes are derived from RoomObject.
    /// </summary>
    public interface IRoomObject
    {
        /// <summary>
        /// Gets if this object still exists.
        /// May return false if a reference to this object is held across ticks and the underlying object is no longer represented in the JS API.
        /// If this returns false, any attempts to access this object's properties or methods will throw an exception.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Applied effects.
        /// </summary>
        IEnumerable<Effect> Effects { get; }

        /// <summary>
        /// An object representing the global position of this object.
        /// </summary>
        RoomPosition RoomPosition { get; }

        /// <summary>
        /// An object representing the local position of this object within the room.
        /// </summary>
        Position LocalPosition => RoomPosition.Position;

        /// <summary>
        /// The link to the Room object. May be undefined in case if an object is a flag or a construction site and is placed in a room that is not visible to you.
        /// </summary>
        IRoom? Room { get; }
    }
}
