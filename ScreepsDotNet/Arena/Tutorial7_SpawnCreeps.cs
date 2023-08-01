using System;
using System.Linq;
using System.Collections.Generic;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Arena
{
    public class Tutorial7_SpawnCreeps : ITutorialScript
    {
        private readonly IGame game;

        private readonly IDictionary<IFlag, ICreep> flagCreepAssignments = new Dictionary<IFlag, ICreep>();

        public Tutorial7_SpawnCreeps(IGame game)
        {
            this.game = game;
        }

        public void Loop()
        {
            var spawn = game.Utils.GetObjectsByType<IStructureSpawn>()
                .Where(x => x.My ?? false)
                .FirstOrDefault();
            if (spawn == null)
            {
                Console.WriteLine($"No spawn found!");
                return;
            }

            var flags = game.Utils.GetObjectsByType<IFlag>();
            if (!flags.Any())
            {
                Console.WriteLine($"No flags found!");
                return;
            }

            foreach (var flag in flags)
            {
                if (!flagCreepAssignments.TryGetValue(flag, out var assignedCreep))
                {
                    if (spawn.Spawning != null) { continue; }
                    Console.WriteLine($"Spawning creep for {flag}");
                    var result = spawn.SpawnCreep(new BodyPartType[] { BodyPartType.Move });
                    if (result.Object != null)
                    {
                        flagCreepAssignments.Add(flag, result.Object);
                        Console.WriteLine($"{spawn}: Spawning creep {result.Object}");
                        // If we spawn something this tick, the spawn won't have a "Spawning" object yet until next tick as the spawn command is queued and not processed immediately
                        // Therefore we shouldn't try to spawn anything else this tick
                        break;
                    }
                    if (result.Error != null)
                    {
                        Console.WriteLine($"{spawn}: Spawn creep error {result.Error}");
                    }
                    continue;
                }

                if (assignedCreep.Spawning) { continue; }

                assignedCreep.MoveTo(flag);
            }

        }
    }
}
