using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeScoreContainer : NativeRoomObjectWithId, IScoreContainer
    {
        private readonly NativeStore store;

        private int? ticksToDecayCache;

        public IStore Store => store;

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);

        public NativeScoreContainer(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
            store = new NativeStore(nativeRoot, proxyObject);
            store.OnRequestNewProxyObject += Store_OnRequestNewProxyObject;
        }

        private void Store_OnRequestNewProxyObject()
        {
            TouchProxyObject();
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

        protected override void OnRenewProxyObject()
        {
            base.OnRenewProxyObject();
            store.RenewProxyObject();
        }

        public override string ToString()
            => $"ScoreContainer[{Id}]";
    }
}
