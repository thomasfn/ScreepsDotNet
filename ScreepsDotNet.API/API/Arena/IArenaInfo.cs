namespace ScreepsDotNet.API.Arena
{
    public interface IArenaInfo
    {
        /// <summary>
        /// The name of the arena.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Currently equals to 1 for basic arena and 2 for advanced.
        /// </summary>
        ArenaLevel Level { get; }

        /// <summary>
        /// The name of the season this arena belongs.
        /// </summary>
        string Season { get; }

        /// <summary>
        /// Game ticks limit.
        /// </summary>
        int TicksLimit { get; }

        /// <summary>
        /// CPU wall time execution limit per one tick (except the first tick).
        /// </summary>
        int CpuTimeLimit { get; }

        /// <summary>
        /// CPU wall time limit on the first tick.
        /// </summary>
        int CpuTimeLimitFirstTick { get; }
    }

    public enum ArenaLevel
    {
        /// <summary>
        /// Basic arena level.
        /// </summary>
        Basic = 1,

        /// <summary>
        /// Advanced arena level.
        /// </summary>
        Advanced = 2,
    }
}
