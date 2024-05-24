namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A mineral deposit. Can be harvested by creeps with a WORK body part using the extractor structure.
    /// </summary>
    public interface IMineral : IRoomObject, IWithId
    {
        /// <summary>
        /// The density that this mineral deposit will be refilled to once ticksToRegeneration reaches 0. This is one of the DENSITY_* constants.
        /// </summary>
        int Density { get; }

        /// <summary>
        /// The remaining amount of resources.
        /// </summary>
        int MineralAmount { get; }

        /// <summary>
        /// The resource type, one of the RESOURCE_* constants.
        /// </summary>
        ResourceType MineralType { get; }

        /// <summary>
        /// The remaining time after which the deposit will be refilled.
        /// </summary>
        int? TicksToRegeneration { get; }
    }
}
