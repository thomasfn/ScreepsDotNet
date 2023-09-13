namespace ScreepsDotNet.API.World
{
    public enum NukerLaunchNukeResult
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
        /// The structure does not have enough energy and/or ghodium.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The nuke can't be launched to the specified RoomPosition.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target room is out of range.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The target is not a valid RoomPosition.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// This structure is still cooling down.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// Room Controller Level insufficient to use this structure.
        /// </summary>
        RclNotEnough = -14
    }

    /// <summary>
    /// Launches a nuke to another room dealing huge damage to the landing area.
    /// Each launch has a cooldown and requires energy and ghodium resources.
    /// Launching creates a Nuke object at the target room position which is visible to any player until it is landed.
    /// Incoming nuke cannot be moved or cancelled.
    /// Nukes cannot be launched from or to novice rooms.
    /// Resources placed into a StructureNuker cannot be withdrawn.
    /// </summary>
    public interface IStructureNuker : IOwnedStructure, IWithStore
    {
        /// <summary>
        /// The amount of game ticks until the next launch is possible.
        /// </summary>
        int Cooldown { get; }

        /// <summary>
        /// Launch a nuke to the specified position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        NukerLaunchNukeResult LaunchNuke(RoomPosition pos);
    }
}
