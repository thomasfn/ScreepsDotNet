namespace ScreepsDotNet.API.Bot
{
    /// <summary>
    /// Represents a bot that can be run in the Screeps game.
    /// </summary>
    public interface IBot
    {
        /// <summary>
        /// Called once per tick to perform the bot's logic.
        /// </summary>
        void Loop();
    }
}