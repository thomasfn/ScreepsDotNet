namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A nuke landing position. This object cannot be removed or modified.
    /// </summary>
    public interface INuke : IRoomObject, IWithId
    {
        /// <summary>
        /// The coord of the room where this nuke has been launched from.
        /// </summary>
        RoomCoord LaunchRoomCoord { get; }

        /// <summary>
        /// The remaining landing time.
        /// </summary>
        int TimeToLand { get; }
    }
}
