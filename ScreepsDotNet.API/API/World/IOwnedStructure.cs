using System;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// The base prototype for a structure that has an owner.
    /// </summary>
    public interface IOwnedStructure : IStructure
    {
        /// <summary>
        /// Whether this is your own structure.
        /// </summary>
        bool My { get; }

        /// <summary>
        /// An object with the structure’s owner info.
        /// </summary>
        OwnerInfo Owner { get; }
    }
}
