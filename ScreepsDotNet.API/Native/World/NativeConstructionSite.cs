using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeConstructionSite : NativeRoomObjectWithId, IConstructionSite
    {
        #region Imports

        [JSImport("ConstructionSite.remove", "game/prototypes/wrapped")]
        internal static partial int Native_Remove(JSObject proxyObject);

        #endregion

        private bool? myCache;
        private OwnerInfo? ownerInfoCache;
        private Type? structureTypeCache;
        private int? progressCache;
        private int? progressTotalCache;

        public bool My => CacheLifetime(ref myCache) ??= ProxyObject.GetPropertyAsBoolean(Names.My);

        public OwnerInfo Owner => CacheLifetime(ref ownerInfoCache) ??= new(ProxyObject.GetPropertyAsJSObject(Names.Owner)!.GetPropertyAsString(Names.Username)!);

        public int Progress => CachePerTick(ref progressCache) ??= ProxyObject.GetPropertyAsInt32(Names.Progress);

        public int ProgressTotal => CachePerTick(ref progressTotalCache) ??= ProxyObject.GetPropertyAsInt32(Names.ProgressTotal);

        public Type StructureType => CacheLifetime(ref structureTypeCache) ??= (NativeRoomObjectTypes.GetTypeForStructureConstant(ProxyObject.GetPropertyAsString(Names.StructureType)!) ?? NativeRoomObjectTypes.TypeOf<IStructure>()).InterfaceType;

        public NativeConstructionSite(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            progressCache = null;
            progressTotalCache = null;
        }

        public void Remove()
            => Native_Remove(ProxyObject);

        public override string ToString()
            => $"ConstructionSite[{Id}]";
    }
}
