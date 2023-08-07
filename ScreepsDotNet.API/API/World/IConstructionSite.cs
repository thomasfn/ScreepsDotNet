using System;

namespace ScreepsDotNet.API.World
{
    public enum ConstructionSiteRemoveResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this construction site, and it's not in your room.
        /// </summary>
        NotOwner = -1,
    }

    /// <summary>
    /// A site of a structure which is currently under construction.
    /// A construction site can be created using the 'Construct' button at the left of the game field or the Room.createConstructionSite method.
    /// To build a structure on the construction site, give a worker creep some amount of energy and perform Creep.build action.
    /// You can remove enemy construction sites by moving a creep on it.
    /// </summary>
    public interface IConstructionSite : IRoomObject
    {
        /// <summary>
        /// A unique object identificator. You can use Game.getObjectById method to retrieve an object instance by its id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Whether this is your own construction site.
        /// </summary>
        bool My { get; }

        /// <summary>
        /// An object with the structure’s owner info.
        /// </summary>
        OwnerInfo Owner { get; }

        /// <summary>
        /// The current construction progress.
        /// </summary>
        int Progress { get; }

        /// <summary>
        /// The total construction progress needed for the structure to be built.
        /// </summary>
        int ProgressTotal { get; }

        /// <summary>
        /// The type of structure being built.
        /// </summary>
        Type StructureType { get; }

        /// <summary>
        /// Gets if this construction site is for the specified type of structure.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool IsStructure<T>() where T : IStructure
            => StructureType.IsAssignableTo(typeof(T));

        /// <summary>
        /// Remove the construction site.
        /// </summary>
        void Remove();
    }
}
