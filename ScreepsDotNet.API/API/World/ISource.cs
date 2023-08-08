using System;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// An energy source object. Can be harvested by creeps with a WORK body part.
    /// </summary>
    public interface ISource : IRoomObject, IWithId
    {
        /// <summary>
        /// The remaining amount of energy.
        /// </summary>
        int Energy { get; }

        /// <summary>
        /// The total amount of energy in the source.
        /// </summary>
        int EnergyCapacity { get; }

        /// <summary>
        /// The remaining time after which the source will be refilled.
        /// </summary>
        int TicksToRegeneration {  get; }
    }
}
