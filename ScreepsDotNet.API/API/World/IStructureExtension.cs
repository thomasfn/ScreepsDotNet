namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Contains energy which can be spent on spawning bigger creeps.
    /// Extensions can be placed anywhere in the room, any spawns will be able to use them regardless of distance.
    /// </summary>
    public interface IStructureExtension : IOwnedStructure
    {
        /// <summary>
        /// A Store object that contains cargo of this structure.
        /// </summary>
        IStore Store { get; }
    }
}
