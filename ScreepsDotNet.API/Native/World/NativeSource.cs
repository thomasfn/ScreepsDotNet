using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeSource : NativeRoomObjectWithId, ISource
    {
        private int? energyCache;
        private int? energyCapacityCache;
        private int? ticksToRegenerationCache;

        public int Energy => CachePerTick(ref energyCache) ??= ProxyObject.GetPropertyAsInt32(Names.Energy);

        public int EnergyCapacity => CachePerTick(ref energyCapacityCache) ??= ProxyObject.GetPropertyAsInt32(Names.EnergyCapacity);

        public int TicksToRegeneration => CachePerTick(ref ticksToRegenerationCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToRegeneration);

        public NativeSource(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            energyCache = null;
            energyCapacityCache = null;
            ticksToRegenerationCache = null;
        }

        public override string ToString()
            => $"Source[{RoomPosition}]";
    }
}
