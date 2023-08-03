namespace ScreepsDotNet.API.World
{
    public interface IGame
    {
        /// <summary>
        /// An object containing information about your CPU usage.
        /// </summary>
        ICpu Cpu { get; }

        /// <summary>
        /// System game tick counter. It is automatically incremented on every tick.
        /// </summary>
        long Time { get; }

        /// <summary>
        /// Call once at the beginning of every game tick.
        /// Refreshes game state.
        /// </summary>
        void Tick();
    }
}
