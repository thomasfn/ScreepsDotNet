using System;
using System.Linq;

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
            foreach (var room in game.Rooms)
            {
                TickRoom(room);
            }
        }

        private void TickRoom(IRoom room)
        {
            var spawns = room.Find<IStructureSpawn>();
            Console.WriteLine($"{room} has spawns: {string.Join(", ", spawns.Select(x => x.ToString()))}");

            var spawn = spawns.FirstOrDefault();
            if (spawn != null)
            {
                var result = spawn.SpawnCreep(new BodyPartType[] { BodyPartType.Move, BodyPartType.Carry, BodyPartType.Work }, "mycreep1", new(dryRun: true));
                Console.WriteLine($"Tried to spawn creep (dry run), result is {result}");
                if (result == SpawnCreepResult.Ok)
                {
                    result = spawn.SpawnCreep(new BodyPartType[] { BodyPartType.Move, BodyPartType.Carry, BodyPartType.Work }, "mycreep1", new());
                    Console.WriteLine($"Tried to spawn creep, result is {result}");
                }
            }
        }
    }
}
