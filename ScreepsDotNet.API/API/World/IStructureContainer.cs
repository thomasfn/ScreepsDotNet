namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A small container that can be used to store resources.
    /// This is a walkable structure.
    /// All dropped resources automatically goes to the container at the same tile.
    /// </summary>
    public interface IStructureContainer : IOwnedStructure, IWithStore
    {
        /// <summary>
        /// The amount of game ticks when this container will lose some hit points.
        /// </summary>
        int TicksToDecay { get; }
    }
}
