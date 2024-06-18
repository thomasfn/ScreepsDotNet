﻿using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;
using System.Linq;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeConstants : IConstants
    {
        #region Imports

        [JSImport("getConstantsObj", "game")]
        internal static partial JSObject Native_GetConstants();

        #endregion

        private readonly JSObject constantsObj;
        private readonly JSObject bodypartCostObj;
        private readonly JSObject structureCostObj;

        private readonly Dictionary<string, int> intCache = [];
        private readonly Dictionary<string, double> doubleCache = [];

        private readonly int?[] bodyPartCostCache = new int?[BodyPartTypes.Count];
        private int[]? rampartHitsMaxCache;

        private readonly Dictionary<Type, int> structureCostCache = [];
        private readonly Dictionary<ResourceType, int?> reactionTimeCache = [];
        private readonly HashSet<Name> obstacleObjectTypes = [];

        private ControllerConstants? controllerConstantsCache;
        private CreepConstants? creepConstantsCache;
        private Dictionary<(ResourceType, ResourceType), ResourceType>? reactionsCache;

        public ControllerConstants Controller
            => controllerConstantsCache ??= new(
                levels: InterpretArrayLikeLookupTableInt(constantsObj.GetPropertyAsJSObject("CONTROLLER_LEVELS")!, 1),
                structureCounts: InterpretMap(
                    constantsObj.GetPropertyAsJSObject("CONTROLLER_STRUCTURES")!,
                    (obj, key) => NativeRoomObjectTypes.GetTypeForStructureConstant(key)?.InterfaceType,
                    (obj, key) => InterpretArrayLikeLookupTableInt(obj.GetPropertyAsJSObject(key)!, 1)
                ),
                downgrade: InterpretArrayLikeLookupTableInt(constantsObj.GetPropertyAsJSObject("CONTROLLER_DOWNGRADE")!, 1),
                downgradeRestore: GetAsInt("CONTROLLER_DOWNGRADE_RESTORE"),
                downgradeSafemodeThreshold: GetAsInt("CONTROLLER_DOWNGRADE_SAFEMODE_THRESHOLD"),
                claimDowngrade: GetAsInt("CONTROLLER_CLAIM_DOWNGRADE"),
                reserve: GetAsInt("CONTROLLER_RESERVE"),
                reserveMax: GetAsInt("CONTROLLER_RESERVE_MAX"),
                maxUpgradePerTick: GetAsInt("CONTROLLER_MAX_UPGRADE_PER_TICK"),
                attackBlockedUpgrade: GetAsInt("CONTROLLER_ATTACK_BLOCKED_UPGRADE"),
                nukeBlockedUpgrade: GetAsInt("CONTROLLER_NUKE_BLOCKED_UPGRADE")
            );

        public CreepConstants Creep
            => creepConstantsCache ??= new(
                creepLifeTime: GetAsInt("CREEP_LIFE_TIME"),
                creepClaimLifeTime: GetAsInt("CREEP_CLAIM_LIFE_TIME"),
                creepCorpseRate: GetAsDouble("CREEP_CORPSE_RATE"),
                creepPartMaxEnergy: GetAsInt("CREEP_PART_MAX_ENERGY"),
                carryCapacity: GetAsInt("CARRY_CAPACITY"),
                harvestPower: GetAsInt("HARVEST_POWER"),
                harvestMineralPower: GetAsInt("HARVEST_MINERAL_POWER"),
                harvestDepositPower: GetAsInt("HARVEST_DEPOSIT_POWER"),
                repairPower: GetAsInt("REPAIR_POWER"),
                dismantlePower: GetAsInt("DISMANTLE_POWER"),
                buildPower: GetAsInt("BUILD_POWER"),
                attackPower: GetAsInt("ATTACK_POWER"),
                upgradeControllerPower: GetAsInt("UPGRADE_CONTROLLER_POWER"),
                rangedAttackPower: GetAsInt("RANGED_ATTACK_POWER"),
                healPower: GetAsInt("HEAL_POWER"),
                rangedHealPower: GetAsInt("RANGED_HEAL_POWER"),
                repairCost: GetAsDouble("REPAIR_COST"),
                dismantleCost: GetAsDouble("DISMANTLE_COST")
            );

        public IReadOnlyDictionary<(ResourceType, ResourceType), ResourceType> Reactions => reactionsCache ??= GetReactions();

        public NativeConstants()
        {
            constantsObj = Native_GetConstants();
            bodypartCostObj = constantsObj.GetPropertyAsJSObject("BODYPART_COST")!;
            structureCostObj = constantsObj.GetPropertyAsJSObject("CONSTRUCTION_COST")!;
        }

        public int GetBodyPartCost(BodyPartType bodyPartType)
            => bodyPartCostCache[(int)bodyPartType] ??= bodypartCostObj.GetPropertyAsInt32(bodyPartType.ToJS());

        public int GetConstructionCost<T>() where T : IStructure
            => GetConstructionCost(typeof(T));

        public int GetConstructionCost(Type structureType)
        {
            if (!structureType.IsAssignableTo(typeof(IStructure))) { throw new ArgumentException($"Must be valid structure type", nameof(structureType)); }
            if (structureCostCache.TryGetValue(structureType, out var result)) { return result; }
            var structureConstant = NativeRoomObjectTypes.GetTypeForInterfaceType(structureType)?.StructureConstant;
            if (structureConstant == null) { throw new ArgumentException($"Could not resolve structure constant for type", nameof(structureType)); }
            result = structureCostObj.GetPropertyAsInt32(structureConstant.Value);
            structureCostCache.Add(structureType, result);
            return result;
        }

        public int GetRampartHitsMax(int rcl)
            => (rampartHitsMaxCache ??= InterpretArrayLikeLookupTableInt(constantsObj.GetPropertyAsJSObject("RAMPART_HITS_MAX")!, 2))[rcl];

        public int? GetReactionTime(ResourceType product)
        {
            if (reactionTimeCache.TryGetValue(product, out var value)) { return value; }
            using var reactionTimeObj = constantsObj.GetPropertyAsJSObject("REACTION_TIME")!;
            int? reactionTime = reactionTimeObj.GetPropertyAsInt32(product.ToJS());
            if (reactionTime == 0) { reactionTime = null; }
            reactionTimeCache.Add(product, reactionTime);
            return reactionTime;
        }

        public int GetAsInt(string key)
        {
            if (intCache.TryGetValue(key, out var value)) { return value; }
            value = constantsObj.GetPropertyAsInt32(key);
            intCache.Add(key, value);
            return value;
        }

        public double GetAsDouble(string key)
        {
            if (doubleCache.TryGetValue(key, out var value)) { return value; }
            value = constantsObj.GetPropertyAsDouble(key);
            doubleCache.Add(key, value);
            return value;
        }

        public bool IsObjectObstacle<T>() where T : IRoomObject
        {
            CheckCacheObstacleObjectTypes();
            var structureConstant = NativeRoomObjectTypes.TypeOf<T>().StructureConstant;
            if (structureConstant == null) { return false; }
            return obstacleObjectTypes.Contains(structureConstant.Value);
        }

        public bool IsObjectObstacle(Type objectType)
        {
            CheckCacheObstacleObjectTypes();
            var structureConstant = NativeRoomObjectTypes.GetTypeForInterfaceType(objectType)?.StructureConstant;
            if (structureConstant == null) { return false; }
            return obstacleObjectTypes.Contains(structureConstant.Value);
        }

        private void CheckCacheObstacleObjectTypes()
        {
            if (obstacleObjectTypes.Count > 0) { return; }
            var arrObj = JSUtils.GetStringArrayOnObject(constantsObj, "OBSTACLE_OBJECT_TYPES");
            if (arrObj == null) { return; }
            foreach (var str in arrObj)
            {
                obstacleObjectTypes.Add(Name.Create(str));
            }
        }

        private Dictionary<(ResourceType, ResourceType), ResourceType> GetReactions()
        {
            var result = new Dictionary<(ResourceType, ResourceType), ResourceType>();
            using var reactionsObj = constantsObj.GetPropertyAsJSObject("REACTIONS")!;
            var topLevelKeys = reactionsObj.GetPropertyNamesAsNames();
            foreach (var res1Raw in topLevelKeys)
            {
                var res1 = res1Raw.ParseResourceType();
                using var subObj = reactionsObj.GetPropertyAsJSObject(res1Raw)!;
                var subKeys = subObj.GetPropertyNamesAsNames();
                foreach (var res2Raw in subKeys)
                {
                    var res2 = res2Raw.ParseResourceType();
                    var productRaw = subObj.GetPropertyAsName(res2Raw)!;
                    var product = productRaw.ParseResourceType();
                    result.Add((res1, res2), product);
                }
            }
            return result;
        }

        private double[] InterpretArrayLikeLookupTableDouble(JSObject obj, int firstIndex = 0)
        {
            var result = new List<double>();
            int i = 0;
            double? value;
            while ((value = obj.TryGetPropertyAsDouble(i.ToString())) != null || i < firstIndex)
            {
                result.Add(value ?? 0.0);
                ++i;
            }
            return result.ToArray();
        }

        private int[] InterpretArrayLikeLookupTableInt(JSObject obj, int firstIndex = 0)
        {
            var result = new List<int>();
            int i = 0;
            int? value;
            while ((value = obj.TryGetPropertyAsInt32(i.ToString())) != null || i < firstIndex)
            {
                result.Add(value ?? 0);
                ++i;
            }
            return result.ToArray();
        }

        private IReadOnlyDictionary<TKey, TValue> InterpretMap<TKey, TValue>(JSObject obj, Func<JSObject, string, TKey?> keySelector, Func<JSObject, string, TValue> valueSelector) where TKey : notnull
        {
            var dict = new Dictionary<TKey, TValue>();
            var keys = obj.GetPropertyNames();
            foreach (var key in keys)
            {
                var selectedKey = keySelector(obj, key);
                if (selectedKey == null) { continue; }
                dict.Add(selectedKey, valueSelector(obj, key));
            }
            return dict;
        }
    }
}
