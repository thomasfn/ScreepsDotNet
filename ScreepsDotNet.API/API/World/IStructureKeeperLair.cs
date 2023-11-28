namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Non-player structure.
    /// Spawns NPC Source Keepers that guards energy sources and minerals in some rooms.
    /// This structure cannot be destroyed.
    /// </summary>
    public interface IStructureKeeperLair : IOwnedStructure
    {
        /// <summary>
        /// Time to spawning of the next Source Keeper.
        /// </summary>
        int TicksToSpawn { get; }
    }
}
