using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructure : NativeGameObject, IStructure
    {
        private int? hitsCache;
        private int? hitsMaxCache;

        public int? Hits => CachePerTick(ref hitsCache) ??= proxyObject.TryGetPropertyAsInt32(Names.Hits);

        public int? HitsMax => CacheLifetime(ref hitsMaxCache) ??= proxyObject.TryGetPropertyAsInt32(Names.HitsMax);

        public NativeStructure(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, false)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            hitsCache = null;
        }

        public override string ToString()
            => Exists ? $"Structure({Id}, {Position})" : "Structure(DEAD)";
    }
}
