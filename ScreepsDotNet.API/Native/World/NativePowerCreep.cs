using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    internal static class NativePowerCreepExtensions
    {
        private static readonly Name[] powerCreepOrderTypeToName =
        [
            Name.Create("drop"),
            Name.Create("move"),
            Name.Create("pickup"),
            Name.Create("say"),
            Name.Create("suicide"),
            Name.Create("transfer"),
            Name.Create("upgrade"),
            Name.Create("usePower"),
            Name.Create("withdraw"),
        ];

        private static readonly Name[] powerCreepClassToName =
        [
            Name.Create("operator"),
        ];

        private static readonly Dictionary<Name, PowerCreepClass> nameToPowerCreepClass;

        static NativePowerCreepExtensions()
        {
            nameToPowerCreepClass = [];
            for (int i = 0; i < powerCreepClassToName.Length; ++i)
            {
                nameToPowerCreepClass.Add(powerCreepClassToName[i], (PowerCreepClass)i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Name ToJS(this PowerCreepOrderType powerCreepOrderType)
            => powerCreepOrderTypeToName[(int)powerCreepOrderType];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Name ToJS(this PowerCreepClass powerCreepClass)
            => powerCreepClassToName[(int)powerCreepClass];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PowerCreepClass ParsePowerCreepClass(this Name name)
            => nameToPowerCreepClass[name];
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativePowerCreep : NativeRoomObjectWithId, IPowerCreep
    {
        #region Imports

        [JSImport("PowerCreep.cancelOrder", "game/prototypes/wrapped")]
        internal static partial void Native_CancelOrder(JSObject proxyObject, Name methodName);

        [JSImport("PowerCreep.delete", "game/prototypes/wrapped")]
        internal static partial int Native_Delete(JSObject proxyObject, bool? cancel);

        [JSImport("PowerCreep.drop", "game/prototypes/wrapped")]
        internal static partial int Native_Drop(JSObject proxyObject, int resourceType, int? amount);

        [JSImport("PowerCreep.enableRoom", "game/prototypes/wrapped")]
        internal static partial int Native_EnableRoom(JSObject proxyObject, JSObject controller);

        [JSImport("PowerCreep.move", "game/prototypes/wrapped")]
        internal static partial int Native_Move(JSObject proxyObject, int direction);

        [JSImport("PowerCreep.moveByPath", "game/prototypes/wrapped")]
        internal static partial int Native_MoveByPath(JSObject proxyObject, string serialisedPath);

        [JSImport("PowerCreep.moveByPath", "game/prototypes/wrapped")]
        internal static partial int Native_MoveByPath(JSObject proxyObject, JSObject[] path);

        [JSImport("PowerCreep.moveTo", "game/prototypes/wrapped")]
        internal static partial int Native_MoveTo(JSObject proxyObject, int x, int y, JSObject? opts);

        [JSImport("PowerCreep.moveTo", "game/prototypes/wrapped")]
        internal static partial int Native_MoveTo(JSObject proxyObject, JSObject target, JSObject? opts);

        [JSImport("PowerCreep.notifyWhenAttacked", "game/prototypes/wrapped")]
        internal static partial int Native_NotifyWhenAttacked(JSObject proxyObject, bool enabled);

        [JSImport("PowerCreep.pickup", "game/prototypes/wrapped")]
        internal static partial int Native_Pickup(JSObject proxyObject, JSObject target);

        [JSImport("PowerCreep.rename", "game/prototypes/wrapped")]
        internal static partial int Native_Rename(JSObject proxyObject, string name);

        [JSImport("PowerCreep.renew", "game/prototypes/wrapped")]
        internal static partial int Native_Renew(JSObject proxyObject, JSObject target);

        [JSImport("PowerCreep.say", "game/prototypes/wrapped")]
        internal static partial int Native_Say(JSObject proxyObject, string message, bool? sayPublic);

        [JSImport("PowerCreep.spawn", "game/prototypes/wrapped")]
        internal static partial int Native_Spawn(JSObject proxyObject, JSObject powerSpawn);

        [JSImport("PowerCreep.suicide", "game/prototypes/wrapped")]
        internal static partial int Native_Suicide(JSObject proxyObject);

        [JSImport("PowerCreep.transfer", "game/prototypes/wrapped")]
        internal static partial int Native_Transfer(JSObject proxyObject, JSObject target, int resourceType, int? amount);

        [JSImport("PowerCreep.upgrade", "game/prototypes/wrapped")]
        internal static partial int Native_Upgrade(JSObject proxyObject, int power);

        [JSImport("PowerCreep.usePower", "game/prototypes/wrapped")]
        internal static partial int Native_UsePower(JSObject proxyObject, int power, JSObject? target);

        [JSImport("PowerCreep.withdraw", "game/prototypes/wrapped")]
        internal static partial int Native_Withdraw(JSObject proxyObject, JSObject target, int resourceType, int? amount);

        #endregion

        private readonly NativeStore store;

        private PowerCreepClass? classCache;
        private DateTime? deleteTimeCache;
        private int? hitsCache;
        private int? hitsMaxCache;
        private int? levelCache;
        private IMemoryObject? memoryCache;
        private bool? myCache;
        private string? nameCache;
        private OwnerInfo? ownerInfoCache;
        private Dictionary<PowerType, PowerState>? powersCache;
        private string? sayingCache;
        private string? shardCache;
        private DateTime? spawnCooldownTimeCache;
        private int? ticksToLiveCache;

        protected override bool CanMove => true;

        public PowerCreepClass Class => CacheLifetime(ref classCache) ??= ProxyObject.GetPropertyAsName(Names.ClassName).ParsePowerCreepClass();

        public DateTime? DeleteTime => CachePerTick(ref deleteTimeCache) ??= DateTime.UnixEpoch + TimeSpan.FromSeconds(ProxyObject.GetPropertyAsInt32(Names.DeleteTime));

        public int Hits => CachePerTick(ref hitsCache) ??= ProxyObject.GetPropertyAsInt32(Names.Hits);

        public int HitsMax => CachePerTick(ref hitsMaxCache) ??= ProxyObject.GetPropertyAsInt32(Names.HitsMax);

        public int Level => CachePerTick(ref levelCache) ??= ProxyObject.GetPropertyAsInt32(Names.Level);

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject(Names.Memory)!);

        public bool My => CacheLifetime(ref myCache) ??= ProxyObject.GetPropertyAsBoolean(Names.My);

        public string Name => CacheLifetime(ref nameCache) ??= ProxyObject.GetPropertyAsString(Names.Name)!;

        public OwnerInfo Owner => CacheLifetime(ref ownerInfoCache) ??= new(ProxyObject.GetPropertyAsJSObject(Names.Owner)!.GetPropertyAsString(Names.Username)!);

        public IReadOnlyDictionary<PowerType, PowerState> Powers => CachePerTick(ref powersCache) ??= FetchPowers();

        public string? Saying => CachePerTick(ref sayingCache) ??= ProxyObject.GetPropertyAsString(Names.Saying)!;

        public string? Shard => CachePerTick(ref shardCache) ??= ProxyObject.GetPropertyAsString(Names.Shard)!;

        public DateTime? SpawnCooldownTime => CachePerTick(ref spawnCooldownTimeCache) ??= FetchSpawnCooldownTime();

        public IStore Store => store;

        public int? TicksToLive => CachePerTick(ref ticksToLiveCache) ??= ProxyObject.TryGetPropertyAsInt32(Names.TicksToLive);


        public NativePowerCreep(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
            store = new NativeStore(nativeRoot, proxyObject);
        }


        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            store.ClearNativeCache();
            deleteTimeCache = null;
            hitsCache = null;
            hitsMaxCache = null;
            levelCache = null;
            memoryCache = null;
            powersCache = null;
            sayingCache = null;
            shardCache = null;
            ticksToLiveCache = null;
        }

        protected override void OnGetNewProxyObject(JSObject newProxyObject)
        {
            base.OnGetNewProxyObject(newProxyObject);
            store.ProxyObject = newProxyObject;
        }

        public void CancelOrder(PowerCreepOrderType orderType)
            => Native_CancelOrder(ProxyObject, orderType.ToJS());

        public PowerCreepDeleteResult Delete(bool? cancel = null)
            => (PowerCreepDeleteResult)Native_Delete(ProxyObject, cancel);

        public PowerCreepDropResult Drop(ResourceType resourceType, int? amount = null)
            => (PowerCreepDropResult)Native_Drop(ProxyObject, (int)resourceType, amount);

        public PowerCreepEnableRoomResult EnableRoom(IStructureController controller)
            => (PowerCreepEnableRoomResult)Native_EnableRoom(ProxyObject, controller.ToJS());

        public PowerCreepMoveResult Move(Direction direction)
            => (PowerCreepMoveResult)Native_Move(ProxyObject, (int)direction);

        public PowerCreepMoveResult MoveByPath(IEnumerable<PathStep> path)
        {
            var pathArr = path.Select(x => x.ToJS()).ToArray();
            try
            {
                return (PowerCreepMoveResult)Native_MoveByPath(ProxyObject, pathArr);
            }
            finally
            {
                pathArr.DisposeAll();
            }
        }

        public PowerCreepMoveResult MoveByPath(string serialisedPath)
            => (PowerCreepMoveResult)Native_MoveByPath(ProxyObject, serialisedPath);

        public PowerCreepMoveResult MoveTo(Position target, MoveToOptions? opts = null)
        {
            using var optsJs = opts?.ToJS();
            return (PowerCreepMoveResult)Native_MoveTo(ProxyObject, target.X, target.Y, optsJs);
        }

        public PowerCreepMoveResult MoveTo(RoomPosition target, MoveToOptions? opts = null)
        {
            using var optsJs = opts?.ToJS();
            return (PowerCreepMoveResult)Native_MoveTo(ProxyObject, target.ToJS(), optsJs);
        }

        public PowerCreepNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled)
            => (PowerCreepNotifyWhenAttackedResult)Native_NotifyWhenAttacked(ProxyObject, enabled);

        public PowerCreepPickupResult Pickup(IResource target)
            => (PowerCreepPickupResult)Native_Pickup(ProxyObject, target.ToJS());

        public PowerCreepRenameResult Rename(string name)
            => (PowerCreepRenameResult)Native_Rename(ProxyObject, name);

        public PowerCreepRenewResult Renew(IStructurePowerBank target)
            => (PowerCreepRenewResult)Native_Renew(ProxyObject, target.ToJS());

        public PowerCreepRenewResult Renew(IStructurePowerSpawn target)
            => (PowerCreepRenewResult)Native_Renew(ProxyObject, target.ToJS());

        public PowerCreepSayResult Say(string message, bool @public = false)
            => (PowerCreepSayResult)Native_Say(ProxyObject, message, @public);

        public PowerCreepSpawnResult Spawn(IStructurePowerSpawn powerSpawn)
            => (PowerCreepSpawnResult)Native_Spawn(ProxyObject, powerSpawn.ToJS());

        public PowerCreepSuicideResult Suicide()
            => (PowerCreepSuicideResult)Native_Suicide(ProxyObject);

        public PowerCreepTransferResult Transfer(ICreep target, ResourceType resourceType, int? amount = null)
            => (PowerCreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), (int)resourceType, amount);

        public PowerCreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount = null)
            => (PowerCreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), (int)resourceType, amount);

        public PowerCreepUpgradeResult Upgrade(PowerType power)
            => (PowerCreepUpgradeResult)Native_Upgrade(ProxyObject, (int)power);

        public PowerCreepUsePowerResult UsePower(PowerType power, IRoomObject? target = null)
            => (PowerCreepUsePowerResult)Native_UsePower(ProxyObject, (int)power, target?.ToJS());

        public PowerCreepWithdrawResult Withdraw(IStructure target, ResourceType resourceType, int? amount = null)
            => (PowerCreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), (int)resourceType, amount);

        public PowerCreepWithdrawResult Withdraw(ITombstone target, ResourceType resourceType, int? amount = null)
            => (PowerCreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), (int)resourceType, amount);

        private Dictionary<PowerType, PowerState> FetchPowers()
        {
            using var powersObj = ProxyObject.GetPropertyAsJSObject(Names.Powers);
            Dictionary<PowerType, PowerState> result = [];
            if (powersObj == null) { return result; }
            foreach (var powerId in powersObj.GetPropertyNames())
            {
                PowerType powerType = (PowerType)int.Parse(powerId);
                using var powerStateObj = powersObj.GetPropertyAsJSObject(powerId);
                if (powerStateObj == null) { continue; }
                result.Add(powerType, new(powerStateObj.GetPropertyAsInt32(Names.Level), powerStateObj.GetPropertyAsInt32(Names.Cooldown)));
            }
            return result;
        }

        private DateTime? FetchSpawnCooldownTime()
        {
            var timestamp = ProxyObject.TryGetPropertyAsInt32(Names.SpawnCooldownTimeCache);
            if (timestamp == null) { return null; }
            return DateTime.UnixEpoch + TimeSpan.FromSeconds(timestamp.Value);
        }
    }
}
