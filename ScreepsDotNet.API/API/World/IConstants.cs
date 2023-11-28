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

    public readonly struct CreepConstants
    {
        public readonly int CreepLifeTime;
        public readonly int CreepClaimLifeTime;
        public readonly double CreepCorpseRate;
        public readonly int CreepPartMaxEnergy;

        public readonly int CarryCapacity;
        public readonly int HarvestPower;
        public readonly int HarvestMineralPower;
        public readonly int HarvestDepositPower;
        public readonly int RepairPower;
        public readonly int DismantlePower;
        public readonly int BuildPower;
        public readonly int AttackPower;
        public readonly int UpgradeControllerPower;
        public readonly int RangedAttackPower;
        public readonly int HealPower;
        public readonly int RangedHealPower;
        public readonly double RepairCost;
        public readonly double DismantleCost;

        public CreepConstants(
            int creepLifeTime, int creepClaimLifeTime, double creepCorpseRate, int creepPartMaxEnergy,
            int carryCapacity, int harvestPower, int harvestMineralPower, int harvestDepositPower, int repairPower, int dismantlePower, int buildPower, int attackPower, int upgradeControllerPower, int rangedAttackPower, int healPower, int rangedHealPower, double repairCost, double dismantleCost
        )
        {
            CreepLifeTime = creepLifeTime;
            CreepClaimLifeTime = creepClaimLifeTime;
            CreepCorpseRate = creepCorpseRate;
            CreepPartMaxEnergy = creepPartMaxEnergy;
            CarryCapacity = carryCapacity;
            HarvestPower = harvestPower;
            HarvestMineralPower = harvestMineralPower;
            HarvestDepositPower = harvestDepositPower;
            RepairPower = repairPower;
            DismantlePower = dismantlePower;
            BuildPower = buildPower;
            AttackPower = attackPower;
            UpgradeControllerPower = upgradeControllerPower;
            RangedAttackPower = rangedAttackPower;
            HealPower = healPower;
            RangedHealPower = rangedHealPower;
            RepairCost = repairCost;
            DismantleCost = dismantleCost;
        }
    }

    public interface IConstants
    {
        ControllerConstants Controller { get; }

        CreepConstants Creep { get; }

        IReadOnlyDictionary<(ResourceType, ResourceType), ResourceType> Reactions { get; }

        bool IsObjectObstacle<T>() where T : IRoomObject;

        bool IsObjectObstacle(Type objectType);

        int GetBodyPartCost(BodyPartType bodyPartType);

        int GetConstructionCost<T>() where T : IStructure;

        int GetConstructionCost(Type structureType);

        int GetRampartHitsMax(int rcl);

        int? GetReactionTime(ResourceType product);

        int GetAsInt(string key);

        double GetAsDouble(string key);
    }
}
