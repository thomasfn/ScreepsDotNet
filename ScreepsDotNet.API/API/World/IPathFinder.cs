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
        public readonly Func<string, ICostMatrix, ICostMatrix?>? CostCallback;

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
            Func<string, ICostMatrix, ICostMatrix?>? costCallback = null,
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

    public readonly struct SearchPathOptions
    {
        /// <summary>
        /// Request from the pathfinder to generate a CostMatrix for a certain room.
        /// The callback accepts one argument, roomName.
        /// This callback will only be called once per room per search.
        /// If you are running multiple pathfinding operations in a single room and in a single tick you may consider caching your CostMatrix to speed up your code.
        /// Please read the CostMatrix documentation below for more information on CostMatrix.
        /// If you return null from the callback the requested room will not be searched, and it won't count against maxRooms
        /// </summary>
        public readonly Func<string, ICostMatrix?>? RoomCallback;

        /// <summary>
        /// Cost for walking on plain positions. The default is 1.
        /// </summary>
        public readonly int? PlainCost;

        /// <summary>
        /// Cost for walking on swamp positions. The default is 5.
        /// </summary>
        public readonly int? SwampCost;

        /// <summary>
        /// Instead of searching for a path to the goals this will search for a path away from the goals. The cheapest path that is out of range of every goal will be returned. The default is false.
        /// </summary>
        public readonly bool? Flee;

        /// <summary>
        /// The maximum allowed pathfinding operations. You can limit CPU time used for the search based on ratio 1 op ~ 0.001 CPU. The default value is 2000.
        /// </summary>
        public readonly int? MaxOps;

        /// <summary>
        /// The maximum allowed rooms to search. The default is 16, maximum is 64.
        /// </summary>
        public readonly int? MaxRooms;

        /// <summary>
        /// The maximum allowed cost of the path returned. If at any point the pathfinder detects that it is impossible to find a path with a cost less than or equal to maxCost it will immediately halt the search. The default is Infinity.
        /// </summary>
        public readonly int? MaxCost;

        /// <summary>
        /// Weight to apply to the heuristic in the A* formula F = G + weight * H. Use this option only if you understand the underlying A* algorithm mechanics! The default value is 1.2.
        /// </summary>
        public readonly double? HeuristicWeight;

        public SearchPathOptions(
            Func<string, ICostMatrix?>? roomCallback = null,
            int? plainCost = null,
            int? swampCost = null,
            bool? flee = null,
            int? maxOps = null,
            int? maxRooms = null,
            int? maxCost = null,
            double? heuristicWeight = null
        )
        {
            RoomCallback = roomCallback;
            PlainCost = plainCost;
            SwampCost = swampCost;
            Flee = flee;
            MaxOps = maxOps;
            MaxRooms = maxRooms;
            MaxCost = maxCost;
            HeuristicWeight = heuristicWeight;
        }
    }

    public readonly struct SearchPathResult
    {
        private readonly RoomPosition[] path;

        public ReadOnlySpan<RoomPosition> Path => Path;

        /// <summary>
        /// Total number of operations performed before this path was calculated.
        /// </summary>
        public readonly int Ops;

        /// <summary>
        /// The total cost of the path as derived from plainCost, swampCost and any given CostMatrix instances.
        /// </summary>
        public readonly int Cost;

        /// <summary>
        /// If the pathfinder fails to find a complete path, this will be true. Note that path will still be populated with a partial path which represents the closest path it could find given the search parameters.
        /// </summary>
        public readonly bool Incomplete;

        public SearchPathResult(ReadOnlySpan<RoomPosition> path, int ops, int cost, bool incomplete)
        {
            this.path = path.ToArray();
            Ops = ops;
            Cost = cost;
            Incomplete = incomplete;
        }
    }

    public readonly struct Goal
    {
        public readonly RoomPosition Position;
        public readonly int Range;

        public Goal(RoomPosition position, int range)
        {
            Position = position;
            Range = range;
        }
    }

    /// <summary>
    /// Contains powerful methods for pathfinding in the game world. This module is written in fast native C++ code and supports custom navigation costs and paths which span multiple rooms.
    /// </summary>
    public interface IPathFinder
    {
        /// <summary>
        /// Find an optimal path between origin and goal.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="goal"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        SearchPathResult Search(RoomPosition origin, Goal goal, SearchPathOptions? opts = null);

        /// <summary>
        /// Find an optimal path between origin and the nearest goal (by path cost).
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="goals"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        SearchPathResult Search(RoomPosition origin, IEnumerable<Goal> goals, SearchPathOptions? opts = null);
    }
}
