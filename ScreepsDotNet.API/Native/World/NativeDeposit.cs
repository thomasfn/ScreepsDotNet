using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeDeposit : NativeRoomObjectWithId, IDeposit
    {
        private int? cooldownCache;
        private ResourceType? depositTypeCache;
        private int? lastCooldownCache;
        private int? ticksToDecayCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32(Names.Cooldown);

        public ResourceType DepositType => CacheLifetime(ref depositTypeCache) ??= ProxyObject.GetPropertyAsName(Names.DepositType)!.ParseResourceType();

        public int LastCooldown => CachePerTick(ref lastCooldownCache) ??= ProxyObject.GetPropertyAsInt32(Names.LastCooldown);

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);


        public NativeDeposit(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            lastCooldownCache = null;
            ticksToDecayCache = null;
        }

        public override string ToString()
            => $"Deposit[{RoomPosition}]";
    }
}
