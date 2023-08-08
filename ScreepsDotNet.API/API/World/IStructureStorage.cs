namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A structure that can store huge amount of resource units.
    /// Only one structure per room is allowed that can be addressed by Room.storage property.
    /// </summary>
    public interface IStructureStorage : IOwnedStructure, IWithStore
    {

    }
}
