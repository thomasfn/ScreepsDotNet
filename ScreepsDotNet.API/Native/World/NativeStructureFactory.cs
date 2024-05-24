using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureFactory : NativeOwnedStructureWithStore, IStructureFactory
    {
        #region Imports

        [JSImport("StructureFactory.produce", "game/prototypes/wrapped")]
        
        internal static partial int Native_Produce(JSObject proxyObject, Name resourceType);

        #endregion

        private int? cooldownCache;
        private int? levelCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32(Names.Cooldown);

        public int Level => CachePerTick(ref levelCache) ??= ProxyObject.GetPropertyAsInt32(Names.Level);

        public NativeStructureFactory(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            levelCache = null;
        }

        public FactoryProduceResult Produce(ResourceType resourceType)
            => (FactoryProduceResult)Native_Produce(ProxyObject, resourceType.ToJS());

        public override string ToString()
            => $"StructureFactory[{Id}]";
    }
}
