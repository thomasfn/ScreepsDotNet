namespace ScreepsDotNet.API
{
    /// <summary>
    /// An energy source object. Can be harvested by creeps with a WORK body part
    /// </summary>
    public interface ISource : IGameObject
    {
        /// <summary>
        /// Current amount of energy in the source
        /// </summary>
        int Energy { get; }

        /// <summary>
        /// The maximum amount of energy in the source
        /// </summary>
        int EnergyCapacity { get; }
    }
}
