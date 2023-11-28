namespace ScreepsDotNet.API.World
{
    public enum RampartSetPublicResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this structure.
        /// </summary>
        NotOwner = -1
    }

    /// <summary>
    /// Blocks movement of hostile creeps, and defends your creeps and structures on the same tile. Can be used as a controllable gate.
    /// </summary>
    public interface IStructureRampart : IOwnedStructure
    {
        /// <summary>
        /// If false (default), only your creeps can step on the same square. If true, any hostile creeps can pass through.
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// The amount of game ticks when this rampart will lose some hit points.
        /// </summary>
        int TicksToDecay { get; }

        /// <summary>
        /// Make this rampart public to allow other players' creeps to pass through.
        /// </summary>
        /// <param name="isPublic"></param>
        RampartSetPublicResult SetPublic(bool isPublic);
    }
}
