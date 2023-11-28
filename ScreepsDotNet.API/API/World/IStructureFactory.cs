namespace ScreepsDotNet.API.World
{
    public enum FactoryProduceResult
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
        /// The factory is not operated by the PWR_OPERATE_FACTORY power.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The structure does not have the required amount of resources.
        /// </summary>
        NotEnoughEnergy = -6,
        /// <summary>
        /// The factory cannot produce the commodity of this level.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The factory cannot contain the produce.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The arguments provided are incorrect.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// The factory is still cooling down.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// Room Controller Level insufficient to use the factory.
        /// </summary>
        RclNotEnough = -14
    }

    /// <summary>
    /// Produces trade commodities from base minerals and other commodities.
    /// </summary>
    public interface IStructureFactory : IOwnedStructure, IWithStore
    {
        /// <summary>
        /// The amount of game ticks the factory has to wait until the next production is possible.
        /// </summary>
        int Cooldown { get; }

        /// <summary>
        /// The factory's level. Can be set by applying the PWR_OPERATE_FACTORY power to a newly built factory. Once set, the level cannot be changed.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Produces the specified commodity. All ingredients should be available in the factory store.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        FactoryProduceResult Produce(ResourceType resourceType);
    }
}
