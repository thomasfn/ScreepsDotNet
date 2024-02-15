﻿using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureFactory : NativeOwnedStructure, IStructureFactory
    {
        #region Imports

        [JSImport("StructureFactory.produce", "game/prototypes/wrapped")]
        
        internal static partial int Native_Produce(JSObject proxyObject, Name resourceType);

        #endregion

        private int? cooldownCache;
        private int? levelCache;
        private NativeStore? storeCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32(Names.Cooldown);

        public int Level => CachePerTick(ref levelCache) ??= ProxyObject.GetPropertyAsInt32(Names.Level);

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject(Names.Store));

        public NativeStructureFactory(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            levelCache = null;
            storeCache?.Dispose();
            storeCache = null;
        }

        public override string ToString()
            => $"StructureFactory[{Id}]";

        public FactoryProduceResult Produce(ResourceType resourceType)
            => (FactoryProduceResult)Native_Produce(ProxyObject, resourceType.ToJS());
    }
}
