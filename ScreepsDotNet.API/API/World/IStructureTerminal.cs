namespace ScreepsDotNet.API.World
{
    public enum TerminalSendResult
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
        /// The structure does not have the given amount of resources.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The arguments provided are incorrect.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// The terminal is still cooling down.
        /// </summary>
        Tired = -11
    }

    /// <summary>
    /// Sends any resources to a Terminal in another room.
    /// The destination Terminal can belong to any player.
    /// Each transaction requires additional energy (regardless of the transfer resource type) that can be calculated using Game.market.calcTransactionCost method.
    /// For example, sending 1000 mineral units from W0N0 to W10N5 will consume 742 energy units.
    /// You can track your incoming and outgoing transactions using the Game.market object.
    /// Only one Terminal per room is allowed that can be addressed by Room.terminal property.
    /// Terminals are used in the Market system.
    /// </summary>
    public interface IStructureTerminal : IOwnedStructure, IWithStore
    {
        /// <summary>
        /// The remaining amount of ticks while this terminal cannot be used to make StructureTerminal.send or Game.market.deal calls.
        /// </summary>
        int Cooldown { get; }

        /// <summary>
        /// Sends resource to a Terminal in another room with the specified name.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount">The amount of resources to be sent.</param>
        /// <param name="destination">The name of the target room. You don't have to gain visibility in this room.</param>
        /// <param name="description">The description of the transaction. It is visible to the recipient. The maximum length is 100 characters.</param>
        /// <returns></returns>
        TerminalSendResult Send(ResourceType resourceType, int amount, string destination, string? description = null);
    }
}
