using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeMineral : NativeRoomObjectWithId, IMineral
    {
        private int? densityCache;
        private int? mineralAmountCache;
        private ResourceType? mineralTypeCache;
        private int? ticksToRegenerationCache;

        public int Density => CacheLifetime(ref densityCache) ??= ProxyObject.GetPropertyAsInt32(Names.Density);

        public int MineralAmount => CachePerTick(ref mineralAmountCache) ??= ProxyObject.GetPropertyAsInt32(Names.MineralAmount);

        public ResourceType MineralType => CacheLifetime(ref mineralTypeCache) ??= ProxyObject.GetPropertyAsName(Names.MineralType).ParseResourceType();

        public int? TicksToRegeneration => CachePerTick(ref ticksToRegenerationCache) ??= ProxyObject.TryGetPropertyAsInt32(Names.TicksToRegeneration);

        public NativeMineral(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            mineralAmountCache = null;
            ticksToRegenerationCache = null;
        }

        public override string ToString()
            => $"Mineral[{RoomPosition}]";
    }
}
