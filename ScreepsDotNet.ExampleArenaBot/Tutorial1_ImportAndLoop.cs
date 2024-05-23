using System;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.ExampleArenaBot
{
    public class Tutorial1_LoopAndImport : IBot
    {
        private readonly IGame game;

        public Tutorial1_LoopAndImport(IGame game)
        {
            this.game = game;
        }

        public void Loop()
        {
            Console.WriteLine($"Current tick: {game.Utils.GetTicks()}");
        }
    }
}
