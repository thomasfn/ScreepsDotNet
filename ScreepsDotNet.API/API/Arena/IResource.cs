using ScreepsDotNet.API.World;

namespace ScreepsDotNet.API.Arena
{
    /// <summary>
    /// A dropped piece of resource. Dropped resource pile decays for ceil(amount/1000) units per tick
    /// </summary>
    public interface IResource : IGameObject
    {
        /// <summary>
        /// The amount of dropped resource
        /// </summary>
        int Amount { get; }

        /// <summary>
        /// The type of dropped resource
        /// </summary>
        ResourceType ResourceType { get; }
    }
}
