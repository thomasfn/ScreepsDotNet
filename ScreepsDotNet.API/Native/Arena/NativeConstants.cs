using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeConstants : IConstants
    {
        #region Imports

        [JSImport("get", "game/constants")]
        
        internal static partial JSObject Native_GetConstants();

        #endregion

        private readonly JSObject constantsObj;
        private readonly JSObject bodypartCostObj;
        private readonly JSObject structureCostObj;

        private CreepConstants? creepCache;
        private TowerConstants? towerCache;
        private SourceConstants? sourceCache;
        private ResourceConstants? resourceCache;
        private ConstructionConstants? constructionCache;
        private ContainerConstants? containerCache;
        private WallConstants? wallCache;
        private RampartConstants? rampartCache;
        private RoadConstants? roadCache;
        private ExtensionConstants? extensionCache;
        private SpawnConstants? spawnCache;

        private int? bodyPartHitsCache;
        private readonly int?[] bodyPartCostCache = new int?[Enum.GetValues<BodyPartType>().Length];
        
        private readonly IDictionary<Type, int> structureCostCache = new Dictionary<Type, int>();
        private readonly ISet<Type> isObstacleSet = new HashSet<Type>
        {
            typeof(ICreep),
            typeof(IStructureTower),
            typeof(IStructureWall),
            typeof(IStructureSpawn),
        };

        public int BodyPartHits => bodyPartHitsCache ??= constantsObj.GetPropertyAsInt32("BODYPART_HITS");

        public CreepConstants Creep
            => creepCache ??= new CreepConstants(
                    constantsObj.GetPropertyAsInt32("RANGED_ATTACK_POWER"),
                    InterpretArrayLikeLookupTable(constantsObj.GetPropertyAsJSObject("RANGED_ATTACK_DISTANCE_RATE")!),
                    constantsObj.GetPropertyAsInt32("ATTACK_POWER"),
                    constantsObj.GetPropertyAsInt32("HEAL_POWER"),
                    constantsObj.GetPropertyAsInt32("RANGED_HEAL_POWER"),
                    constantsObj.GetPropertyAsInt32("CARRY_CAPACITY"),
                    constantsObj.GetPropertyAsInt32("REPAIR_POWER"),
                    constantsObj.GetPropertyAsInt32("DISMANTLE_POWER"),
                    constantsObj.GetPropertyAsDouble("REPAIR_COST"),
                    constantsObj.GetPropertyAsDouble("DISMANTLE_COST"),
                    constantsObj.GetPropertyAsInt32("HARVEST_POWER"),
                    constantsObj.GetPropertyAsInt32("BUILD_POWER"),
                    constantsObj.GetPropertyAsInt32("MAX_CREEP_SIZE"),
                    constantsObj.GetPropertyAsInt32("CREEP_SPAWN_TIME")
                );

        public TowerConstants Tower
            => towerCache ??= new TowerConstants(
                    constantsObj.GetPropertyAsInt32("TOWER_ENERGY_COST"),
                    constantsObj.GetPropertyAsInt32("TOWER_RANGE"),
                    constantsObj.GetPropertyAsInt32("TOWER_HITS"),
                    constantsObj.GetPropertyAsInt32("TOWER_CAPACITY"),
                    constantsObj.GetPropertyAsInt32("TOWER_POWER_ATTACK"),
                    constantsObj.GetPropertyAsInt32("TOWER_POWER_HEAL"),
                    constantsObj.GetPropertyAsInt32("TOWER_POWER_REPAIR"),
                    constantsObj.GetPropertyAsInt32("TOWER_OPTIMAL_RANGE"),
                    constantsObj.GetPropertyAsInt32("TOWER_FALLOFF_RANGE"),
                    constantsObj.GetPropertyAsDouble("TOWER_FALLOFF"),
                    constantsObj.GetPropertyAsInt32("TOWER_COOLDOWN")
                );

        public SourceConstants Source
            => sourceCache ??= new SourceConstants(
                    constantsObj.GetPropertyAsInt32("SOURCE_ENERGY_REGEN")
                );

        public ResourceConstants Resource
            => resourceCache ??= new ResourceConstants(
                    constantsObj.GetPropertyAsInt32("RESOURCE_DECAY")
                );

        public ConstructionConstants Construction
            => constructionCache ??= new ConstructionConstants(
                    constantsObj.GetPropertyAsInt32("MAX_CONSTRUCTION_SITES"),
                    constantsObj.GetPropertyAsInt32("CONSTRUCTION_COST_ROAD_SWAMP_RATIO"),
                    constantsObj.GetPropertyAsInt32("CONSTRUCTION_COST_ROAD_WALL_RATIO")
                );

        public ContainerConstants Container
            => containerCache ??= new ContainerConstants(
                    constantsObj.GetPropertyAsInt32("CONTAINER_HITS"),
                    constantsObj.GetPropertyAsInt32("CONTAINER_CAPACITY")
                );

        public WallConstants Wall
            => wallCache ??= new WallConstants(
                    constantsObj.GetPropertyAsInt32("WALL_HITS"),
                    constantsObj.GetPropertyAsInt32("WALL_HITS_MAX")
                );

        public RampartConstants Rampart
            => rampartCache ??= new RampartConstants(
                    constantsObj.GetPropertyAsInt32("RAMPART_HITS"),
                    constantsObj.GetPropertyAsInt32("RAMPART_HITS_MAX")
                );

        public RoadConstants Road
            => roadCache ??= new RoadConstants(
                    constantsObj.GetPropertyAsInt32("ROAD_HITS"),
                    constantsObj.GetPropertyAsInt32("ROAD_WEAROUT")
                );

        public ExtensionConstants Extension
            => extensionCache ??= new ExtensionConstants(
                    constantsObj.GetPropertyAsInt32("EXTENSION_HITS"),
                    constantsObj.GetPropertyAsInt32("EXTENSION_ENERGY_CAPACITY")
                );

        public SpawnConstants Spawn
            => spawnCache ??= new SpawnConstants(
                    constantsObj.GetPropertyAsInt32("SPAWN_ENERGY_CAPACITY"),
                    constantsObj.GetPropertyAsInt32("SPAWN_HITS")
                );

        public NativeConstants()
        {
            constantsObj = Native_GetConstants();
            bodypartCostObj = constantsObj.GetPropertyAsJSObject("BODYPART_COST")!;
            structureCostObj = constantsObj.GetPropertyAsJSObject("CONSTRUCTION_COST")!;
        }

        private double[] InterpretArrayLikeLookupTable(JSObject obj)
        {
            var result = new List<double>();
            int i = 0;
            while (obj.HasProperty(i.ToString()))
            {
                result.Add(obj.GetPropertyAsDouble(i.ToString()));
                ++i;
            }
            return result.ToArray();
        }

        public int GetBodyPartCost(BodyPartType bodyPartType)
            => bodyPartCostCache[(int)bodyPartType] ??= bodypartCostObj.GetPropertyAsInt32(bodyPartType.ToJS());

        public int GetConstructionCost<T>() where T : IStructure
            => GetConstructionCost(typeof(T));

        public int GetConstructionCost(Type structureType)
        {
            if (!structureType.IsAssignableTo(typeof(IStructure))) { throw new ArgumentException($"Must be valid structure type", nameof(structureType)); }
            if (structureCostCache.TryGetValue(structureType, out var result)) { return result; }
            result = structureCostObj.GetPropertyAsInt32(NativeGameObjectUtils.GetPrototypeName(structureType));
            structureCostCache.Add(structureType, result);
            return result;
        }

        public bool GetObjectIsObstacle<T>() where T : IGameObject
            => GetObjectIsObstacle(typeof(T));

        public bool GetObjectIsObstacle(Type objectType)
        {
            // The JS API for this is a bit weird. The property OBSTACLE_OBJECT_TYPES holds the following array:
            // [ 'creep', 'tower', 'constructedWall', 'spawn', 'extension', 'link' ]
            // These names don't map to any known consistent identifier - we're expecting prototype names like StructureTower
            // There are also references to things that don't exist like links - this list was probably copied from world and never changed
            // For now, let's just hard-code this, as it's HIGHLY unlikely that this will ever change
            return isObstacleSet.Contains(objectType);
        }
    }
}
