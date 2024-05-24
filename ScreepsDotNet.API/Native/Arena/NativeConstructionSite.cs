using System.Runtime.CompilerServices;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeConstructionSite : NativeGameObject, IConstructionSite
    {
        #region Imports

        [JSImport("ConstructionSite.remove", "game/prototypes/wrapped")]
        internal static partial int Native_Remove(JSObject proxyObject);

        #endregion

        private int? progressCache;
        private int? progressTotalCache;
        private IStructure? structureCache;
        private bool? myCache;

        public int Progress => CachePerTick(ref progressCache) ??= proxyObject.GetPropertyAsInt32(Names.Progress);

        public int ProgressTotal => CachePerTick(ref progressTotalCache) ??= proxyObject.GetPropertyAsInt32(Names.ProgressTotal);

        public IStructure? Structure => CacheLifetime(ref structureCache) ??= FetchStructure();

        public bool My => CacheLifetime(ref myCache) ??= proxyObject.GetPropertyAsBoolean(Names.My);

        public NativeConstructionSite(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, false)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            progressCache = null;
            progressTotalCache = null;
        }

        public void Remove()
            => Native_Remove(proxyObject);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IStructure? FetchStructure()
        {
            var obj = proxyObject.GetPropertyAsJSObject(Names.Structure);
            if (obj == null) { return null; }
            return nativeRoot.GetOrCreateWrapperForObject<NativeStructure>(obj);
        }

        public override string ToString()
            => Exists ? $"ConstructionSite({Id}, {Position})" : $"ConstructionSite(DEAD)";
    }
}
