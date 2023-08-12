using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeConstants : IConstants
    {
        #region Imports

        [JSImport("getConstantsObj", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetConstants();

        #endregion

        private readonly JSObject constantsObj;
        private readonly JSObject bodypartCostObj;
        private readonly JSObject structureCostObj;

        private readonly IDictionary<string, int> intCache = new Dictionary<string, int>();
        private readonly IDictionary<string, double> doubleCache = new Dictionary<string, double>();

        private readonly int?[] bodyPartCostCache = new int?[Enum.GetValues<BodyPartType>().Length];
        private int[]? rampartHitsMaxCache;

        private readonly IDictionary<Type, int> structureCostCache = new Dictionary<Type, int>();

        private ControllerConstants? controllerConstantsCache;

        public ControllerConstants Controller
            => controllerConstantsCache ??= new(
                levels: InterpretArrayLikeLookupTableInt(constantsObj.GetPropertyAsJSObject("CONTROLLER_LEVELS")!, 1),
                structureCounts: InterpretMap(
                    constantsObj.GetPropertyAsJSObject("CONTROLLER_STRUCTURES")!,
                    (obj, key) => NativeRoomObjectUtils.GetInterfaceTypeForStructureConstant(key),
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
            var structureConstant = NativeRoomObjectUtils.GetStructureConstantForInterfaceType(structureType);
            if (string.IsNullOrEmpty(structureConstant)) { throw new ArgumentException($"Could not resolve structure constant for type", nameof(structureType)); }
            result = structureCostObj.GetPropertyAsInt32(structureConstant);
            structureCostCache.Add(structureType, result);
            return result;
        }

        public int GetRampartHitsMax(int rcl)
            => (rampartHitsMaxCache ??= InterpretArrayLikeLookupTableInt(constantsObj.GetPropertyAsJSObject("RAMPART_HITS_MAX")!, 2))[rcl];

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

        private double[] InterpretArrayLikeLookupTableDouble(JSObject obj, int firstIndex = 0)
        {
            var result = new List<double>();
            int i = 0;
            while (obj.HasProperty(i.ToString()) || i < firstIndex)
            {
                result.Add(obj.GetPropertyAsDouble(i.ToString()));
                ++i;
            }
            return result.ToArray();
        }

        private int[] InterpretArrayLikeLookupTableInt(JSObject obj, int firstIndex = 0)
        {
            var result = new List<int>();
            int i = 0;
            while (obj.HasProperty(i.ToString()) || i < firstIndex)
            {
                result.Add(obj.GetPropertyAsInt32(i.ToString()));
                ++i;
            }
            return result.ToArray();
        }

        private IReadOnlyDictionary<TKey, TValue> InterpretMap<TKey, TValue>(JSObject obj, Func<JSObject, string, TKey?> keySelector, Func<JSObject, string, TValue> valueSelector) where TKey : notnull
        {
            var dict = new Dictionary<TKey, TValue>();
            var keys = JSUtils.GetKeysOf(obj);
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
