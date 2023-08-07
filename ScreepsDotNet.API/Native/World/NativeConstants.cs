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

        private readonly IDictionary<Type, int> structureCostCache = new Dictionary<Type, int>();

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
    }
}
