using System;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.World
{
    public class BasicExample : ITutorialScript
    {
        private readonly IGame game;

        public BasicExample(IGame game)
        {
            this.game = game;
        }

        public void Loop()
        {
            Console.WriteLine($"Hello world from C#! The current tick is {game.Time} and your bucket contains {game.Cpu.Bucket} cpu. So far we used {game.Cpu.GetUsed()} cpu this tick.");
        }
    }
}
