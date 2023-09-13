namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// <para>
    /// This NPC structure is a control center of NPC Strongholds, and also rules all invaders in the sector.
    /// It spawns NPC defenders of the stronghold, refill towers, repairs structures.
    /// While it's alive, it will spawn invaders in all rooms in the same sector.
    /// It also contains some valuable resources inside, which you can loot from its ruin if you destroy the structure.
    /// </para>
    /// <para>
    /// An Invader Core has two lifetime stages: deploy stage and active stage.
    /// When it appears in a random room in the sector, it has ticksToDeploy property, public ramparts around it, and doesn't perform any actions.
    /// While in this stage it's invulnerable to attacks(has EFFECT_INVULNERABILITY enabled).
    /// When the ticksToDeploy timer is over, it spawns structures around it and starts spawning creeps, becomes vulnerable, and receives EFFECT_COLLAPSE_TIMER which will remove the stronghold when this timer is over.
    /// </para>
    /// <para>
    /// An active Invader Core spawns level-0 Invader Cores in neutral neighbor rooms inside the sector.
    /// These lesser Invader Cores are spawned near the room controller and don't perform any activity except reserving/attacking the controller.
    /// One Invader Core can spawn up to 42 lesser Cores during its lifetime.
    /// </para>
    /// </summary>
    public interface IStructureInvaderCore : IOwnedStructure
    {
        /// <summary>
        /// The level of the stronghold. The amount and quality of the loot depends on the level.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Shows the timer for a not yet deployed stronghold, undefined otherwise.
        /// </summary>
        int? TicksToDeploy { get; }

        /// <summary>
        /// If the core is in process of spawning a new creep, this object will contain a StructureSpawn.Spawning object, or null otherwise.
        /// </summary>
        ISpawning? Spawning { get; }
    }
}
