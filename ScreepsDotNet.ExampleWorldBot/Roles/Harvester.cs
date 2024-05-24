using System;
using System.Collections.Generic;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.ExampleWorldBot.Roles
{
    /// <summary>
    /// The harvester role will instruct creeps to harvest the nearest source until they're full, then return to the nearest spawn and deposit energy.
    /// Sources and spawns will be cached to the heap for efficiency.
    /// </summary>
    /// <param name="room"></param>
    public class Harvester(IRoom room) : IRole
    {
        private readonly IRoom room = room;

        private readonly List<ISource> cachedSources = [];
        private readonly List<IStructureSpawn> cachedSpawns = [];

        public void Run(ICreep creep)
        {
            // Check energy storage
            if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            {
                // There is space for more energy - harvest the nearest source
                var source = FindNearestSource(creep.LocalPosition);
                if (source == null) { return; }
                var harvestResult = creep.Harvest(source);
                if (harvestResult == CreepHarvestResult.NotInRange)
                {
                    creep.MoveTo(source.RoomPosition);
                }
                else if (harvestResult != CreepHarvestResult.Ok)
                {
                    Console.WriteLine($"{this}: {creep} unexpected result when harvesting {source} ({harvestResult})");
                }
            }
            else
            {
                // We're full - drop off at the nearest spawn
                var spawn = FindNearestSpawn(creep.LocalPosition);
                if (spawn == null) { return; }
                var transferResult = creep.Transfer(spawn, ResourceType.Energy);
                if (transferResult == CreepTransferResult.NotInRange)
                {
                    creep.MoveTo(spawn.RoomPosition);
                }
                else if (transferResult != CreepTransferResult.Ok)
                {
                    Console.WriteLine($"{this}: {creep} unexpected result when depositing to {spawn} ({transferResult})");
                }
            }
        }

        private ISource? FindNearestSource(Position pos)
        {
            // If there are no sources in the cache, assume the cache has not yet been built, so populate it
            if (cachedSources.Count == 0)
            {
                foreach (var source in room.Find<ISource>())
                {
                    cachedSources.Add(source);
                }
            }

            // Now find the one closest to the position
            return cachedSources.MinBy(x => x.LocalPosition.LinearDistanceTo(pos));
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
