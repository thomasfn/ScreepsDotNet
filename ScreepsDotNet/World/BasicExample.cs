using System;
using System.Collections.Generic;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.World
{
    public class BasicExample : ITutorialScript
    {
        private readonly IGame game;

        private readonly IDictionary<IRoom, RoomManager> roomManagers = new Dictionary<IRoom, RoomManager>();

        public BasicExample(IGame game)
        {
            this.game = game;
        }

        public void Loop()
        {
            // Check for any rooms that are no longer visible and remove their manager
            var trackedRooms = roomManagers.Keys.ToArray();
            foreach (var room in trackedRooms)
            {
                if (room.Exists) { continue; }
                Console.WriteLine($"Removing room manager for {room} as it is no longer visible");
                roomManagers.Remove(room);
            }

            // Iterate over all visible rooms, create their manager if needed, and tick them
            foreach (var room in game.Rooms.Values)
            {
                if (!room.Controller?.My ?? false) { continue; }
                if (!roomManagers.TryGetValue(room, out var roomManager))
                {
                    roomManager = new RoomManager(game, room);
                    roomManagers.Add(room, roomManager);
                    Console.WriteLine($"Adding room manager for {room} as it is now visible and controlled by us");
                    roomManager.Init();
                }
                roomManager.Tick();
            }
            
        }
    }

    internal class RoomManager
    {
        private readonly IGame game;
        private readonly IRoom room;
        private readonly IStructureController roomController;

        private readonly ISet<IStructureSpawn> spawns = new HashSet<IStructureSpawn>();
        private readonly ISet<ISource> sources = new HashSet<ISource>();

        private readonly ISet<ICreep> allCreeps = new HashSet<ICreep>();
        private readonly ISet<ICreep> minerCreeps = new HashSet<ICreep>();
        private readonly ISet<ICreep> upgraderCreeps = new HashSet<ICreep>();

        private readonly Random rng = new();

        private int targetMinerCount = 0;
        private int targetUpgraderCount = 3;

        private static readonly BodyType<BodyPartType> workerBodyType = new(stackalloc (BodyPartType, int)[] { (BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 1) });

        public RoomManager(IGame game, IRoom room)
        {
            this.game = game;
            this.room = room;
            var roomController = room.Controller;
            if (roomController == null) { throw new InvalidOperationException($"Room {room} has no controller!"); }
            this.roomController = roomController;
        }

        public void Init()
        {
            // Cache all spawns and sources
            spawns.Clear();
            foreach (var spawn in room.Find<IStructureSpawn>())
            {
                spawns.Add(spawn);
            }
            sources.Clear();
            targetMinerCount = 0;
            foreach (var source in room.Find<ISource>())
            {
                sources.Add(source);
                ++targetMinerCount;
            }
            Console.WriteLine($"{this}: INIT success (tracking {spawns.Count} spawns and {sources.Count} sources)");
            Console.WriteLine($"{this}: room exits = {game.Map.DescribeExits(room.Name)}");
        }

        public void Tick()
        {
            // Check for any creeps we're tracking that no longer exist
            foreach (var creep in allCreeps.ToArray())
            {
                if (creep.Exists) { continue; }
                allCreeps.Remove(creep);
                OnCreepDied(creep);
            }

            // Check the latest creep list for any new creeps
            var newCreepList = new HashSet<ICreep>(room.Find<ICreep>().Where(x => x.My));
            foreach (var creep in newCreepList)
            {
                if (!allCreeps.Add(creep)) { continue; }
                OnCreepSpawned(creep);
            }

            // Tick all spawns
            foreach (var spawn in spawns)
            {
                if (!spawn.Exists) { continue; }
                TickSpawn(spawn);
            }

            // Tick all tracked creeps
            foreach (var creep in minerCreeps)
            {
                TickMiner(creep);
            }
            foreach (var creep in upgraderCreeps)
            {
                TickUpgrader(creep);
            }
        }

        private void OnCreepSpawned(ICreep creep)
        {
            // Check the body type and assign the creep a role by putting it in the right tracking list
            if (creep.BodyType == workerBodyType)
            {
                if (minerCreeps.Count < targetMinerCount)
                {
                    Console.WriteLine($"{this}: {creep} assigned as miner");
                    minerCreeps.Add(creep);
                    return;
                }
                if (upgraderCreeps.Count < targetUpgraderCount)
                {
                    Console.WriteLine($"{this}: {creep} assigned as upgrader");
                    upgraderCreeps.Add(creep);
                    return;
                }
            }
            Console.WriteLine($"{this}: {creep} unknown role");
        }

        private void OnCreepDied(ICreep creep)
        {
            // Remove it from all tracking lists
            minerCreeps.Remove(creep);
            upgraderCreeps.Remove(creep);
            Console.WriteLine($"{this}: {creep} died");
        }

        private void TickSpawn(IStructureSpawn spawn)
        {
            // Check if we're able to spawn something, and spawn until we've filled our target role counts
            if (spawn.Spawning != null) { return; }
            if (minerCreeps.Count < targetMinerCount || upgraderCreeps.Count < targetUpgraderCount)
            {
                var name = FindUniqueCreepName();
                if (spawn.SpawnCreep(workerBodyType, name, new(dryRun: true)) == SpawnCreepResult.Ok)
                {
                    Console.WriteLine($"{this}: spawning a {workerBodyType} from {spawn}...");
                    spawn.SpawnCreep(workerBodyType, name);
                }
            }
        }

        private void TickMiner(ICreep creep)
        {
            // Check energy storage
            if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            {
                // There is space for more energy
                if (!sources.Any()) { return; }
                var source = sources.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
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
                // We're full, go to drop off
                if (!spawns.Any()) { return; }
                var spawn = spawns.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
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

        private void TickUpgrader(ICreep creep)
        {
            // Check energy storage
            if (creep.Store[ResourceType.Energy] > 0)
            {
                // There is energy to drop off
                var upgradeResult = creep.UpgradeController(roomController);
                if (upgradeResult == CreepUpgradeControllerResult.NotInRange)
                {
                    creep.MoveTo(roomController.RoomPosition);
                }
                else if (upgradeResult != CreepUpgradeControllerResult.Ok)
                {
                    Console.WriteLine($"{this}: {creep} unexpected result when upgrading {roomController} ({upgradeResult})");
                }
            }
            else
            {
                // We're empty, go to pick up
                if (!spawns.Any()) { return; }
                var spawn = spawns.Best(x => -x.LocalPosition.LinearDistanceTo(creep.LocalPosition));
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

        private string FindUniqueCreepName()
            => $"{room.Name}_{rng.Next()}";

        public override string ToString()
            => $"RoomManager[{room.Name}]";
    }
}
