using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public readonly struct PathStep
    {
        public readonly int X;
        public readonly int Y;
        public readonly int DX;
        public readonly int DY;
        public readonly Direction Direction;

        public Position Position => (X, Y);

        public PathStep(int x, int y, int dX, int dY, Direction direction)
        {
            X = x;
            Y = y;
            DX = dX;
            DY = dY;
            Direction = direction;
        }
    }

    public readonly struct FindPathOptions
    {
        /// <summary>
        /// Treat squares with creeps as walkable. Can be useful with too many moving creeps around or in some other cases. The default value is false.
        /// </summary>
        public readonly bool? IgnoreCreeps;

        /// <summary>
        /// Treat squares with destructible structures (constructed walls, ramparts, spawns, extensions) as walkable. The default value is false.
        /// </summary>
        public readonly bool? IgnoreDestructibleStructures;

        /// <summary>
        /// Ignore road structures. Enabling this option can speed up the search. The default value is false. This is only used when the new PathFinder is enabled.
        /// </summary>
        public readonly bool? IgnoreRoads;

        /// <summary>
        /// You can use this callback to modify a CostMatrix for any room during the search.
        /// The callback accepts two arguments, roomName and costMatrix.
        /// Use the costMatrix instance to make changes to the positions costs.
        /// If you return a new matrix from this callback, it will be used instead of the built-in cached one.
        /// This option is only used when the new PathFinder is enabled.
        /// </summary>
        public readonly Func<string, object, object?>? CostCallback;

        /// <summary>
        /// An array of the room's objects or RoomPosition objects which should be treated as walkable tiles during the search.
        /// This option cannot be used when the new PathFinder is enabled (use costCallback option instead).
        /// </summary>
        public readonly IEnumerable<Position>? Ignore;

        /// <summary>
        /// An array of the room's objects or RoomPosition objects which should be treated as walkable tiles during the search.
        /// This option cannot be used when the new PathFinder is enabled (use costCallback option instead).
        /// </summary>
        public readonly IEnumerable<Position>? Avoid;

        /// <summary>
        /// The maximum limit of possible pathfinding operations. You can limit CPU time used for the search based on ratio 1 op ~ 0.001 CPU. The default value is 2000.
        /// </summary>
        public readonly int? MaxOps;

        /// <summary>
        /// Weight to apply to the heuristic in the A formula F = G + weight H. Use this option only if you understand the underlying A* algorithm mechanics! The default value is 1.2.
        /// </summary>
        public readonly double? HeuristicWeight;

        /// <summary>
        /// The maximum allowed rooms to search. The default (and maximum) is 16. This is only used when the new PathFinder is enabled.
        /// </summary>
        public readonly int? MaxRooms;

        /// <summary>
        /// Find a path to a position in specified linear range of target. The default is 0.
        /// </summary>
        public readonly int? Range;

        /// <summary>
        /// Cost for walking on plain positions. The default is 1.
        /// </summary>
        public readonly int? PlainCost;

        /// <summary>
        /// Cost for walking on swamp positions. The default is 5.
        /// </summary>
        public readonly int? SwampCost;

        public FindPathOptions(
            bool? ignoreCreeps = null,
            bool? ignoreDestructibleStructures = null,
            bool? ignoreRoads = null,
            Func<string, object, object?>? costCallback = null,
            IEnumerable<Position>? ignore = null,
            IEnumerable<Position>? avoid = null,
            int? maxOps = null,
            double? heuristicWeight = null,
            int? maxRooms = null,
            int? range = null,
            int? plainCost = null,
            int? swampCost = null
        )
        {
            IgnoreCreeps = ignoreCreeps;
            IgnoreDestructibleStructures = ignoreDestructibleStructures;
            IgnoreRoads = ignoreRoads;
            CostCallback = costCallback;
            Ignore = ignore;
            Avoid = avoid;
            MaxOps = maxOps;
            HeuristicWeight = heuristicWeight;
            MaxRooms = maxRooms;
            Range = range;
            PlainCost = plainCost;
            SwampCost = swampCost;
        }
    }
}
