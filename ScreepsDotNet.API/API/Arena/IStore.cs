using System.Collections.Generic;

namespace ScreepsDotNet.API.Arena
{
    public enum ResourceType
    {
        Energy,
        Score,
        ScoreX,
        ScoreY,
        ScoreZ,
        Unknown
    }

    /// <summary>
    /// An object that can contain resources in its cargo
    /// </summary>
    public interface IStore
    {
        /// <summary>
        /// Returns capacity of this store for the specified resource
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int? GetCapacity(ResourceType? resourceType = null);

        /// <summary>
        /// Returns the capacity used by the specified resource
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int? GetUsedCapacity(ResourceType? resourceType = null);

        /// <summary>
        /// Returns free capacity for the store
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int? GetFreeCapacity(ResourceType? resourceType = null);

        /// <summary>
        /// Gets all resource types contained within this store.
        /// </summary>
        IEnumerable<ResourceType> ContainedResourceTypes { get; }

        /// <summary>
        /// Gets or sets how much of each resource is in this store.
        /// Note that setting the resource amount will not affect GetUsedCapacity and GetFreeCapacity, and will not persist across ticks.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int this[ResourceType resourceType] { get; set; }
    }
}
