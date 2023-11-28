namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Decreases movement cost to 1.
    /// Using roads allows creating creeps with less MOVE body parts.
    /// You can also build roads on top of natural terrain walls which are otherwise impassable.
    /// </summary>
    public interface IStructureRoad : IStructure
    {
        /// <summary>
        /// The amount of game ticks when this road will lose some hit points.
        /// </summary>
        int TicksToDecay { get; }
    }
}
