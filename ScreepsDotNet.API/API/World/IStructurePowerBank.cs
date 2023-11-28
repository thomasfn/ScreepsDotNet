namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Non-player structure.
    /// Contains power resource which can be obtained by destroying the structure.
    /// Hits the attacker creep back on each attack.
    /// </summary>
    public interface IStructurePowerBank : IStructure
    {
        /// <summary>
        /// The amount of power containing.
        /// </summary>
        int Power { get; }

        /// <summary>
        /// The amount of game ticks when this structure will disappear.
        /// </summary>
        int TicksToDecay { get; }
    }
}
