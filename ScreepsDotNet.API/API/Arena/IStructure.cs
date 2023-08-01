namespace ScreepsDotNet.API.Arena
{
    public interface IStructure : IGameObject
    {
        /// <summary>
        /// The current amount of hit points of the structure
        /// </summary>
        int Hits { get; }

        /// <summary>
        /// The maximum amount of hit points of the structure
        /// </summary>
        int HitsMax { get; }
    }
}
