using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Arena
{
    public class Tutorial10_FinalTest : ITutorialScript
    {
        private readonly IGame game;

        private IStructureSpawn? mySpawn;
        private IEnumerable<ICreep> allCreeps = Enumerable.Empty<ICreep>();
        private IEnumerable<ICreep> myCreeps = Enumerable.Empty<ICreep>();
        private IEnumerable<ICreep> enemyCreeps = Enumerable.Empty<ICreep>();

        private bool readyToAttack = false;

        private readonly Queue<IEnumerable<BodyPartType>> spawnQueue = new();

        public Tutorial10_FinalTest(IGame game)
        {
            this.game = game;

            // Prepare a queue of creeps to spawn
            for (int i = 0; i < 3; ++i)
            {
                spawnQueue.Enqueue(new BodyPartType[] { BodyPartType.Move, BodyPartType.Work, BodyPartType.Carry });
            }
            spawnQueue.Enqueue(new BodyPartType[] { BodyPartType.Move, BodyPartType.Attack });
            spawnQueue.Enqueue(new BodyPartType[] { BodyPartType.Move, BodyPartType.Heal });
            spawnQueue.Enqueue(new BodyPartType[] { BodyPartType.Move, BodyPartType.RangedAttack });
        }

        public void Loop()
        {
            // Cache lists and objects
            mySpawn = game.Utils.GetObjectsByType<IStructureSpawn>()
                .Where(x => x.My ?? false)
                .FirstOrDefault();
            allCreeps = game.Utils.GetObjectsByType<ICreep>();
            myCreeps = allCreeps.Where(x => x.Exists && x.My && (mySpawn == null || x.Position != mySpawn.Position)).ToArray();
            enemyCreeps = allCreeps.Where(x => x.Exists && !x.My).ToArray();

            // Skip first tick as JIT often causes us to time out here
            if (game.Utils.GetTicks() <= 1) { return; }

            // Process all our owned objects
            ProcessCreeps();
            ProcessSpawn();
        }

        private void ProcessSpawn()
        {
            // If the spawn is gone or all creeps are spawned, signal the attack
            if (mySpawn == null || spawnQueue.Count == 0)
            {
                readyToAttack = true;
                return;
            }

            // If spawn is still spawning something, wait
            if (mySpawn.Spawning != null) { return; }

            // Check if spawn has enough energy to spawn next creep in the queue
            var spawnNext = spawnQueue.Peek();
            var costToSpawnNext = spawnNext.Select(game.Constants.GetBodyPartCost).Sum();
            if (mySpawn.Store[ResourceType.Energy] < costToSpawnNext) { return; }

            // Spawn
            spawnNext = spawnQueue.Dequeue();
            var spawnResult = mySpawn.SpawnCreep(spawnNext);
            if (spawnResult.Error != null)
            {
                Console.WriteLine($"{mySpawn}: unexpected result when spawning {spawnNext.ToBodyString()} ({spawnResult.Error})");
                return;
            }
            Console.WriteLine($"{mySpawn}: spawning {spawnResult.Object}");
        }

        private void ProcessCreeps()
        {
            foreach (var myCreep in myCreeps)
            {
                // Check what body parts the creep has and defer to different logic based on their archetype
                var body = myCreep.Body;
                if (body.Any(x => x.Type == BodyPartType.Work))
                {
                    ProcessWorker(myCreep);
                }
                else if (body.Any(x => x.Type == BodyPartType.Heal))
                {
                    ProcessHealer(myCreep);
                }
                else if (body.Any(x => x.Type == BodyPartType.RangedAttack))
                {
                    ProcessRangedAttacker(myCreep);
                }
                else if (body.Any(x => x.Type == BodyPartType.Attack))
                {
                    ProcessAttacker(myCreep);
                }
            }
        }

        private void ProcessWorker(ICreep creep)
        {
            // Find nearest source
            var source = creep.FindClosestByPath(game.Utils.GetObjectsByType<ISource>(), null);
            if (source == null)
            {
                Console.WriteLine($"{creep}: Could not find a source to work");
                return;
            }

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
                return;
            }

            // If the creep has energy, move to spawn and deposit it
            if (creep.Store[ResourceType.Energy] > 0 && mySpawn != null)
            {
                var transferResult = creep.Transfer(mySpawn, ResourceType.Energy);
                if (transferResult == CreepTransferResult.NotInRange)
                {
                    creep.MoveTo(mySpawn);
                }
                else if (transferResult != CreepTransferResult.Ok)
                {
                    Console.WriteLine($"{creep}: Unexpected transfer result for {mySpawn} {transferResult}");
                }
            }
        }

        private void ProcessHealer(ICreep creep)
        {
            // Look for injured friendly creeps to heal; prioritise most hurt ones first
            var target = myCreeps
                .Where(x => x.Hits < x.HitsMax)
                .OrderBy(x => x.Hits / (double)x.HitsMax)
                .FirstOrDefault();
            if (target == null) { return; }
            if (creep.Heal(target) == CreepHealResult.NotInRange)
            {
                creep.MoveTo(target);
            }
        }

        private void ProcessRangedAttacker(ICreep creep)
        {
            // If we're not yet ready to attack, hang around
            if (!readyToAttack) { return; }

            // Look for enemy creeps to attack; prioritise closer ones first
            var target = creep.FindClosestByPath(enemyCreeps, null);
            if (target == null) { return; }
            if (creep.RangedAttack(target) == CreepAttackResult.NotInRange)
            {
                creep.MoveTo(target);
            }
        }

        private void ProcessAttacker(ICreep creep)
        {
            // If we're not yet ready to attack, hang around
            if (!readyToAttack) { return; }

            // Look for enemy creeps to attack; prioritise closer ones first
            var target = creep.FindClosestByPath(enemyCreeps, null);
            if (target == null) { return; }
            if (creep.Attack(target) == CreepAttackResult.NotInRange)
            {
                creep.MoveTo(target);
            }
        }
    }
}
