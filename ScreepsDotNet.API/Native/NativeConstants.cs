using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeConstants : IConstants
    {
        #region Imports

        [JSImport("get", "game/constants")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetConstants();

        #endregion

        private readonly JSObject constantsObj;
        private readonly JSObject bodypartCostObj;

        private int? bodyPartHitsCache;
        private readonly int?[] bodyPartCostCache = new int?[Enum.GetValues<BodyPartType>().Length];

        public int BodyPartHits => bodyPartHitsCache ??= constantsObj.GetPropertyAsInt32("BODYPART_HITS");

        public NativeConstants()
        {
            constantsObj = Native_GetConstants();
            bodypartCostObj = constantsObj.GetPropertyAsJSObject("BODYPART_COST")!;
        }

        public int GetBodyPartCost(BodyPartType bodyPartType)
            => bodyPartCostCache[(int)bodyPartType] ??= bodypartCostObj.GetPropertyAsInt32(bodyPartType.ToJS());
    }
}
