namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Blocks movement of all creeps. Players can build destructible walls in controlled rooms.
    /// Some rooms also contain indestructible walls separating novice and respawn areas from the rest of the world or dividing novice / respawn areas into smaller sections.
    /// Indestructible walls have no hits property.
    /// </summary>
    public interface IStructureWall : IStructure
    {
        
    }
}
