using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeResource : NativeRoomObjectWithId, IResource
    {
        private int? amountCache;
        private ResourceType? resourceTypeCache;

        public int Amount => CachePerTick(ref amountCache) ??= ProxyObject.GetPropertyAsInt32(Names.Amount);

        public ResourceType ResourceType => CacheLifetime(ref resourceTypeCache) ??= ProxyObject.GetPropertyAsName(Names.ResourceType)!.ParseResourceType();

        public NativeResource(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            amountCache = null;
        }

        public override string ToString()
            => $"Resource[{Id}]";
    }
}
