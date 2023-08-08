namespace ScreepsDotNet.API.World
{
    public interface IWithId
    {
        /// <summary>
        /// A unique object identificator. You can use Game.getObjectById method to retrieve an object instance by its id.
        /// </summary>
        string Id { get; }
    }

    public interface IWithName
    {
        /// <summary>
        /// A unique name for the object, usually chosen when the object is created (e.g. creeps or certain structures). Cannot be changed later.
        /// </summary>
        string Name { get; }
    }

    public interface IWithStore
    {
        /// <summary>
        /// A Store object that contains cargo of this object.
        /// </summary>
        IStore Store { get; }
    }
}
