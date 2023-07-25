namespace ScreepsDotNet.API
{
    public interface IUtils
    {
        /// <returns>the number of ticks passed from the start of the current game.</returns>
        int GetTicks();

        /// <returns>CPU wall time elapsed in the current tick in nanoseconds</returns>
        long GetCpuTime();
    }
}
