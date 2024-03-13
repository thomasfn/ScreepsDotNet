using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeNuke : NativeRoomObjectWithId, INuke
    {
        private RoomCoord? launchRoomCoordCache;
        private int? timeToLandCache;

        public RoomCoord LaunchRoomCoord => CacheLifetime(ref launchRoomCoordCache) ??= new(ProxyObject.GetPropertyAsString(Names.LaunchRoomName)!);

        public int TimeToLand => CachePerTick(ref timeToLandCache) ??= ProxyObject.GetPropertyAsInt32(Names.TimeToLand);

        public NativeNuke(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            timeToLandCache = null;
        }

        public override string ToString()
            => $"Nuke[{(Exists ? RoomPosition.ToString() : "DEAD")}]";
    }
}
