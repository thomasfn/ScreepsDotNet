namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Allows to harvest a mineral deposit.
    /// </summary>
    public interface IStructureExtractor : IOwnedStructure
    {
        /// <summary>
        /// The amount of game ticks until the next harvest action is possible.
        /// </summary>
        int Cooldown { get; }
    }
}
