namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A dropped piece of resource. It will decay after a while if not picked up. Dropped resource pile decays for ceil(amount/1000) units per tick.
    /// </summary>
    public interface IResource : IRoomObject
    {
        /// <summary>
        /// The amount of resource units containing.
        /// </summary>
        int Amount { get; }

        /// <summary>
        /// A unique object identificator. You can use Game.getObjectById method to retrieve an object instance by its id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// One of the RESOURCE_* constants.
        /// </summary>
        ResourceType ResourceType { get; }
    }
}
