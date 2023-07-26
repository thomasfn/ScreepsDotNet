using System;
using System.Linq;

using ScreepsDotNet.API;

namespace ScreepsDotNet
{
    public class Tutorial3_FirstAttack : ITutorialScript
    {
        private readonly IGame game;

        public Tutorial3_FirstAttack(IGame game)
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
            if (!enemyCreeps.Any())
            {
                Console.WriteLine($"No enemy creeps found!");
                return;
            }

            foreach (var myCreep in myCreeps)
            {
                var enemyCreep = enemyCreeps.First();
                Console.WriteLine($"Attemping to attack {enemyCreep} with {myCreep}");
                if (myCreep.Attack(enemyCreep) == CreepAttackResult.NotInRange)
                {
                    Console.WriteLine($"Out of range, moving...");
                    myCreep.MoveTo(enemyCreep);
                }
            }
        }
    }
}
