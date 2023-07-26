using System;
using System.Collections.Generic;
using System.Linq;

using ScreepsDotNet.API;

namespace ScreepsDotNet
{
    public class Tutorial4_CreepsBodies : ITutorialScript
    {
        private readonly IGame game;

        private IEnumerable<ICreep> myCreeps = Enumerable.Empty<ICreep>();
        private IEnumerable<ICreep> enemyCreeps = Enumerable.Empty<ICreep>();

        public Tutorial4_CreepsBodies(IGame game)
        {
            this.game = game;
        }

        public void Loop()
        {
            var allCreeps = game.Utils.GetObjectsByType<ICreep>();

            myCreeps = allCreeps
                .Where(x => x.My && !x.Spawning)
                .ToArray();
            if (!myCreeps.Any())
            {
                Console.WriteLine($"No friendly creeps found!");
                return;
            }

            enemyCreeps = allCreeps
                .Where(x => !x.My)
                .ToArray();
            if (!enemyCreeps.Any())
            {
                Console.WriteLine($"No enemy creeps found!");
                return;
            }

            foreach (var myCreep in myCreeps)
            {
                // Check what body parts the creep has and defer to different logic based on their archetype
                var body = myCreep.Body;
                if (body.Any(x => x.Type == BodyPartType.Heal))
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

        private void ProcessHealer(ICreep creep)
        {
            // Look for injured friendly creeps to heal; prioritise most hurt ones first
            var target = myCreeps
                .Where(x => x.Hits < x.HitsMax)
                .OrderBy(x => x.Hits / (double)x.HitsMax)
                .FirstOrDefault();
            if (target == null)
            {
                Console.WriteLine($"{creep} could not find any injured teammates to heal!");
                return;
            }
            if (creep.Heal(target) == CreepHealResult.NotInRange)
            {
                creep.MoveTo(target);
            }
        }

        private void ProcessRangedAttacker(ICreep creep)
        {
            // Look for enemy creeps to attack; prioritise closer ones first
            var target = creep.FindClosestByPath(enemyCreeps, null);
            if (target == null)
            {
                Console.WriteLine($"{creep} could not find any enemies to range attack!");
                return;
            }
            if (creep.RangedAttack(target) == CreepAttackResult.NotInRange)
            {
                creep.MoveTo(target);
            }
        }

        private void ProcessAttacker(ICreep creep)
        {
            // Look for enemy creeps to attack; prioritise closer ones first
            var target = creep.FindClosestByPath(enemyCreeps, null);
            if (target == null)
            {
                Console.WriteLine($"{creep} could not find any enemies to range attack!");
                return;
            }
            if (creep.Attack(target) == CreepAttackResult.NotInRange)
            {
                creep.MoveTo(target);
            }
        }
    }
}
