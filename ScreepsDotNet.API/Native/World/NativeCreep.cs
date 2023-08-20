using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
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

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeCreep : NativeRoomObject, ICreep, IEquatable<NativeCreep?>
    {
        #region Imports

        [JSImport("Creep.attack", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Attack([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.attackController", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_AttackController([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.build", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Build([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.cancelOrder", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_CancelOrder([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string methodName);

        [JSImport("Creep.claimController", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_ClaimController([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.dismantle", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Dismantle([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.drop", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Drop([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string resourceType, [JSMarshalAs<JSType.Number>] int? amount);

        [JSImport("Creep.generateSafeMode", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GenerateSafeMode([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.getActiveBodyparts", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetActiveBodyparts([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string type);

        [JSImport("Creep.harvest", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Harvest([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.heal", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Heal([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.move", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Move([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int direction);

        [JSImport("Creep.moveByPath", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_MoveByPath([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] path);

        [JSImport("Creep.moveByPath", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_MoveByPath([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string serialisedPath);

        [JSImport("Creep.moveTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_MoveTo([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y, [JSMarshalAs<JSType.Object>] JSObject? opts);

        [JSImport("Creep.moveTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_MoveTo([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject target, [JSMarshalAs<JSType.Object>] JSObject? opts);

        [JSImport("Creep.notifyWhenAttacked", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_NotifyWhenAttacked([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Boolean>] bool enabled);

        [JSImport("Creep.pickup", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Pickup([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.pull", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Pull([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.rangedAttack", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_RangedAttack([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.rangedHeal", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_RangedHeal([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.rangedMassAttack", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_RangedMassAttack([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("Creep.repair", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Repair([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.reserveController", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_ReserveController([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.say", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Say([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.Boolean>] bool? sayPublic);

        [JSImport("Creep.signController", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_SignController([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject, [JSMarshalAs<JSType.String>] string text);

        [JSImport("Creep.suicide", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Suicide([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("Creep.transfer", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Transfer([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject, [JSMarshalAs<JSType.String>] string resourceType, [JSMarshalAs<JSType.Number>] int? amount);

        [JSImport("Creep.upgradeController", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_UpgradeController([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.withdraw", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Withdraw([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject, [JSMarshalAs<JSType.String>] string resourceType, [JSMarshalAs<JSType.Number>] int? amount);

        #endregion

        private readonly string id;

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

        public string Id => id;

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject("memory")!);

        public bool My => CacheLifetime(ref myCache) ??= ProxyObject.GetPropertyAsBoolean("my");

        public string Name => CacheLifetime(ref nameCache) ??= ProxyObject.GetPropertyAsString("name")!;

        public OwnerInfo Owner => CacheLifetime(ref ownerInfoCache) ??= new(ProxyObject.GetPropertyAsJSObject("owner")!.GetPropertyAsString("username")!);

        public string? Saying => ProxyObject.GetPropertyAsString("saying")!;

        public bool Spawning => ProxyObject.GetPropertyAsBoolean("spawning");

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public int TicksToLive => CachePerTick(ref ticksToLiveCache) ??= ProxyObject.GetPropertyAsInt32("ticksToLive");

        public NativeCreep(INativeRoot nativeRoot, JSObject proxyObject, string knownId)
            : base(nativeRoot, proxyObject)
        {
            id = knownId;
        }

        public NativeCreep(INativeRoot nativeRoot, JSObject proxyObject)
            : this(nativeRoot, proxyObject, proxyObject.GetPropertyAsString("id")!)
        { }

        public NativeCreep(INativeRoot nativeRoot, string id, RoomPosition? roomPos)
            : base(nativeRoot, null)
        {
            this.id = id;
            positionCache = roomPos;
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

        //CreepHarvestResult Harvest(IMineral source);

        //CreepHarvestResult Harvest(IDeposit source);

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

        // CreepWithdrawResult Withdraw(IRuin target, ResourceType resourceType, int? amount = null);

        public override string ToString()
            => $"Creep['{nameCache ?? id}']({(Exists ? $"{RoomPosition}" : "DEAD")})";

        public override bool Equals(object? obj) => Equals(obj as NativeCreep);

        public bool Equals(NativeCreep? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeCreep? left, NativeCreep? right) => EqualityComparer<NativeCreep>.Default.Equals(left, right);

        public static bool operator !=(NativeCreep? left, NativeCreep? right) => !(left == right);
    }
}
