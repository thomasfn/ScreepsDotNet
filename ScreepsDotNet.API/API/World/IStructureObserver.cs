namespace ScreepsDotNet.API.World
{
    public enum ObserverObserveRoomResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this structure.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The target room is out of range.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The target room is not a valid room name value.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// Room Controller Level insufficient to use this structure.
        /// </summary>
        RclNotEnough = -14
    }

    /// <summary>
    /// Provides visibility into a distant room from your script.
    /// </summary>
    public interface IStructureObserver : IOwnedStructure
    {
        /// <summary>
        /// Provide visibility into a distant room from your script. The target room object will be available on the next tick.
        /// </summary>
        /// <param name="roomCoord"></param>
        /// <returns></returns>
        ObserverObserveRoomResult ObserveRoom(RoomCoord roomCoord);
    }
}
