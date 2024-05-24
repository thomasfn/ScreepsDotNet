using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeSource : NativeGameObject, ISource
    {
        private int? energyCache;
        private int? energyCapacityCache;

        public int Energy => CachePerTick(ref energyCache) ??= proxyObject.GetPropertyAsInt32(Names.Energy);

        public int EnergyCapacity => CacheLifetime(ref energyCapacityCache) ??= proxyObject.GetPropertyAsInt32(Names.EnergyCapacity);

        public NativeSource(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, false)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            energyCache = null;
        }

        public override string ToString()
            => $"Source({Id}, {Position})";
    }
}
