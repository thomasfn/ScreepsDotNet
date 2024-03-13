using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureRampart : NativeOwnedStructure, IStructureRampart
    {
        #region Imports

        [JSImport("StructureRampart.setPublic", "game/prototypes/wrapped")]
        
        internal static partial int Native_SetPublic(JSObject proxyObject, bool isPublic);

        #endregion

        private bool? isPublicCache;
        private int? ticksToDecayCache;

        public bool IsPublic => CachePerTick(ref isPublicCache) ??= ProxyObject.GetPropertyAsBoolean(Names.IsPublic);

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);

        public NativeStructureRampart(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            isPublicCache = null;
            ticksToDecayCache = null;
        }

        public RampartSetPublicResult SetPublic(bool isPublic)
            => (RampartSetPublicResult)Native_SetPublic(ProxyObject, isPublic);

        public override string ToString()
            => $"StructureRampart[{Id}]";
    }
}
