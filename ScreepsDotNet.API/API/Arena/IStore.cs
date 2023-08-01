namespace ScreepsDotNet.API.Arena
{
    public enum ResourceType
    {
        Energy
    }

    /// <summary>
    /// An object that class contain resources in its cargo
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
        /// Gets how much of each resource is in this store
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int this[ResourceType resourceType] { get; }
    }
}
