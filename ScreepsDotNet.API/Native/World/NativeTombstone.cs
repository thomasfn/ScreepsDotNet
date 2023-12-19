using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeTombstone : NativeRoomObject, ITombstone, IEquatable<NativeTombstone?>
    {
        private readonly ObjectId id;

        private NativeStore? storeCache;

        public ICreep? Creep => nativeRoot.GetOrCreateWrapperObject<ICreep>(ProxyObject.GetPropertyAsJSObject("creep"));

        public int DeathTime => ProxyObject.GetPropertyAsInt32("deathTime");

        public ObjectId Id => id;

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public int TicksToDecay => ProxyObject.GetPropertyAsInt32("ticksToDecay");

        public NativeTombstone(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache?.Dispose();
            storeCache = null;
        }

        public override string ToString()
            => $"Tombstone[{(Exists ? RoomPosition.ToString() : "DEAD")}]";

        public override bool Equals(object? obj) => Equals(obj as NativeTombstone);

        public bool Equals(NativeTombstone? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeTombstone? left, NativeTombstone? right) => EqualityComparer<NativeTombstone>.Default.Equals(left, right);

        public static bool operator !=(NativeTombstone? left, NativeTombstone? right) => !(left == right);
    }
}
