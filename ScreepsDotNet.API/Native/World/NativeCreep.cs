using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    internal static class NativeCreepExtensions
    {
        public static string ToJS(this BodyPartType bodyPartType)
            => bodyPartType switch
            {
                BodyPartType.Attack => "attack",
                BodyPartType.Carry => "carry",
                BodyPartType.Heal => "heal",
                BodyPartType.Move => "move",
                BodyPartType.RangedAttack => "ranged_attack",
                BodyPartType.Tough => "tough",
                BodyPartType.Work => "work",
                BodyPartType.Claim => "claim",
                _ => throw new NotImplementedException($"Unknown body part type '{bodyPartType}'"),
            };

        public static string ToJS(this CreepOrderType creepOrderType)
            => creepOrderType switch
            {
                CreepOrderType.Attack => "attack",
                CreepOrderType.AttackController => "attackController",
                CreepOrderType.Build => "build",
                CreepOrderType.ClaimController => "claimController",
                CreepOrderType.Dismantle => "dismantle",
                CreepOrderType.Drop => "drop",
                CreepOrderType.GenerateSafeMode => "generateSafeMode",
                CreepOrderType.Harvest => "harvest",
                CreepOrderType.Heal => "heal",
                CreepOrderType.Move => "move",
                CreepOrderType.Pickup => "pickup",
                CreepOrderType.Pull => "pull",
                CreepOrderType.RangedAttack => "rangedAttack",
                CreepOrderType.RangedMassAttack => "rangedMassAttack",
                CreepOrderType.Repair => "repair",
                CreepOrderType.ReserveController => "ReserveController",
                CreepOrderType.Say => "say",
                CreepOrderType.SignController => "signController",
                CreepOrderType.Suicide => "suicide",
                CreepOrderType.Transfer => "transfer",
                CreepOrderType.UpgradeController => "upgradeController",
                CreepOrderType.Withdraw => "withdraw",
                _ => throw new NotImplementedException($"Unknown creep order type '{creepOrderType}'"),
            };

        public static BodyPartType ParseBodyPartType(this string str)
            => str switch
            {
                "attack" => BodyPartType.Attack,
                "carry" => BodyPartType.Carry,
                "heal" => BodyPartType.Heal,
                "move" => BodyPartType.Move,
                "ranged_attack" => BodyPartType.RangedAttack,
                "tough" => BodyPartType.Tough,
                "work" => BodyPartType.Work,
                "claim" => BodyPartType.Claim,
                _ => throw new NotImplementedException($"Unknown body part type '{str}'"),
            };
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeCreep : NativeRoomObject, ICreep, IEquatable<NativeCreep?>
    {
        #region Imports

        [JSImport("Creep.attack", "game/prototypes/wrapped")]
        
        internal static partial int Native_Attack(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.attackController", "game/prototypes/wrapped")]
        
        internal static partial int Native_AttackController(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.build", "game/prototypes/wrapped")]
        
        internal static partial int Native_Build(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.cancelOrder", "game/prototypes/wrapped")]
        
        internal static partial int Native_CancelOrder(JSObject proxyObject, string methodName);

        [JSImport("Creep.claimController", "game/prototypes/wrapped")]
        
        internal static partial int Native_ClaimController(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.dismantle", "game/prototypes/wrapped")]
        
        internal static partial int Native_Dismantle(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.drop", "game/prototypes/wrapped")]
        
        internal static partial int Native_Drop(JSObject proxyObject, string resourceType, int? amount);

        [JSImport("Creep.generateSafeMode", "game/prototypes/wrapped")]
        
        internal static partial int Native_GenerateSafeMode(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.getActiveBodyparts", "game/prototypes/wrapped")]
        
        internal static partial int Native_GetActiveBodyparts(JSObject proxyObject, string type);

        [JSImport("Creep.harvest", "game/prototypes/wrapped")]
        
        internal static partial int Native_Harvest(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.heal", "game/prototypes/wrapped")]
        
        internal static partial int Native_Heal(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.move", "game/prototypes/wrapped")]
        
        internal static partial int Native_Move(JSObject proxyObject, int direction);

        [JSImport("Creep.moveByPath", "game/prototypes/wrapped")]
        
        internal static partial int Native_MoveByPath(JSObject proxyObject, JSObject[] path);

        [JSImport("Creep.moveByPath", "game/prototypes/wrapped")]
        
        internal static partial int Native_MoveByPath(JSObject proxyObject, string serialisedPath);

        [JSImport("Creep.moveTo", "game/prototypes/wrapped")]
        
        internal static partial int Native_MoveTo(JSObject proxyObject, int x, int y, JSObject? opts);

        [JSImport("Creep.moveTo", "game/prototypes/wrapped")]
        
        internal static partial int Native_MoveTo(JSObject proxyObject, JSObject target, JSObject? opts);

        [JSImport("Creep.notifyWhenAttacked", "game/prototypes/wrapped")]
        
        internal static partial int Native_NotifyWhenAttacked(JSObject proxyObject, bool enabled);

        [JSImport("Creep.pickup", "game/prototypes/wrapped")]
        
        internal static partial int Native_Pickup(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.pull", "game/prototypes/wrapped")]
        
        internal static partial int Native_Pull(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.rangedAttack", "game/prototypes/wrapped")]
        
        internal static partial int Native_RangedAttack(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.rangedHeal", "game/prototypes/wrapped")]
        
        internal static partial int Native_RangedHeal(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.rangedMassAttack", "game/prototypes/wrapped")]
        
        internal static partial int Native_RangedMassAttack(JSObject proxyObject);

        [JSImport("Creep.repair", "game/prototypes/wrapped")]
        
        internal static partial int Native_Repair(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.reserveController", "game/prototypes/wrapped")]
        
        internal static partial int Native_ReserveController(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.say", "game/prototypes/wrapped")]
        
        internal static partial int Native_Say(JSObject proxyObject, string message, bool? sayPublic);

        [JSImport("Creep.signController", "game/prototypes/wrapped")]
        
        internal static partial int Native_SignController(JSObject proxyObject, JSObject targetProxyObject, string text);

        [JSImport("Creep.suicide", "game/prototypes/wrapped")]
        
        internal static partial int Native_Suicide(JSObject proxyObject);

        [JSImport("Creep.transfer", "game/prototypes/wrapped")]
        
        internal static partial int Native_Transfer(JSObject proxyObject, JSObject targetProxyObject, string resourceType, int? amount);

        [JSImport("Creep.upgradeController", "game/prototypes/wrapped")]
        
        internal static partial int Native_UpgradeController(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.withdraw", "game/prototypes/wrapped")]
        
        internal static partial int Native_Withdraw(JSObject proxyObject, JSObject targetProxyObject, string resourceType, int? amount);

        #endregion

        private readonly ObjectId id;

        private BodyPart<BodyPartType>[]? bodyCache;
        private BodyType<BodyPartType>? bodyTypeCache;
        private int? fatigueCache;
        private int? hitsCache;
        private int? hitsMaxCache;
        private IMemoryObject? memoryCache;
        private bool? myCache;
        private string? nameCache;
        private OwnerInfo? ownerInfoCache;
        private NativeStore? storeCache;
        private int? ticksToLiveCache;

        protected override bool CanMove { get => true; }

        public IEnumerable<BodyPart<BodyPartType>> Body
            => bodyCache ??= JSUtils.GetObjectArrayOnObject(ProxyObject, "body")!
                .Select(x => new BodyPart<BodyPartType>(x.GetPropertyAsString("type")!.ParseBodyPartType(), x.GetPropertyAsInt32("hits"), x.GetPropertyAsString("boost")))
                .ToArray();

        public BodyType<BodyPartType> BodyType => CachePerTick(ref bodyTypeCache) ??= new(Body.Select(x => x.Type));

        public int Fatigue => CachePerTick(ref fatigueCache) ??= ProxyObject.GetPropertyAsInt32("fatigue");

        public int Hits => CachePerTick(ref hitsCache) ??= ProxyObject.GetPropertyAsInt32("hits");

        public int HitsMax => CachePerTick(ref hitsMaxCache) ??= ProxyObject.GetPropertyAsInt32("hitsMax");

        public ObjectId Id => id;

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject("memory")!);

        public bool My => CacheLifetime(ref myCache) ??= ProxyObject.GetPropertyAsBoolean("my");

        public string Name => CacheLifetime(ref nameCache) ??= ProxyObject.GetPropertyAsString("name")!;

        public OwnerInfo Owner => CacheLifetime(ref ownerInfoCache) ??= new(ProxyObject.GetPropertyAsJSObject("owner")!.GetPropertyAsString("username")!);

        public string? Saying => ProxyObject.GetPropertyAsString("saying")!;

        public bool Spawning => ProxyObject.GetPropertyAsBoolean("spawning");

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public int TicksToLive => CachePerTick(ref ticksToLiveCache) ??= ProxyObject.GetPropertyAsInt32("ticksToLive");

        public NativeCreep(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override void UpdateFromDataPacket(RoomObjectDataPacket dataPacket)
        {
            base.UpdateFromDataPacket(dataPacket);
            myCache = dataPacket.My;
            hitsCache = dataPacket.Hits;
            hitsMaxCache = dataPacket.HitsMax;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            bodyCache = null;
            fatigueCache = null;
            hitsCache = null;
            hitsMaxCache = null;
            memoryCache = null;
            storeCache = null;
            ticksToLiveCache = null;
        }

        public CreepAttackResult Attack(ICreep target)
            => (CreepAttackResult)Native_Attack(ProxyObject, target.ToJS());

        // CreepAttackResult Attack(IPowerCreep target);

        public CreepAttackResult Attack(IStructure target)
            => (CreepAttackResult)Native_Attack(ProxyObject, target.ToJS());

        public CreepAttackControllerResult AttackController(IStructureController target)
            => (CreepAttackControllerResult)Native_AttackController(ProxyObject, target.ToJS());

        public CreepBuildResult Build(IConstructionSite target)
            => (CreepBuildResult)Native_Build(ProxyObject, target.ToJS());

        public void CancelOrder(CreepOrderType orderType)
            => Native_CancelOrder(ProxyObject, orderType.ToJS());

        public CreepClaimControllerResult ClaimController(IStructureController controller)
            => (CreepClaimControllerResult)Native_AttackController(ProxyObject, controller.ToJS());

        public CreepDismantleResult Dismantle(IStructure target)
            => (CreepDismantleResult)Native_Dismantle(ProxyObject, target.ToJS());

        public CreepDropResult Drop(ResourceType resourceType, int? amount = null)
            => (CreepDropResult)Native_Drop(ProxyObject, resourceType.ToJS(), amount);

        public CreepGenerateSafeModeResult GenerateSafeMode(IStructureController controller)
            => (CreepGenerateSafeModeResult)Native_GenerateSafeMode(ProxyObject, controller.ToJS());

        public int GetActiveBodyparts(BodyPartType type)
            => Native_GetActiveBodyparts(ProxyObject, type.ToJS());

        public CreepHarvestResult Harvest(ISource source)
            => (CreepHarvestResult)Native_Harvest(ProxyObject, source.ToJS());

        public CreepHarvestResult Harvest(IMineral mineral)
            => (CreepHarvestResult)Native_Harvest(ProxyObject, mineral.ToJS());

        public CreepHarvestResult Harvest(IDeposit deposit)
            => (CreepHarvestResult) Native_Harvest(ProxyObject, deposit.ToJS());

        public CreepHealResult Heal(ICreep target)
            => (CreepHealResult)Native_Heal(ProxyObject, target.ToJS());

        public CreepMoveResult Move(Direction direction)
            => (CreepMoveResult)Native_Move(ProxyObject, (int)direction);

        public CreepMoveResult MoveByPath(IEnumerable<PathStep> path)
        {
            var pathArr = path.Select(x => x.ToJS()).ToArray();
            try
            {
                return (CreepMoveResult)Native_MoveByPath(ProxyObject, pathArr);
            }
            finally
            {
                foreach (var obj in pathArr) { obj.Dispose(); }
            }
        }

        public CreepMoveResult MoveByPath(string serialisedPath)
            => (CreepMoveResult)Native_MoveByPath(ProxyObject, serialisedPath);

        public CreepMoveResult MoveTo(Position target, object? opts = null)
            => (CreepMoveResult)Native_MoveTo(ProxyObject, target.X, target.Y, null);

        public CreepMoveResult MoveTo(RoomPosition target, object? opts = null)
            => (CreepMoveResult)Native_MoveTo(ProxyObject, target.ToJS(), null);

        public CreepNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled)
            => (CreepNotifyWhenAttackedResult)Native_NotifyWhenAttacked(ProxyObject, enabled);

        public CreepPickupResult Pickup(IResource target)
            => (CreepPickupResult)Native_Pickup(ProxyObject, target.ToJS());

        public CreepPullResult Pull(ICreep target)
            => (CreepPullResult)Native_Pull(ProxyObject, target.ToJS());

        public CreepRangedAttackResult RangedAttack(ICreep target)
            => (CreepRangedAttackResult)Native_RangedAttack(ProxyObject, target.ToJS());

        // CreepRangedAttackResult RangedAttack(IPowerCreep target);

        public CreepRangedAttackResult RangedAttack(IStructure target)
            => (CreepRangedAttackResult)Native_RangedAttack(ProxyObject, target.ToJS());

        public CreepHealResult RangedHeal(ICreep target)
            => (CreepHealResult)Native_RangedHeal(ProxyObject, target.ToJS());

        public CreepRangedMassAttackResult RangedMassAttack()
            => (CreepRangedMassAttackResult)Native_RangedMassAttack(ProxyObject);

        public CreepRepairResult Repair(IStructure target)
            => (CreepRepairResult)Native_Repair(ProxyObject, target.ToJS());

        public CreepReserveControllerResult ReserveController(IStructureController target)
            => (CreepReserveControllerResult)Native_ReserveController(ProxyObject, target.ToJS());

        public CreepSayResult Say(string message, bool sayPublic = false)
            => (CreepSayResult)Native_Say(ProxyObject, message, sayPublic);

        public CreepSignControllerResult SignController(IStructureController target, string text)
            => (CreepSignControllerResult)Native_SignController(ProxyObject, target.ToJS(), text);

        public CreepSuicideResult Suicide()
            => (CreepSuicideResult)Native_Suicide(ProxyObject);

        public CreepTransferResult Transfer(ICreep target, ResourceType resourceType, int? amount = null)
            => (CreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        // CreepTransferResult Transfer(IPowerCreep target, ResourceType resourceType, int? amount = null);

        public CreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount = null)
            => (CreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepUpgradeControllerResult UpgradeController(IStructureController target)
            => (CreepUpgradeControllerResult)Native_UpgradeController(ProxyObject, target.ToJS());

        public CreepWithdrawResult Withdraw(IStructure target, ResourceType resourceType, int? amount = null)
            => (CreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepWithdrawResult Withdraw(ITombstone target, ResourceType resourceType, int? amount = null)
            => (CreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepWithdrawResult Withdraw(IRuin target, ResourceType resourceType, int? amount = null)
            => (CreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public override string ToString()
            => $"Creep[{(Exists ? $"'{Name}'" : id.ToString())}]({(Exists ? $"{RoomPosition}" : "DEAD")})";

        public override bool Equals(object? obj) => Equals(obj as NativeCreep);

        public bool Equals(NativeCreep? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeCreep? left, NativeCreep? right) => EqualityComparer<NativeCreep>.Default.Equals(left, right);

        public static bool operator !=(NativeCreep? left, NativeCreep? right) => !(left == right);
    }
}
