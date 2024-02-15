using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureKeeperLair : NativeOwnedStructure, IStructureKeeperLair
    {
        private int? ticksToSpawnCache;

        public int TicksToSpawn => CachePerTick(ref ticksToSpawnCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToSpawn);

        public NativeStructureKeeperLair(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            ticksToSpawnCache = null;
        }

        public override string ToString()
            => $"StructureKeeperLair[{Id}]";
    }
}
