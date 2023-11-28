namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A rare resource deposit needed for producing commodities.
    /// Can be harvested by creeps with a WORK body part.
    /// Each harvest operation triggers a cooldown period, which becomes longer and longer over time.
    /// </summary>
    public interface IDeposit : IRoomObject, IWithId
    {
        /// <summary>
        /// The amount of game ticks until the next harvest action is possible.
        /// </summary>
        int Cooldown { get; }

        /// <summary>
        /// The deposit type. One of ResourceType.Mist, ResourceType.Biomass, ResourceType.Metal or ResourceType.Silicon.
        /// </summary>
        ResourceType DepositType { get; }

        /// <summary>
        /// The cooldown of the last harvest operation on this deposit.
        /// </summary>
        int LastCooldown { get; }

        /// <summary>
        /// The amount of game ticks when this deposit will disappear.
        /// </summary>
        int TicksToDecay { get; }
    }
}
