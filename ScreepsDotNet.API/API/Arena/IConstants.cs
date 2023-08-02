using System;

namespace ScreepsDotNet.API.Arena
{
    public readonly struct CreepConstants
    {
        public readonly int RangedAttackPower;
        private readonly double[] rangedAttackDistanceRate;
        public ReadOnlySpan<double> RangedAttackDistanceRate => rangedAttackDistanceRate ?? ReadOnlySpan<double>.Empty;
        public readonly int AttackPower;
        public readonly int HealPower;
        public readonly int RangedHealPower;
        public readonly int CarryCapacity;
        public readonly int RepairPower;
        public readonly int DismantlePower;
        public readonly double RepairCost;
        public readonly double DismantleCost;
        public readonly int HarvestPower;
        public readonly int BuildPower;
        public readonly int MaxSize;
        public readonly int SpawnTime;

        public CreepConstants(int rangedAttackPower, ReadOnlySpan<double> rangedAttackDistanceRate, int attackPower, int healPower, int rangedHealPower, int carryCapacity, int repairPower, int dismantlePower, double repairCost, double dismantleCost, int harvestPower, int buildPower, int maxSize, int spawnTime)
        {
            RangedAttackPower = rangedAttackPower;
            this.rangedAttackDistanceRate = rangedAttackDistanceRate.ToArray();
            AttackPower = attackPower;
            HealPower = healPower;
            RangedHealPower = rangedHealPower;
            CarryCapacity = carryCapacity;
            RepairPower = repairPower;
            DismantlePower = dismantlePower;
            RepairCost = repairCost;
            DismantleCost = dismantleCost;
            HarvestPower = harvestPower;
            BuildPower = buildPower;
            MaxSize = maxSize;
            SpawnTime = spawnTime;
        }
    }

    public readonly struct TowerConstants
    {
        public readonly int EnergyCost;
        public readonly int Range;
        public readonly int Hits;
        public readonly int Capacity;
        public readonly int PowerAttack;
        public readonly int PowerHeal;
        public readonly int PowerRepair;
        public readonly int OptimalRange;
        public readonly int FalloffRange;
        public readonly double Falloff;
        public readonly int Cooldown;

        public TowerConstants(int energyCost, int range, int hits, int capacity, int powerAttack, int powerHeal, int powerRepair, int optimalRange, int falloffRange, double falloff, int cooldown)
        {
            EnergyCost = energyCost;
            Range = range;
            Hits = hits;
            Capacity = capacity;
            PowerAttack = powerAttack;
            PowerHeal = powerHeal;
            PowerRepair = powerRepair;
            OptimalRange = optimalRange;
            FalloffRange = falloffRange;
            Falloff = falloff;
            Cooldown = cooldown;
        }
    }

    public readonly struct SourceConstants
    {
        public readonly int EnergyRegen;

        public SourceConstants(int energyRegen)
        {
            EnergyRegen = energyRegen;
        }
    }

    public readonly struct ResourceConstants
    {
        public readonly int Decay;

        public ResourceConstants(int decay)
        {
            Decay = decay;
        }
    }

    public readonly struct ConstructionConstants
    {
        public readonly int MaxConstructionSites;
        public readonly int ConstructionCostRoadSwampRatio;
        public readonly int ConstructionCostRoadWallRatio;

        public ConstructionConstants(int maxConstructionSites, int constructionCostRoadSwampRatio, int constructionCostRoadWallRatio)
        {
            MaxConstructionSites = maxConstructionSites;
            ConstructionCostRoadSwampRatio = constructionCostRoadSwampRatio;
            ConstructionCostRoadWallRatio = constructionCostRoadWallRatio;
        }
    }

    public readonly struct ContainerConstants
    {
        public readonly int Hits;
        public readonly int Capacity;

        public ContainerConstants(int hits, int capacity)
        {
            Hits = hits;
            Capacity = capacity;
        }
    }

    public readonly struct WallConstants
    {
        public readonly int Hits;
        public readonly int HitsMax;

        public WallConstants(int hits, int hitsMax)
        {
            Hits = hits;
            HitsMax = hitsMax;
        }
    }

    public readonly struct RampartConstants
    {
        public readonly int Hits;
        public readonly int HitsMax;

        public RampartConstants(int hits, int hitsMax)
        {
            Hits = hits;
            HitsMax = hitsMax;
        }
    }

    public readonly struct RoadConstants
    {
        public readonly int Hits;
        public readonly int Wearout;

        public RoadConstants(int hits, int wearout)
        {
            Hits = hits;
            Wearout = wearout;
        }
    }

    public readonly struct ExtensionConstants
    {
        public readonly int Hits;
        public readonly int EnergyCapacity;

        public ExtensionConstants(int hits, int energyCapacity)
        {
            Hits = hits;
            EnergyCapacity = energyCapacity;
        }
    }

    public readonly struct SpawnConstants
    {
        public readonly int EnergyCapacity;
        public readonly int Hits;

        public SpawnConstants(int energyCapacity, int hits)
        {
            EnergyCapacity = energyCapacity;
            Hits = hits;
        }
    }

    public interface IConstants
    {
        int BodyPartHits { get; }

        CreepConstants Creep { get; }

        TowerConstants Tower { get; }

        SourceConstants Source { get; }

        ResourceConstants Resource { get; }

        ConstructionConstants Construction { get; }

        ContainerConstants Container { get; }

        WallConstants Wall { get; }

        RampartConstants Rampart { get; }

        RoadConstants Road { get; }

        ExtensionConstants Extension { get; }

        SpawnConstants Spawn { get; }

        int GetBodyPartCost(BodyPartType bodyPartType);

        int GetConstructionCost<T>() where T : IStructure;

        int GetConstructionCost(Type structureType);

        bool GetObjectIsObstacle<T>() where T : IGameObject;

        bool GetObjectIsObstacle(Type objectType);
    }
}
