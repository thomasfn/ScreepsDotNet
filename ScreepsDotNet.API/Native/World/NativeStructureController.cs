using System;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureController : NativeOwnedStructure, IStructureController
    {
        #region Imports

        [JSImport("StructureController.activateSafeMode", "game/prototypes/wrapped")]
        
        internal static partial int Native_ActivateSafeMode(JSObject proxyObject);

        [JSImport("StructureController.unclaim", "game/prototypes/wrapped")]
        
        internal static partial int Native_Unclaim(JSObject proxyObject);

        #endregion

        private bool? isPowerEnabledCache;
        private int? levelCache;
        private int? progressCache;
        private int? progressTotalCache;
        private ControllerReservation? reservationCache;
        private int? safeModeCache;
        private int? safeModeAvailableCache;
        private int? safeModeCooldownCache;
        private ControllerSign? signCache;
        private int? ticksToDowngradeCache;
        private int? upgradeBlockedCache;

        public bool IsPowerEnabled => CachePerTick(ref isPowerEnabledCache) ??= ProxyObject.GetPropertyAsBoolean(Names.IsPowerEnabled);

        public int Level => CachePerTick(ref levelCache) ??= ProxyObject.GetPropertyAsInt32(Names.Level);

        public int Progress => CachePerTick(ref progressCache) ??= ProxyObject.GetPropertyAsInt32(Names.Progress);

        public int ProgressTotal => CachePerTick(ref progressTotalCache) ??= ProxyObject.GetPropertyAsInt32(Names.ProgressTotal);

        public ControllerReservation? Reservation => CachePerTick(ref reservationCache) ??= FetchReservation();

        public int? SafeMode => CachePerTick(ref safeModeCache) ??= ProxyObject.TryGetPropertyAsInt32(Names.SafeMode);

        public int SafeModeAvailable => CachePerTick(ref safeModeAvailableCache) ??= ProxyObject.GetPropertyAsInt32(Names.SafeModeAvailable);

        public int? SafeModeCooldown => CachePerTick(ref safeModeCooldownCache) ??= ProxyObject.TryGetPropertyAsInt32(Names.SafeModeCooldown);

        public ControllerSign? Sign => CachePerTick(ref signCache) ?? FetchSign();

        public int TicksToDowngrade => CachePerTick(ref ticksToDowngradeCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDowngrade);

        public int UpgradeBlocked => CachePerTick(ref upgradeBlockedCache) ??= ProxyObject.GetPropertyAsInt32(Names.UpgradeBlocked);

        public NativeStructureController(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, ownershipCanChange: true)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            isPowerEnabledCache = null;
            levelCache = null;
            progressCache = null;
            progressTotalCache = null;
            reservationCache = null;
            safeModeCache = null;
            safeModeAvailableCache = null;
            safeModeCooldownCache = null;
            signCache = null;
            ticksToDowngradeCache = null;
            upgradeBlockedCache = null;
    }

        public ControllerActivateSafeModeResult ActivateSafeMode()
            => (ControllerActivateSafeModeResult)Native_ActivateSafeMode(ProxyObject);

        public ControllerUnclaimResult Unclaim()
            => (ControllerUnclaimResult)Native_Unclaim(ProxyObject);

        private ControllerReservation? FetchReservation()
        {
            var obj = ProxyObject.GetPropertyAsJSObject(Names.Reservation);
            if (obj == null) { return null; }
            return new(obj.GetPropertyAsString(Names.Username)!, obj.GetPropertyAsInt32(Names.TicksToEnd));
        }

        private ControllerSign? FetchSign()
        {
            using var obj = ProxyObject.GetPropertyAsJSObject(Names.Sign);
            if (obj == null) { return null; }
            return new(obj.GetPropertyAsString(Names.Username)!, obj.GetPropertyAsString(Names.Text)!, obj.GetPropertyAsInt32(Names.Time), obj.GetPropertyAsJSObject(Names.Datetime)!.ToDateTime());
        }

        public override string ToString()
            => $"StructureController[{Id}]";
    }
}
