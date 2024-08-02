using System;
using System.Collections.Generic;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Bot;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.ExampleWorldBot
{
    public class BasicExample : IBot
    {
        private readonly IGame game;

        private readonly Dictionary<IRoom, RoomManager> roomManagers = [];

        public BasicExample(IGame game)
        {
            this.game = game;

            // Clean memory once on startup
            // Since our IVM will reset periodically, this will run frequently enough without us needing to schedule it properly
            CleanMemory();
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
                    Console.WriteLine($"Adding room manager for {room} as it is now visible and controlled by us");
                    roomManager = new RoomManager(game, room);
                    roomManagers.Add(room, roomManager);
                }
                roomManager.Tick();
            }

        }

        private void CleanMemory()
        {
            if (!game.Memory.TryGetObject("creeps", out var creepsObj)) { return; }

            // Delete all creeps in memory that no longer exist
            int clearCnt = 0;
            foreach (var creepName in creepsObj.Keys)
            {
                if (!game.Creeps.ContainsKey(creepName))
                {
                    creepsObj.ClearValue(creepName);
                    ++clearCnt;
                }
            }
            if (clearCnt > 0) { Console.WriteLine($"Cleared {clearCnt} dead creeps from memory"); }
        }
    }
}
