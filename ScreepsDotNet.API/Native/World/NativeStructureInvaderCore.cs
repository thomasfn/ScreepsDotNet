using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureInvaderCore : NativeOwnedStructure, IStructureInvaderCore
    {
        private int? levelCache;
        private int? ticksToDeployCache;

        public int Level => CacheLifetime(ref levelCache) ??= ProxyObject.GetPropertyAsInt32(Names.Level);

        public int? TicksToDeploy => CachePerTick(ref ticksToDeployCache) ??= ProxyObject.TryGetPropertyAsInt32(Names.TicksToDeploy);

        public ISpawning? Spawning
        {
            get
            {
                using var spawningObj = ProxyObject.GetPropertyAsJSObject(Names.Spawning);
                if (spawningObj == null) { return null; }
                return new NativeSpawning(nativeRoot, spawningObj);
            }
        }

        public NativeStructureInvaderCore(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            ticksToDeployCache = null;
        }

        public override string ToString()
            => $"StructureInvaderCore[{Id}]";
    }
}
