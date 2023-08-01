namespace ScreepsDotNet.API.Arena
{
    public interface IGame
    {
        IUtils Utils { get; }

        IPathFinder PathFinder { get; }

        IConstants Constants { get; }
    }
}
