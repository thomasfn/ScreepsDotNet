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

        public NativeConstants()
        {
            constantsObj = Native_GetConstants();
            bodypartCostObj = constantsObj.GetPropertyAsJSObject("BODYPART_COST")!;
        }

        public int GetBodyPartCost(BodyPartType bodyPartType)
            => bodypartCostObj.GetPropertyAsInt32(bodyPartType.ToJS());
    }
}
