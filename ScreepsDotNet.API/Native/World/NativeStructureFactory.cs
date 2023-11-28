using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureFactory : NativeOwnedStructure, IStructureFactory
    {
        #region Imports

        [JSImport("StructureFactory.produce", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Produce([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string resourceType);

        #endregion

        private int? cooldownCache;
        private int? levelCache;
        private NativeStore? storeCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32("cooldown");

        public int Level => CachePerTick(ref levelCache) ??= ProxyObject.GetPropertyAsInt32("level");

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureFactory(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            levelCache = null;
            storeCache = null;
        }

        public override string ToString()
            => $"StructureFactory[{Id}]";

        public FactoryProduceResult Produce(ResourceType resourceType)
            => (FactoryProduceResult)Native_Produce(ProxyObject, resourceType.ToJS());
    }
}
