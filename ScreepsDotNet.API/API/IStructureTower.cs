namespace ScreepsDotNet.API
{
    public enum TowerActionResult
    {
        Ok = 0,
        NotOwner = -1,
        Tired = -11,
        InvalidTarget = -7,
        NotEnoughEnergy = -6
    }

    /// <summary>
    /// Remotely attacks game objects or heals creeps within its range
    /// </summary>
    public interface IStructureTower : IOwnedStructure
    {
        /// <summary>
        ///  A Store object that contains cargo of this structure.
        /// </summary>
        IStore Store { get; }

        /// <summary>
        /// The remaining amount of ticks while this tower cannot be used
        /// </summary>
        int Cooldown { get; }

        /// <summary>
        /// Remotely attack any creep in range
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Attack(ICreep target);

        /// <summary>
        /// Remotely attack any structure in range
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Attack(IStructure target);

        /// <summary>
        /// Remotely heal any creep in range
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TowerActionResult Heal(ICreep target);
    }
}
