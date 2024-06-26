﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ScreepsDotNet.API.Arena
{
    public readonly struct SearchPathOptions
    {
        /// <summary>
        /// Custom navigation cost data
        /// </summary>
        public readonly ICostMatrix? CostMatrix;

        /// <summary>
        /// Cost for walking on plain positions.
        /// </summary>
        public readonly byte PlainCost;

        /// <summary>
        /// Cost for walking on swamp positions.
        /// </summary>
        public readonly byte SwampCost;

        /// <summary>
        /// Instead of searching for a path to the goals this will search for a path away from the goals.
        /// The cheapest path that is out of range of every goal will be returned.
        /// </summary>
        public readonly bool Flee;

        /// <summary>
        /// The maximum allowed pathfinding operations.
        /// </summary>
        public readonly int MaxOps;

        /// <summary>
        /// The maximum allowed cost of the path returned.
        /// </summary>
        public readonly int MaxCost;

        /// <summary>
        /// Weight from 1 to 9 to apply to the heuristic in the A* formula F = G + weight * H.
        /// </summary>
        public readonly double HeuristicWeight;

        public SearchPathOptions(ICostMatrix? costMatrix = null, byte plainCost = 2, byte swampCost = 10, bool flee = false, int maxOps = 50000, int maxCost = int.MaxValue, double heuristicWeight = 1.2)
        {
            CostMatrix = costMatrix;
            PlainCost = plainCost;
            SwampCost = swampCost;
            Flee = flee;
            MaxOps = maxOps;
            MaxCost = maxCost;
            HeuristicWeight = heuristicWeight;
        }
    }

    public readonly struct FindPathOptions
    {
        public readonly SearchPathOptions BaseOptions;
        private readonly IGameObject[] ignore;
        public ReadOnlySpan<IGameObject> Ignore => ignore;

        public FindPathOptions(SearchPathOptions baseOptions, ReadOnlySpan<IGameObject> ignore)
        {
            BaseOptions = baseOptions;
            this.ignore = ignore.ToArray();
        }

        public static implicit operator FindPathOptions(SearchPathOptions options) => new(options, ReadOnlySpan<IGameObject>.Empty);
    }

    public readonly struct SearchPathResult : IEquatable<SearchPathResult>
    {
        private readonly Position[] path;

        /// <summary>
        /// The path found as an array of objects containing x and y properties
        /// </summary>
        public ReadOnlySpan<Position> Path => path;

        /// <summary>
        /// Total number of operations performed before this path was calculated
        /// </summary>
        public readonly int Ops;

        /// <summary>
        /// The total cost of the path as derived from plainCost, swampCost, and given CostMatrix instance
        /// </summary>
        public readonly double Cost;

        /// <summary>
        /// If the pathfinder fails to find a complete path, this will be true
        /// </summary>
        public readonly bool Incomplete;

        public SearchPathResult(ReadOnlySpan<Position> path, int ops, double cost, bool incomplete)
        {
            this.path = path.ToArray();
            Ops = ops;
            Cost = cost;
            Incomplete = incomplete;
        }

        public override bool Equals(object? obj) => obj is SearchPathResult result && Equals(result);

        public bool Equals(SearchPathResult other)
            => EqualityComparer<Position[]>.Default.Equals(path, other.path)
            && Path.SequenceEqual(other.Path)
            && Ops == other.Ops
            && Cost == other.Cost
            && Incomplete == other.Incomplete;

        public override int GetHashCode()
        {
            var hashCode = HashCode.Combine(Ops, Cost, Incomplete);
            foreach (var position in path)
            {
                hashCode = HashCode.Combine(hashCode, position);
            }
            return hashCode;
        }

        public static bool operator ==(SearchPathResult left, SearchPathResult right) => left.Equals(right);

        public static bool operator !=(SearchPathResult left, SearchPathResult right) => !(left == right);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
    public readonly record struct Goal
    (
        Position Position,
        int Range = 0
    )
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Goal(Position pos) => new(pos);
    }

    public interface IPathFinder
    {
        /// <summary>
        /// Creates a new CostMatrix containing 0's for all positions.
        /// </summary>
        /// <returns></returns>
        ICostMatrix CreateCostMatrix();

        /// <summary>
        /// Find an optimal path between origin and goal.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="goal"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        SearchPathResult SearchPath(Position origin, Goal goal, SearchPathOptions? options = null);

        /// <summary>
        /// Find an optimal path between origin and the nearest goal.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="goals"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        SearchPathResult SearchPath(Position origin, ReadOnlySpan<Goal> goals, SearchPathOptions? options = null);
    }
}
