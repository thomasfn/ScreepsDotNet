using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeConstructionSite : NativeRoomObject, IConstructionSite, IEquatable<NativeConstructionSite?>
    {
        #region Imports

        [JSImport("ConstructionSite.remove", "game/prototypes/wrapped")]
        
        internal static partial int Native_Remove(JSObject proxyObject);

        #endregion

        private readonly ObjectId id;

        private bool? myCache;
        private OwnerInfo? ownerInfoCache;
        private Type? structureTypeCache;
        private int? progressCache;
        private int? progressTotalCache;

        public ObjectId Id => id;

        public bool My => CacheLifetime(ref myCache) ??= ProxyObject.GetPropertyAsBoolean("my");

        public OwnerInfo Owner => CacheLifetime(ref ownerInfoCache) ??= new(ProxyObject.GetPropertyAsJSObject("owner")!.GetPropertyAsString("username")!);

        public int Progress => CachePerTick(ref progressCache) ??= ProxyObject.GetPropertyAsInt32("progress");

        public int ProgressTotal => CachePerTick(ref progressTotalCache) ??= ProxyObject.GetPropertyAsInt32("progressTotal");

        public Type StructureType => CacheLifetime(ref structureTypeCache) ??= (NativeRoomObjectUtils.GetInterfaceTypeForStructureConstant(ProxyObject.GetPropertyAsString("structureType")!) ?? typeof(IStructure));

        public NativeConstructionSite(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override void UpdateFromDataPacket(RoomObjectDataPacket dataPacket)
        {
            base.UpdateFromDataPacket(dataPacket);
            progressCache = dataPacket.Hits;
            progressTotalCache = dataPacket.HitsMax;
            myCache = dataPacket.My;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            progressCache = null;
            progressTotalCache = null;
        }

        public void Remove()
            => Native_Remove(ProxyObject);

        public override bool Equals(object? obj) => Equals(obj as NativeConstructionSite);

        public bool Equals(NativeConstructionSite? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeConstructionSite? left, NativeConstructionSite? right) => EqualityComparer<NativeConstructionSite>.Default.Equals(left, right);

        public static bool operator !=(NativeConstructionSite? left, NativeConstructionSite? right) => !(left == right);

        public override string ToString()
            => $"ConstructionSite[{id}]";
    }
}
