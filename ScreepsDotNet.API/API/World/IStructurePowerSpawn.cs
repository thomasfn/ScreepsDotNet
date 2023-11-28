namespace ScreepsDotNet.API.World
{
    public enum PowerSpawnProcessPowerResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = -0,
        /// <summary>
        /// You are not the owner of this structure.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The structure does not have enough energy or power resource units.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// Room Controller Level insufficient to use this structure.
        /// </summary>
        RclNotEnough = -14
    }

    /// <summary>
    /// Processes power into your account, and spawns power creeps with special unique powers.
    /// </summary>
    public interface IStructurePowerSpawn : IOwnedStructure, IWithStore
    {
        /// <summary>
        /// Register power resource units into your account. Registered power allows to develop power creeps skills.
        /// </summary>
        PowerSpawnProcessPowerResult ProcessPower();
    }
}
