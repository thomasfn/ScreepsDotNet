using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeRuin : NativeRoomObjectWithId, IRuin
    {
        private readonly NativeStore store;

        private long? destroyTimeCache;
        private IStructure? structureCache;
        private int? ticksToDecayCache;

        public long DestroyTime => CacheLifetime(ref destroyTimeCache) ??= ProxyObject.GetPropertyAsInt32(Names.DestroyTime);

        public IStore Store => store;

        public IStructure? Structure => CacheLifetime(ref structureCache) ??= nativeRoot.GetOrCreateWrapperObject<IStructure>(ProxyObject.GetPropertyAsJSObject(Names.Store));

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);

        public NativeRuin(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
            store = new(nativeRoot, proxyObject);
        }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            store.ClearNativeCache();
            ticksToDecayCache = null;
        }

        protected override void OnGetNewProxyObject(JSObject newProxyObject)
        {
            base.OnGetNewProxyObject(newProxyObject);
            store.ProxyObject = newProxyObject;
        }

        public override string ToString()
            => $"Ruin[{(Exists ? RoomPosition.ToString() : "DEAD")}]";
    }
}
