using System;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// An energy source object. Can be harvested by creeps with a WORK body part.
    /// </summary>
    public interface ISource : IRoomObject
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
        /// A unique object identificator. You can use Game.getObjectById method to retrieve an object instance by its id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The remaining time after which the source will be refilled.
        /// </summary>
        int TicksToRegeneration {  get; }
    }
}
