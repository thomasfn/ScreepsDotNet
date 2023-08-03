using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Arena
{
    public class Tutorial9_Construction : ITutorialScript
    {
        private readonly IGame game;

        public Tutorial9_Construction(IGame game)
        {
            this.game = game;
        }

        public void Loop()
        {
            var allCreeps = game.Utils.GetObjectsByType<ICreep>();

            var myCreeps = allCreeps.Where(x => x.My);
            if (!myCreeps.Any())
            {
                Console.WriteLine($"No friendly creeps found!");
                return;
            }

            var container = game.Utils.GetObjectsByType<IStructureContainer>()
                .Where(x => x.My ?? true)
                .FirstOrDefault();
            if (container == null)
            {
                Console.WriteLine($"No containers found!");
                return;
            }

            var constructionSite = game.Utils.GetObjectsByType<IConstructionSite>()
                .Where(x => x.My)
                .FirstOrDefault();
            if (constructionSite == null)
            {
                Console.WriteLine($"No construction site found, spawning at (50, 55)");
                var result = game.Utils.CreateConstructionSite<IStructureTower>((50, 55));
                if (result.Object != null)
                {
                    Console.WriteLine($"Created {result.Object})");
                    constructionSite = result.Object;
                }
                if (result.Error != null)
                {
                    Console.WriteLine($"Got unexpected error creating construction site {result.Error}");
                }
            }

            if (constructionSite == null) { return; }

            foreach (var myCreep in myCreeps)
            {
                var energyStored = myCreep.Store[ResourceType.Energy];
                var energySpace = myCreep.Store.GetFreeCapacity(ResourceType.Energy) ?? 0;
                var energyAvailable = container.Store[ResourceType.Energy];

                // If we have energy, go to the construction site and build it
                if (energyStored > 0)
                {
                    var buildResult = myCreep.Build(constructionSite);
                    if (buildResult == CreepBuildResult.NotInRange)
                    {
                        myCreep.MoveTo(constructionSite);

                    }
                    else if (buildResult != CreepBuildResult.Ok)
                    {
                        Console.WriteLine($"{myCreep}: Unexpected build result: {buildResult}");
                    }
                    continue;
                }

                // If there is energy available, go to the container and withdraw it
                if (energyAvailable > 0)
                {
                    var withdrawResult = myCreep.Withdraw(container, ResourceType.Energy);
                    if (withdrawResult == CreepTransferResult.NotInRange)
                    {
                        myCreep.MoveTo(container);

                    }
                    else if (withdrawResult != CreepTransferResult.Ok)
                    {
                        Console.WriteLine($"{myCreep}: Unexpected withdraw result: {withdrawResult}");
                    }
                    continue;
                }

            }

        }
    }
}
