using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public readonly struct ControllerConstants
    {
        private readonly int[] levels;
        
        private readonly IReadOnlyDictionary<Type, int[]> structureCounts;
        private readonly int[] downgrade;
        
        public readonly int DowngradeRestore;
        public readonly int DowngradeSafemodeThreshold;
        public readonly int ClaimDowngrade;
        public readonly int Reserve;
        public readonly int ReserveMax;
        public readonly int MaxUpgradePerTick;
        public readonly int AttackBlockedUpgrade;
        public readonly int NukeBlockedUpgrade;

        public ReadOnlySpan<int> Levels => levels ?? ReadOnlySpan<int>.Empty;

        public ReadOnlySpan<int> Downgrade => downgrade ?? ReadOnlySpan<int>.Empty;

        public ControllerConstants(ReadOnlySpan<int> levels, IReadOnlyDictionary<Type, int[]> structureCounts, int[] downgrade, int downgradeRestore, int downgradeSafemodeThreshold, int claimDowngrade, int reserve, int reserveMax, int maxUpgradePerTick, int attackBlockedUpgrade, int nukeBlockedUpgrade)
        {
            this.levels = levels.ToArray();
            this.structureCounts = structureCounts;
            this.downgrade = downgrade;
            DowngradeRestore = downgradeRestore;
            DowngradeSafemodeThreshold = downgradeSafemodeThreshold;
            ClaimDowngrade = claimDowngrade;
            Reserve = reserve;
            ReserveMax = reserveMax;
            MaxUpgradePerTick = maxUpgradePerTick;
            AttackBlockedUpgrade = attackBlockedUpgrade;
            NukeBlockedUpgrade = nukeBlockedUpgrade;
        }

        public int GetMaxStructureCount<T>(int controllerLevel) where T : IStructure
            => GetMaxStructureCount(typeof(T), controllerLevel);

        public int GetMaxStructureCount(Type structureType, int controllerLevel)
            => structureCounts.TryGetValue(structureType, out var arr) ? arr[controllerLevel] : 0;
    }

    public interface IConstants
    {
        ControllerConstants Controller { get; }

        int GetBodyPartCost(BodyPartType bodyPartType);

        int GetConstructionCost<T>() where T : IStructure;

        int GetConstructionCost(Type structureType);

        int GetRampartHitsMax(int rcl);

        int GetAsInt(string key);

        double GetAsDouble(string key);
    }
}
