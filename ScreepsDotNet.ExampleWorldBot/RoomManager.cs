using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using ScreepsDotNet.ExampleWorldBot.Roles;

namespace ScreepsDotNet.ExampleWorldBot
{
    /// <summary>
    /// The room manager will take care of all creep and spawning logic for a certain room controlled by our bot.
    /// </summary>
    public class RoomManager
    {
        private readonly IGame game;
        private readonly IRoom room;

        private readonly Dictionary<string, IRole> roleMap = [];

        private readonly HashSet<IStructureSpawn> cachedSpawns = [];

        private readonly HashSet<ICreep> allCreeps = [];
        private readonly HashSet<ICreep> harvesterCreeps = [];
        private readonly HashSet<ICreep> upgraderCreeps = [];

        private readonly Random rng = new();

        private const int targetMinerCount = 3;
        private const int targetUpgraderCount = 3;

        private static readonly BodyType<BodyPartType> workerBodyType = new([(BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 1)]);

        public RoomManager(IGame game, IRoom room)
        {
            this.game = game;
            this.room = room;

            // Populate role map - the role instances will live in the heap until the next IVM reset
            roleMap.Add("harvester", new Harvester(room));
            roleMap.Add("upgrader", new Upgrader(room));

            // Populate the spawns cache
            // TODO: We're going to want a way to update this periodically in case new spawns get built or old ones destroyed
            // We use a cache because calls to room.Find are expensive on CPU usage!
            foreach (var spawn in room.Find<IStructureSpawn>())
            {
                cachedSpawns.Add(spawn);
            }
        }

        public void Tick()
        {
            // Check for any creeps we're tracking that no longer exist
            foreach (var creep in allCreeps.ToImmutableArray())
            {
                if (creep.Exists) { continue; }
                allCreeps.Remove(creep);
                OnCreepDied(creep);
            }

            // Check the latest creep list for any new creeps
            var newCreepList = new HashSet<ICreep>(room.Find<ICreep>().Where(static x => x.My));
            foreach (var creep in newCreepList)
            {
                if (!allCreeps.Add(creep)) { continue; }
                OnCreepSpawned(creep);
            }

            // Tick all spawns
            foreach (var spawn in cachedSpawns)
            {
                if (!spawn.Exists) { continue; }
                TickSpawn(spawn);
            }

            // Tick all tracked creeps
            foreach (var creep in allCreeps)
            {
                TickCreep(creep);
            }
        }

        private void OnCreepSpawned(ICreep creep)
        {
            // Get their role instance and sort them into the correct list
            var role = GetCreepRole(creep);
            switch (role)
            {
                case Harvester:
                    harvesterCreeps.Add(creep);
                    break;
                case Upgrader:
                    upgraderCreeps.Add(creep);
                    break;
                default:
                    Console.WriteLine($"{this}: {creep} has unknown role!");
                    break;
            }
        }

        private void OnCreepDied(ICreep creep)
        {
            // Remove it from all tracking lists
            harvesterCreeps.Remove(creep);
            upgraderCreeps.Remove(creep);
            Console.WriteLine($"{this}: {creep} died");
        }

        private void TickSpawn(IStructureSpawn spawn)
        {
            // Check if we're able to spawn something, and spawn until we've filled our target role counts
            if (spawn.Spawning != null) { return; }
            if (harvesterCreeps.Count < targetMinerCount)
            {
                TrySpawnCreep(spawn, workerBodyType, "harvester");
            }
            else if (upgraderCreeps.Count < targetUpgraderCount)
            {
                TrySpawnCreep(spawn, workerBodyType, "upgrader");
            }
        }

        private void TrySpawnCreep(IStructureSpawn spawn, BodyType<BodyPartType> bodyType, string roleName)
        {
            var name = FindUniqueCreepName();
            if (spawn.SpawnCreep(bodyType, name, new(dryRun: true)) == SpawnCreepResult.Ok)
            {
                Console.WriteLine($"{this}: spawning a {roleName} ({workerBodyType}) from {spawn}...");
                var initialMemory = game.CreateMemoryObject();
                initialMemory.SetValue("role", roleName);
                spawn.SpawnCreep(bodyType, name, new(dryRun: false, memory: initialMemory));
            }
        }

        private void TickCreep(ICreep creep)
        {
            // Lookup the role instance
            var role = GetCreepRole(creep);
            if (role == null) { return; }

            // Run the role logic
            role.Run(creep);
        }

        private IRole? GetCreepRole(ICreep creep)
        {
            // First, see if we've stored the role instance on the creep from a previous tick (this will save us some CPU)
            if (creep.TryGetUserData<IRole>(out var role)) { return role; }

            // Lookup their role from memory
            if (!creep.Memory.TryGetString("role", out var roleName)) { return null; }

            // Lookup the role instance
            if (!roleMap.TryGetValue(roleName, out role)) { return null; }

            // We found it, assign it to the creep user data for later retrieval
            creep.SetUserData(role);
            return role;
        }

        private string FindUniqueCreepName()
            => $"{room.Name}_{rng.Next()}";

        public override string ToString()
            => $"RoomManager[{room.Name}]";
    }
}
