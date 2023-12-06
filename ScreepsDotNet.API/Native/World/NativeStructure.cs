using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructure : NativeRoomObject, IStructure, IEquatable<NativeStructure?>
    {
        #region Imports

        [JSImport("Structure.destroy", "game/prototypes/wrapped")]
        
        internal static partial int Native_Destroy(JSObject proxyObject);

        [JSImport("Structure.isActive", "game/prototypes/wrapped")]
        
        internal static partial bool Native_IsActive(JSObject proxyObject);

        [JSImport("Structure.notifyWhenAttacked", "game/prototypes/wrapped")]
        
        internal static partial int Native_NotifyWhenAttacked(JSObject proxyObject, bool enabled);

        #endregion

        private readonly ObjectId id;

        private int? hitsCache;
        private int? hitsMaxCache;

        public int Hits => CachePerTick(ref hitsCache) ??= ProxyObject.GetPropertyAsInt32("hits");

        public int HitsMax => CachePerTick(ref hitsMaxCache) ??= ProxyObject.GetPropertyAsInt32("hitsMax");

        public ObjectId Id => id;

        public NativeStructure(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override void UpdateFromDataPacket(RoomObjectDataPacket dataPacket)
        {
            base.UpdateFromDataPacket(dataPacket);
            hitsCache = dataPacket.Hits;
            hitsMaxCache = dataPacket.HitsMax;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            hitsCache = null;
            hitsMaxCache = null;
        }

        public StructureDestroyResult Destroy()
            => (StructureDestroyResult)Native_Destroy(ProxyObject);

        public bool IsActive()
            => Native_IsActive(ProxyObject);

        public StructureNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled)
            => (StructureNotifyWhenAttackedResult)Native_NotifyWhenAttacked(ProxyObject, enabled);

        public override bool Equals(object? obj) => Equals(obj as NativeStructure);

        public bool Equals(NativeStructure? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeStructure? left, NativeStructure? right) => EqualityComparer<NativeStructure>.Default.Equals(left, right);

        public static bool operator !=(NativeStructure? left, NativeStructure? right) => !(left == right);

        public override string ToString()
            => $"NativeStructure[{id}]";
    }
}
