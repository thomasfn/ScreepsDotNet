namespace ScreepsDotNet.API.World
{
    public enum TowerActionResult
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
        /// The tower does not have enough energy.
        /// </summary>
        NotEnoughEnergy = -6,
        /// <summary>
        /// The target is not a valid attackable object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// Room Controller Level insufficient to use this structure.
        /// </summary>
        RclNotEnough = -14
    }

    /// <summary>
    /// Remotely attacks or heals creeps, or repairs structures.
    /// Can be targeted to any object in the room. However, its effectiveness linearly depends on the distance.
    /// Each action consumes energy.
    /// </summary>
    public interface IStructureTower : IOwnedStructure, IWithStore
    {
        /// <summary>
        /// Remotely attack any creep, power creep or structure in the room.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Attack(ICreep target);

        /// <summary>
        /// Remotely attack any power creep in the room.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Attack(IPowerCreep target);

        /// <summary>
        /// Remotely attack any structure in the room.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Attack(IStructure target);

        /// <summary>
        /// Remotely heal any creep in the room.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Heal(ICreep target);

        /// <summary>
        /// Remotely heal any power creep in the room.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Heal(IPowerCreep target);

        /// <summary>
        /// Remotely repair any structure in the room.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Repair(IStructure target);   
    }
}
