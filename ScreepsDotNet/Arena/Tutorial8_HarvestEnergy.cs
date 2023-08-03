using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Arena
{
    public class Tutorial8_HarvestEnergy : ITutorialScript
    {
        private readonly IGame game;

        public Tutorial8_HarvestEnergy(IGame game)
        {
            this.game = game;
        }

        public void Loop()
        {
            var spawn = game.Utils.GetObjectsByType<IStructureSpawn>()
                .Where(x => x.My ?? false)
                .FirstOrDefault();
            if (spawn == null)
            {
                Console.WriteLine($"No spawn found!");
                return;
            }

            var source = game.Utils.GetObjectsByType<ISource>()
                .FirstOrDefault();
            if (source == null)
            {
                Console.WriteLine($"No source found!");
                return;
            }

            var allCreeps = game.Utils.GetObjectsByType<ICreep>();

            var myCreeps = allCreeps.Where(x => x.My);
            if (!myCreeps.Any())
            {
                Console.WriteLine($"No friendly creeps found!");
                return;
            }

            foreach (var creep in myCreeps)
            {
                // If the creep has space and the source has more energy to harvest, move to source and harvest
                if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0 && source.Energy > 0)
                {
                    var harvestResult = creep.Harvest(source);
                    if (harvestResult == CreepHarvestResult.NotInRange)
                    {
                        creep.MoveTo(source);
                    }
                    else if (harvestResult != CreepHarvestResult.Ok)
                    {
                        Console.WriteLine($"{creep}: Unexpected harvest result for {source} {harvestResult}");
                    }
                    continue;
                }

                // If the creep has energy, move to spawn and deposit it
                if (creep.Store[ResourceType.Energy] > 0)
                {
                    var transferResult = creep.Transfer(spawn, ResourceType.Energy);
                    if (transferResult == CreepTransferResult.NotInRange)
                    {
                        creep.MoveTo(spawn);
                    }
                    else if (transferResult != CreepTransferResult.Ok)
                    {
                        Console.WriteLine($"{creep}: Unexpected transfer result for {spawn} {transferResult}");
                    }
                }
            }

        }
    }
}
