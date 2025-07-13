using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeScoreCollector : NativeRoomObjectWithId, IScoreCollector
    {
        private readonly NativeStore store;

        public IStore Store => store;

        public NativeScoreCollector(INativeRoot nativeRoot, JSObject proxyObject)
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
            => $"ScoreCollector[{Id}]";
    }
}
