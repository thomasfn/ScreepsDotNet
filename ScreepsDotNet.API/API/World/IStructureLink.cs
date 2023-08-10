namespace ScreepsDotNet.API.World
{
    public enum LinkTransferEnergyResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this link.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The structure does not have the given amount of energy.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The target is not a valid StructureLink object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target cannot receive any more energy.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The energy amount is incorrect.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// The link is still cooling down.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// Room Controller Level insufficient to use this link.
        /// </summary>
        RclNotEnough = -14
    }

    /// <summary>
    /// Remotely transfers energy to another Link in the same room.
    /// </summary>
    public interface IStructureLink : IOwnedStructure, IWithStore
    {
        /// <summary>
        /// The amount of game ticks the link has to wait until the next transfer is possible.
        /// </summary>
        int Cooldown { get; }

        /// <summary>
        /// Remotely transfer energy to another link at any location in the same room.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="amount">The amount of energy to be transferred. If omitted, all the available energy is used.</param>
        /// <returns></returns>
        LinkTransferEnergyResult TransferEnergy(IStructureLink target, int? amount = null);
    }
}
