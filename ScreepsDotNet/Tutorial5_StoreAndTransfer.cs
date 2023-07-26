using System;
using System.Linq;

using ScreepsDotNet.API;

namespace ScreepsDotNet
{
    public class Tutorial5_StoreAndTransfer : ITutorialScript
    {
        private readonly IGame game;

        public Tutorial5_StoreAndTransfer(IGame game)
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

            var enemyCreeps = allCreeps.Where(x => !x.My);

            var container = game.Utils.GetObjectsByType<IStructureContainer>()
                .Where(x => x.My ?? true)
                .FirstOrDefault();
            if (container == null)
            {
                Console.WriteLine($"No containers found!");
                return;
            }

            var tower = game.Utils.GetObjectsByType<IStructureTower>()
                .Where(x => x.My ?? false)
                .FirstOrDefault();
            if (tower == null)
            {
                Console.WriteLine($"No towers found!");
                return;
            }

            foreach (var myCreep in myCreeps)
            {
                var energyStored = myCreep.Store[ResourceType.Energy];
                var energySpace = myCreep.Store.GetFreeCapacity(ResourceType.Energy) ?? 0;
                Console.WriteLine($"{myCreep} has {energyStored} energy stored and {energySpace} space for more");
                var energyAvailable = container.Store[ResourceType.Energy];
                Console.WriteLine($"{container} has {energyAvailable} energy available");
                
                // If we have space, load up until either the creep is full or the container is empty
                if (energySpace > 0 && energyAvailable > 0)
                {
                    var withdrawResult = myCreep.Withdraw(container, ResourceType.Energy, null);
                    if (withdrawResult == CreepTransferResult.Full || withdrawResult == CreepTransferResult.NotEnoughResources || withdrawResult == CreepTransferResult.Ok)
                    {
                        continue;
                    }
                    Console.WriteLine($"{myCreep}: Unexpected withdraw result {withdrawResult}");
                    continue;
                }
                
                // If we don't have space or the container is empty, unload to tower
                if (energyStored > 0)
                {
                    var transferResult = myCreep.Transfer(tower, ResourceType.Energy, null);
                    if (transferResult == CreepTransferResult.Full || transferResult == CreepTransferResult.NotEnoughResources || transferResult == CreepTransferResult.Ok)
                    {
                        continue;
                    }
                    Console.WriteLine($"{myCreep}: Unexpected transfer result {transferResult}");
                    continue;
                }
            }

            if (enemyCreeps.Any())
            {
                tower.Attack(enemyCreeps.First());
            }

        }
    }
}
