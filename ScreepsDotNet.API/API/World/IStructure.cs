namespace ScreepsDotNet.API.World
{
    public enum StructureDestroyResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this structure, and it's not in your room.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// Hostile creeps are in the room.
        /// </summary>
        Busy = -4
    }

    public enum StructureNotifyWhenAttackedResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this structure.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// enable argument is not a boolean value.
        /// </summary>
        InvalidArgs = -10
    }

    public interface IStructure : IRoomObject
    {
        /// <summary>
        /// The current amount of hit points of the structure.
        /// </summary>
        int Hits { get; }

        /// <summary>
        /// The maximum amount of hit points of the structure.
        /// </summary>
        int HitsMax { get; }

        /// <summary>
        /// A unique object identificator. You can use Game.getObjectById method to retrieve an object instance by its id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Destroy this structure immediately.
        /// </summary>
        StructureDestroyResult Destroy();

        /// <summary>
        /// Check whether this structure can be used. If room controller level is insufficient, then this method will return false, and the structure will be highlighted with red in the game.
        /// </summary>
        bool IsActive();

        /// <summary>
        /// Toggle auto notification when the structure is under attack. The notification will be sent to your account email. Turned on by default.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        StructureNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled);
    }
}
