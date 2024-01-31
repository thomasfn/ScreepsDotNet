using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeMineral : NativeRoomObject, IMineral, IEquatable<NativeMineral?>
    {
        private readonly ObjectId id;

        private int? densityCache;
        private int? mineralAmountCache;
        private ResourceType? mineralTypeCache;
        private int? ticksToRegenerationCache;

        public int Density => CacheLifetime(ref densityCache) ??= ProxyObject.GetPropertyAsInt32("density");

        public int MineralAmount => CachePerTick(ref mineralAmountCache) ??= ProxyObject.GetPropertyAsInt32("mineralAmount");

        public ResourceType MineralType => CacheLifetime(ref mineralTypeCache) ??= ProxyObject.GetPropertyAsString("mineralType")!.ParseResourceType();

        public ObjectId Id => id;

        public int? TicksToRegeneration => CachePerTick(ref ticksToRegenerationCache) ??= ProxyObject.TryGetPropertyAsInt32("ticksToRegeneration");

        public NativeMineral(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override void UpdateFromDataPacket(RoomObjectDataPacket dataPacket)
        {
            base.UpdateFromDataPacket(dataPacket);
            mineralAmountCache = dataPacket.Hits;
            densityCache = dataPacket.HitsMax;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            mineralAmountCache = null;
            ticksToRegenerationCache = null;
        }

        public override string ToString()
            => $"Mineral[{RoomPosition}]";

        public override bool Equals(object? obj) => Equals(obj as NativeMineral);

        public bool Equals(NativeMineral? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeMineral? left, NativeMineral? right) => EqualityComparer<NativeMineral>.Default.Equals(left, right);

        public static bool operator !=(NativeMineral? left, NativeMineral? right) => !(left == right);
    }
}
