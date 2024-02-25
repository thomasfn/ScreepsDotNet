using ScreepsDotNet.Interop;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeResource : NativeGameObject, IResource
    {
        private int? amountCache;
        private ResourceType? resourceTypeCache;

        public int Amount => CachePerTick(ref amountCache) ??= proxyObject.GetPropertyAsInt32(Names.Amount);

        public ResourceType ResourceType => CacheLifetime(ref resourceTypeCache) ??= proxyObject.GetPropertyAsName(Names.ResourceType)!.ParseResourceType();

        public NativeResource(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, false)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            amountCache = null;
        }

        public override string ToString()
            => Exists ? $"Resource({Id}, {Position})" : "Resource(DEAD)";
    }
}
