using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureInvaderCore : NativeOwnedStructure, IStructureInvaderCore
    {
        private int? levelCache;
        private int? ticksToDeployCache;

        public int Level => CacheLifetime(ref levelCache) ??= ProxyObject.GetPropertyAsInt32("level");

        public int? TicksToDeploy => CachePerTick(ref ticksToDeployCache) ??= ProxyObject.GetTypeOfProperty("ticksToDeploy") == JSPropertyType.Number ? ProxyObject.GetPropertyAsInt32("ticksToDeploy") : null;

        public ISpawning? Spawning
        {
            get
            {
                var spawningObj = ProxyObject.GetPropertyAsJSObject("spawning");
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
