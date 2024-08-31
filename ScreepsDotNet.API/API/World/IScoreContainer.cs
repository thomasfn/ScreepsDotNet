namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Season 1 score container.
    /// </summary>
    public interface IScoreContainer : IRoomObject, IWithStore
    {
        int TicksToDecay { get; }
    }
}
