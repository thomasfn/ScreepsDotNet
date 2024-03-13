using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeTombstone : NativeRoomObjectWithId, ITombstone
    {
        private readonly NativeStore store;

        private int? deathTimeCache;
        private int? ticksToDecayCache;

        public ICreep? Creep => nativeRoot.GetOrCreateWrapperObject<ICreep>(ProxyObject.GetPropertyAsJSObject(Names.Creep));

        public int DeathTime => CacheLifetime(ref deathTimeCache) ??= ProxyObject.GetPropertyAsInt32(Names.DeathTime);

        public IStore Store => store;

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);

        public NativeTombstone(INativeRoot nativeRoot, JSObject proxyObject)
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
            => $"Tombstone[{Id}]";
    }
}
