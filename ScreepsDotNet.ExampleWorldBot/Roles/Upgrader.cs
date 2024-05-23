using System;
using System.Collections.Generic;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.ExampleWorldBot.Roles
{
    /// <summary>
    /// The upgrader role will instruct creeps to collect energy from the spawn, then upgrade the room controller.
    /// Spawns will be cached to the heap for efficiency.
    /// </summary>
    /// <param name="room"></param>
    public class Upgrader(IRoom room) : IRole
    {
        private readonly IRoom room = room;

        private readonly List<IStructureSpawn> cachedSpawns = [];

        public void Run(ICreep creep)
        {
            // Check energy storage
            if (creep.Store[ResourceType.Energy] > 0)
            {
                // There is energy to use - upgrade the controller
                var controller = room.Controller;
                if (controller == null) { return; }
                var upgradeResult = creep.UpgradeController(controller);
                if (upgradeResult == CreepUpgradeControllerResult.NotInRange)
                {
                    creep.MoveTo(controller.RoomPosition);
                }
                else if (upgradeResult != CreepUpgradeControllerResult.Ok)
                {
                    Console.WriteLine($"{this}: {creep} unexpected result when upgrading {controller} ({upgradeResult})");
                }
            }
            else
            {
                // We're empty - go to pick up
                var spawn = FindNearestSpawn(creep.LocalPosition);
                if (spawn == null) { return; }
                var withdrawResult = creep.Withdraw(spawn, ResourceType.Energy);
                if (withdrawResult == CreepWithdrawResult.NotInRange)
                {
                    creep.MoveTo(spawn.RoomPosition);
                }
                else if (withdrawResult != CreepWithdrawResult.Ok)
                {
                    Console.WriteLine($"{this}: {creep} unexpected result when withdrawing from {spawn} ({withdrawResult})");
                }
            }
        }

        private IStructureSpawn? FindNearestSpawn(Position pos)
        {
            // If there are no spawns in the cache, assume the cache has not yet been built, so populate it
            if (cachedSpawns.Count == 0)
            {
                foreach (var spawn in room.Find<IStructureSpawn>())
                {
                    cachedSpawns.Add(spawn);
                }
            }

            // Now find the one closest to the position (don't forget to check that it wasn't destroyed - we might want to do some cleanup of the cache if this happens)
            return cachedSpawns
                .Where(static x => x.Exists)
                .MinBy(x => x.LocalPosition.LinearDistanceTo(pos));
        }
    }
}
