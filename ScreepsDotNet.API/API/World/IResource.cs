namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// A dropped piece of resource. It will decay after a while if not picked up. Dropped resource pile decays for ceil(amount/1000) units per tick.
    /// </summary>
    public interface IResource : IRoomObject, IWithId
    {
        /// <summary>
        /// The amount of resource units containing.
        /// </summary>
        int Amount { get; }

        /// <summary>
        /// One of the RESOURCE_* constants.
        /// </summary>
        ResourceType ResourceType { get; }
    }
}
