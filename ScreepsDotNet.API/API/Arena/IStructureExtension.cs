namespace ScreepsDotNet.API.Arena
{
    /// <summary>
    /// Contains energy that can be spent on spawning bigger creeps. Extensions can be placed anywhere, any spawns will be able to use them regardless of distance.
    /// </summary>
    public interface IStructureExtension : IOwnedStructure
    {
        /// <summary>
        /// A Store object that contains cargo of this structure.
        /// </summary>
        IStore Store { get; }
    }
}
