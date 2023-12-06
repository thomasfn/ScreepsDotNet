using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeDeposit : NativeRoomObject, IDeposit, IEquatable<NativeDeposit?>
    {
        private readonly ObjectId id;

        private int? cooldownCache;
        private ResourceType? depositTypeCache;
        private int? lastCooldownCache;
        private int? ticksToDecayCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32("cooldown");

        public ResourceType DepositType => CacheLifetime(ref depositTypeCache) ??= ProxyObject.GetPropertyAsString("depositType")!.ParseResourceType();

        public ObjectId Id => id;

        public int LastCooldown => CachePerTick(ref lastCooldownCache) ??= ProxyObject.GetPropertyAsInt32("lastCooldown");

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32("ticksToDecay");


        public NativeDeposit(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            lastCooldownCache = null;
            ticksToDecayCache = null;
        }

        public override string ToString()
            => $"Deposit[{RoomPosition}]";

        public override bool Equals(object? obj) => Equals(obj as NativeDeposit);

        public bool Equals(NativeDeposit? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeDeposit? left, NativeDeposit? right) => EqualityComparer<NativeDeposit>.Default.Equals(left, right);

        public static bool operator !=(NativeDeposit? left, NativeDeposit? right) => !(left == right);
    }
}
