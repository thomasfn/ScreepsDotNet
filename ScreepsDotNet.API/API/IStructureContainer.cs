using System.ComponentModel;

namespace ScreepsDotNet.API
{
    /// <summary>
    /// A small container that can be used to store resources.
    /// This is a walkable structure.
    /// All dropped resources automatically goes to the container at the same tile.
    /// </summary>
    public interface IStructureContainer : IOwnedStructure
    {
        /// <summary>
        ///  A Store object that contains cargo of this structure.
        /// </summary>
        IStore Store { get; }
    }
}
