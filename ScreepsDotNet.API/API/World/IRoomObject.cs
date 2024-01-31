using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum EffectType
    {
        PowerGenerateOps = 1,
        PowerOperateSpawn = 2,
        PowerOperateTower = 3,
        PowerOperateStorage = 4,
        PowerOperateLab = 5,
        PowerOperateExtension = 6,
        PowerOperateObserver = 7,
        PowerOperateTerminal = 8,
        PowerDisruptSpawn = 9,
        PowerDisruptTower = 10,
        PowerDisruptSource = 11,
        PowerShield = 12,
        PowerRegen_source = 13,
        PowerRegen_mineral = 14,
        PowerDisrupt_terminal = 15,
        PowerOperate_power = 16,
        PowerFortify = 17,
        PowerOperate_controller = 18,
        PowerOperate_factory = 19,
        Invulnerability = 1001,
        CollapseTimer = 1002,
    }

    public readonly struct Effect
    {
        /// <summary>
        /// Effect ID of the applied effect. Can be either natural effect ID or Power ID.
        /// </summary>
        public readonly EffectType EffectType;

        /// <summary>
        /// Power level of the applied effect. Absent if the effect is not a Power effect.
        /// </summary>
        public readonly int? Level;

        /// <summary>
        /// How many ticks will the effect last.
        /// </summary>
        public readonly int TicksRemaining;

        public Effect(EffectType effectType, int? level, int ticksRemaining)
        {
            EffectType = effectType;
            Level = level;
            TicksRemaining = ticksRemaining;
        }
    }

    /// <summary>
    /// Any object with a position in a room. Almost all game objects prototypes are derived from RoomObject.
    /// </summary>
    public interface IRoomObject : IWithUserData
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
