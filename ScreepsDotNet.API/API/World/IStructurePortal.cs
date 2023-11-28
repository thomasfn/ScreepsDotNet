namespace ScreepsDotNet.API.World
{
    public readonly struct PortalInterShardDestination
    {
        public readonly string Shard;
        public readonly RoomCoord Room;

        public PortalInterShardDestination(string shard, RoomCoord room)
        {
            Shard = shard;
            Room = room;
        }
    }

    /// <summary>
    /// A non-player structure.
    /// Instantly teleports your creeps to a distant room acting as a room exit tile.
    /// Portals appear randomly in the central room of each sector.
    /// </summary>
    public interface IStructurePortal : IStructure
    {
        /// <summary>
        /// If this is an inter-room portal, then this property contains a RoomPosition object leading to the point in the destination room.
        /// </summary>
        RoomPosition? InterRoomDestination { get; }

        /// <summary>
        /// If this is an inter-shard portal, then this property contains an object with shard and room string properties. Exact coordinates are undetermined, the creep will appear at any free spot in the destination room.
        /// </summary>
        PortalInterShardDestination? InterShardDestination { get; }

        /// <summary>
        /// The amount of game ticks when the portal disappears, or undefined when the portal is stable.
        /// </summary>
        int? TicksToDecay { get; }
    }
}
