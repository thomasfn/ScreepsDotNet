using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeSource : NativeRoomObject, ISource, IEquatable<NativeSource?>
    {
        private readonly ObjectId id;

        private int? energyCache;
        private int? energyCapacityCache;
        private int? ticksToRegenerationCache;

        public int Energy => CachePerTick(ref energyCache) ??= ProxyObject.GetPropertyAsInt32(Names.Energy);

        public int EnergyCapacity => CachePerTick(ref energyCapacityCache) ??= ProxyObject.GetPropertyAsInt32(Names.EnergyCapacity);

        public ObjectId Id => id;

        public int TicksToRegeneration => CachePerTick(ref ticksToRegenerationCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToRegeneration);

        public NativeSource(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            energyCache = null;
            energyCapacityCache = null;
            ticksToRegenerationCache = null;
        }

        public override string ToString()
            => $"Source[{RoomPosition}]";

        public override bool Equals(object? obj) => Equals(obj as NativeSource);

        public bool Equals(NativeSource? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeSource? left, NativeSource? right) => EqualityComparer<NativeSource>.Default.Equals(left, right);

        public static bool operator !=(NativeSource? left, NativeSource? right) => !(left == right);
    }
}
