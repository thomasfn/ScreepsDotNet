using System;
using System.Collections.Generic;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

using ScreepsDotNet.SpawnAndSwamp;

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
            var trackedRooms = roomManagers.Keys.ToArray();
            foreach (var room in trackedRooms)
            {
                if (room.Exists) { continue; }
                Console.WriteLine($"Removing room manager for {room} as it is no longer visible");
                roomManagers.Remove(room);
            }
            foreach (var room in game.Rooms)
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
        }

        public void Init()
        {
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
        }

        public void Tick()
        {
            foreach (var creep in allCreeps.ToArray())
            {
                if (creep.Exists) { continue; }
                allCreeps.Remove(creep);
                OnCreepDied(creep);
            }
            var newCreepList = new HashSet<ICreep>(room.Find<ICreep>().Where(x => x.My));
            foreach (var creep in newCreepList)
            {
                if (!allCreeps.Add(creep)) { continue; }
                OnCreepSpawned(creep);
            }
            foreach (var spawn in spawns)
            {
                if (!spawn.Exists) { continue; }
                TickSpawn(spawn);
            }
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
            if (creep.BodyType == workerBodyType)
            {
                minerCreeps.Remove(creep);
                upgraderCreeps.Remove(creep);
                Console.WriteLine($"{this}: {creep} died");
            }
        }

        private void TickSpawn(IStructureSpawn spawn)
        {
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
            if (creep.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            {
                // There is space for more energy
                if (!sources.Any()) { return; }
                var source = sources.Best(x => -x.Position.Position.LinearDistanceTo(creep.Position.Position));
                if (source == null) { return; }
                var harvestResult = creep.Harvest(source);
                if (harvestResult == CreepHarvestResult.NotInRange)
                {
                    creep.MoveTo(source.Position);
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
                var spawn = spawns.Best(x => -x.Position.Position.LinearDistanceTo(creep.Position.Position));
                if (spawn == null) { return; }
                var transferResult = creep.Transfer(spawn, ResourceType.Energy);
                if (transferResult == CreepTransferResult.NotInRange)
                {
                    creep.MoveTo(spawn.Position);
                }
                else if (transferResult != CreepTransferResult.Ok)
                {
                    Console.WriteLine($"{this}: {creep} unexpected result when depositing to {spawn} ({transferResult})");
                }
            }
        }

        private void TickUpgrader(ICreep creep)
        {
            var controller = room.Controller;
            if (controller == null) { return; }
            if (creep.Store[ResourceType.Energy] > 0)
            {
                // There is energy to drop off
                var upgradeResult = creep.UpgradeController(controller);
                if (upgradeResult == CreepUpgradeControllerResult.NotInRange)
                {
                    creep.MoveTo(controller.Position);
                }
                else if (upgradeResult != CreepUpgradeControllerResult.Ok)
                {
                    Console.WriteLine($"{this}: {creep} unexpected result when upgrading {controller} ({upgradeResult})");
                }
            }
            else
            {
                // We're empty, go to pick up
                if (!spawns.Any()) { return; }
                var spawn = spawns.Best(x => -x.Position.Position.LinearDistanceTo(creep.Position.Position));
                if (spawn == null) { return; }
                var withdrawResult = creep.Withdraw(spawn, ResourceType.Energy);
                if (withdrawResult == CreepWithdrawResult.NotInRange)
                {
                    creep.MoveTo(spawn.Position);
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
