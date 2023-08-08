using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureController : NativeOwnedStructure, IStructureController
    {
        #region Imports

        [JSImport("StructureController.activateSafeMode", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_ActivateSafeMode([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("StructureController.unclaim", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Unclaim([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        #endregion

        public bool IsPowerEnabled => ProxyObject.GetPropertyAsBoolean("isPowerEnabled");

        public int Level => ProxyObject.GetPropertyAsInt32("level");

        public int Progress => ProxyObject.GetPropertyAsInt32("progress");

        public int ProgressTotal => ProxyObject.GetPropertyAsInt32("progressTotal");

        public ControllerReservation? Reservation
        {
            get
            {
                var obj = ProxyObject.GetPropertyAsJSObject("reservation");
                if (obj == null) { return null; }
                return new(obj.GetPropertyAsString("username")!, obj.GetPropertyAsInt32("ticksToEnd"));
            }
        }

        public int? SafeMode => ProxyObject.GetTypeOfProperty("safeMode") == "number" ? ProxyObject.GetPropertyAsInt32("safeMode") : null;

        public int SafeModeAvailable => ProxyObject.GetPropertyAsInt32("safeModeAvailable");

        public int? SafeModeCooldown => ProxyObject.GetTypeOfProperty("safeModeCooldown") == "number" ? ProxyObject.GetPropertyAsInt32("safeModeCooldown") : null;

        public ControllerSign? Sign
        {
            get
            {
                var obj = ProxyObject.GetPropertyAsJSObject("sign");
                if (obj == null) { return null; }
                return new(obj.GetPropertyAsString("username")!, obj.GetPropertyAsString("text")!, obj.GetPropertyAsInt32("time"), obj.GetPropertyAsJSObject("dateTime")!.ToDateTime());
            }
        }

        public int TicksToDowngrade => ProxyObject.GetPropertyAsInt32("ticksToDowngrade");

        public int UpgradeBlocked => ProxyObject.GetPropertyAsInt32("upgradeBlocked");

        public NativeStructureController(INativeRoot nativeRoot, JSObject proxyObject, string knownId)
            : base(nativeRoot, proxyObject, knownId)
        { }

        public ControllerActivateSafeModeResult ActivateSafeMode()
            => (ControllerActivateSafeModeResult)Native_ActivateSafeMode(ProxyObject);

        public ControllerUnclaimResult Unclaim()
            => (ControllerUnclaimResult)Native_Unclaim(ProxyObject);

        public override string ToString()
            => $"StructureController[{Id}]";
    }
}
