using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ScreepsDotNet.Interop;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class NativeCreepExtensions
    {
        private static readonly Name[] bodyPartTypeToName =
        [
            Name.Create("move"),
            Name.Create("work"),
            Name.Create("carry"),
            Name.Create("attack"),
            Name.Create("ranged_attack"),
            Name.Create("tough"),
            Name.Create("heal"),
            Name.Create("claim"),
        ];

        private static readonly Name[] creepOrderTypeToName =
        [
            Name.Create("attack"),
            Name.Create("attackController"),
            Name.Create("build"),
            Name.Create("claimController"),
            Name.Create("dismantle"),
            Name.Create("drop"),
            Name.Create("generateSafeMode"),
            Name.Create("harvest"),
            Name.Create("heal"),
            Name.Create("move"),
            Name.Create("pickup"),
            Name.Create("pull"),
            Name.Create("rangedAttack"),
            Name.Create("rangedMassAttack"),
            Name.Create("repair"),
            Name.Create("reserveController"),
            Name.Create("say"),
            Name.Create("signController"),
            Name.Create("suicide"),
            Name.Create("transfer"),
            Name.Create("upgradeController"),
            Name.Create("withdraw"),
        ];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Name ToJS(this BodyPartType bodyPartType)
            => bodyPartTypeToName[(int)bodyPartType];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Name ToJS(this CreepOrderType creepOrderType)
            => creepOrderTypeToName[(int)creepOrderType];

        public static JSObject ToJS(this MoveToOptions moveToOptions)
        {
            var obj = moveToOptions.FindPathOptions != null ? moveToOptions.FindPathOptions.Value.ToJS() : JSObject.Create();
            obj.SetProperty(Names.ReusePath, moveToOptions.ReusePath);
            obj.SetProperty(Names.SerializeMemory, moveToOptions.SerializeMemory);
            obj.SetProperty(Names.NoPathFinding, moveToOptions.NoPathFinding);
            if (moveToOptions.VisualizePathStyle != null) { obj.SetProperty(Names.VisualizePathStyle, moveToOptions.VisualizePathStyle.Value.ToJS()); }
            return obj;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeCreep : NativeRoomObjectWithId, ICreep
    {
        #region Imports

        [JSImport("Creep.attack", "game/prototypes/wrapped")]
        internal static partial int Native_Attack(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.attackController", "game/prototypes/wrapped")]
        internal static partial int Native_AttackController(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.build", "game/prototypes/wrapped")]
        internal static partial int Native_Build(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.cancelOrder", "game/prototypes/wrapped")]
        internal static partial int Native_CancelOrder(JSObject proxyObject, Name methodName);

        [JSImport("Creep.claimController", "game/prototypes/wrapped")]
        internal static partial int Native_ClaimController(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.dismantle", "game/prototypes/wrapped")]
        internal static partial int Native_Dismantle(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.drop", "game/prototypes/wrapped")]
        internal static partial int Native_Drop(JSObject proxyObject, Name resourceType, int? amount);

        [JSImport("Creep.generateSafeMode", "game/prototypes/wrapped")]
        internal static partial int Native_GenerateSafeMode(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.getActiveBodyparts", "game/prototypes/wrapped")]
        internal static partial int Native_GetActiveBodyparts(JSObject proxyObject, Name type);

        [JSImport("Creep.harvest", "game/prototypes/wrapped")]
        internal static partial int Native_Harvest(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.heal", "game/prototypes/wrapped")]
        internal static partial int Native_Heal(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.move", "game/prototypes/wrapped")]
        internal static partial int Native_Move(JSObject proxyObject, int direction);

        [JSImport("Creep.move", "game/prototypes/wrapped")]
        internal static partial int Native_Move(JSObject proxyObject, JSObject targetProxyObject);

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
        internal static partial int Native_Transfer(JSObject proxyObject, JSObject targetProxyObject, Name resourceType, int? amount);

        [JSImport("Creep.upgradeController", "game/prototypes/wrapped")]
        internal static partial int Native_UpgradeController(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.withdraw", "game/prototypes/wrapped")]
        internal static partial int Native_Withdraw(JSObject proxyObject, JSObject targetProxyObject, Name resourceType, int? amount);

        [JSImport("Creep.getEncodedBody", "game/prototypes/wrapped")]
        internal static partial int Native_GetEncodedBody(JSObject proxyObject, IntPtr outEncodedBodyParts);

        #endregion

        private readonly NativeStore store;

        private BodyPart<BodyPartType>[]? bodyMemoryCache;
        private BodyPart<BodyPartType>[]? bodyCache;
        private BodyType<BodyPartType>? bodyTypeCache;
        private int? fatigueCache;
        private int? hitsCache;
        private int? hitsMaxCache;
        private IMemoryObject? memoryCache;
        private bool? myCache;
        private string? nameCache;
        private OwnerInfo? ownerInfoCache;
        private int? ticksToLiveCache;

        protected override bool CanMove => true;

        public IEnumerable<BodyPart<BodyPartType>> Body => CachePerTick(ref bodyCache) ??= FetchBody();

        public BodyType<BodyPartType> BodyType => CacheLifetime(ref bodyTypeCache) ??= new(Body.Select(x => x.Type));

        public int Fatigue => CachePerTick(ref fatigueCache) ??= ProxyObject.GetPropertyAsInt32(Names.Fatigue);

        public int Hits => CachePerTick(ref hitsCache) ??= ProxyObject.GetPropertyAsInt32(Names.Hits);

        public int HitsMax => CachePerTick(ref hitsMaxCache) ??= ProxyObject.GetPropertyAsInt32(Names.HitsMax);

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject(Names.Memory)!);

        public bool My => CacheLifetime(ref myCache) ??= ProxyObject.GetPropertyAsBoolean(Names.My);

        public string Name => CacheLifetime(ref nameCache) ??= ProxyObject.GetPropertyAsString(Names.Name)!;

        public OwnerInfo Owner => CacheLifetime(ref ownerInfoCache) ??= new(ProxyObject.GetPropertyAsJSObject(Names.Owner)!.GetPropertyAsString(Names.Username)!);

        public string? Saying => ProxyObject.GetPropertyAsString(Names.Saying)!;

        public bool Spawning => ProxyObject.GetPropertyAsBoolean(Names.Spawning);

        public IStore Store => store;

        public int TicksToLive => CachePerTick(ref ticksToLiveCache) ??= FetchTTL();

        public NativeCreep(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
            store = new NativeStore(nativeRoot, proxyObject);
            store.OnRequestNewProxyObject += Store_OnRequestNewProxyObject;
        }

        private void Store_OnRequestNewProxyObject()
        {
            TouchProxyObject();
        }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            store.ClearNativeCache();
            bodyCache = null;
            fatigueCache = null;
            hitsCache = null;
            hitsMaxCache = null;
            memoryCache = null;
            ticksToLiveCache = null;
        }

        protected override void OnGetNewProxyObject(JSObject newProxyObject)
        {
            base.OnGetNewProxyObject(newProxyObject);
            store.ProxyObject = newProxyObject;
        }

        protected override void OnRenewProxyObject()
        {
            base.OnRenewProxyObject();
            store.RenewProxyObject();
        }

        public CreepAttackResult Attack(ICreep target)
            => (CreepAttackResult)Native_Attack(ProxyObject, target.ToJS());

        public CreepAttackResult Attack(IPowerCreep target)
            => (CreepAttackResult)Native_Attack(ProxyObject, target.ToJS());

        public CreepAttackResult Attack(IStructure target)
            => (CreepAttackResult)Native_Attack(ProxyObject, target.ToJS());

        public CreepAttackControllerResult AttackController(IStructureController target)
            => (CreepAttackControllerResult)Native_AttackController(ProxyObject, target.ToJS());

        public CreepBuildResult Build(IConstructionSite target)
            => (CreepBuildResult)Native_Build(ProxyObject, target.ToJS());

        public void CancelOrder(CreepOrderType orderType)
            => Native_CancelOrder(ProxyObject, orderType.ToJS());

        public CreepClaimControllerResult ClaimController(IStructureController controller)
            => (CreepClaimControllerResult)Native_ClaimController(ProxyObject, controller.ToJS());

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

        public CreepHealResult Heal(IPowerCreep target)
            => (CreepHealResult)Native_Heal(ProxyObject, target.ToJS());

        public CreepMoveResult Move(Direction direction)
            => (CreepMoveResult)Native_Move(ProxyObject, (int)direction);

        public CreepMoveResult Move(ICreep creep)
            => (CreepMoveResult)Native_Move(ProxyObject, creep.ToJS());

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

        public CreepMoveResult MoveTo(Position target, MoveToOptions? opts = null)
        {
            using var optsJs = opts?.ToJS();
            return (CreepMoveResult)Native_MoveTo(ProxyObject, target.X, target.Y, optsJs);
        }

        public CreepMoveResult MoveTo(RoomPosition target, MoveToOptions? opts = null)
        {
            using var optsJs = opts?.ToJS();
            return (CreepMoveResult)Native_MoveTo(ProxyObject, target.ToJS(), optsJs);
        }

        public CreepNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled)
            => (CreepNotifyWhenAttackedResult)Native_NotifyWhenAttacked(ProxyObject, enabled);

        public CreepPickupResult Pickup(IResource target)
            => (CreepPickupResult)Native_Pickup(ProxyObject, target.ToJS());

        public CreepPullResult Pull(ICreep target)
            => (CreepPullResult)Native_Pull(ProxyObject, target.ToJS());

        public CreepRangedAttackResult RangedAttack(ICreep target)
            => (CreepRangedAttackResult)Native_RangedAttack(ProxyObject, target.ToJS());

        public CreepRangedAttackResult RangedAttack(IPowerCreep target)
            => (CreepRangedAttackResult)Native_RangedAttack(ProxyObject, target.ToJS());

        public CreepRangedAttackResult RangedAttack(IStructure target)
            => (CreepRangedAttackResult)Native_RangedAttack(ProxyObject, target.ToJS());

        public CreepHealResult RangedHeal(ICreep target)
            => (CreepHealResult)Native_RangedHeal(ProxyObject, target.ToJS());

        public CreepHealResult RangedHeal(IPowerCreep target)
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

        public CreepTransferResult Transfer(IPowerCreep target, ResourceType resourceType, int? amount = null)
            => (CreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount = null)
            => (CreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepTransferResult Transfer(IScoreCollector target, ResourceType resourceType, int? amount = null)
            => (CreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepUpgradeControllerResult UpgradeController(IStructureController target)
            => (CreepUpgradeControllerResult)Native_UpgradeController(ProxyObject, target.ToJS());

        public CreepWithdrawResult Withdraw(IStructure target, ResourceType resourceType, int? amount = null)
            => (CreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepWithdrawResult Withdraw(ITombstone target, ResourceType resourceType, int? amount = null)
            => (CreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepWithdrawResult Withdraw(IRuin target, ResourceType resourceType, int? amount = null)
            => (CreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepWithdrawResult Withdraw(IScoreContainer target, ResourceType resourceType, int? amount = null)
            => (CreepWithdrawResult)Native_Withdraw(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        private BodyPart<BodyPartType>[] FetchBody()
        {
            Span<int> encodedBodyParts = stackalloc int[50];
            int num;
            unsafe
            {
                fixed (int* encodedBodyPartsPtr = encodedBodyParts)
                {
                    num = Native_GetEncodedBody(ProxyObject, (IntPtr)encodedBodyPartsPtr);
                }
            }
            if (bodyMemoryCache == null || bodyMemoryCache.Length != num)
            {
                bodyMemoryCache = new BodyPart<BodyPartType>[num];
            }
            for (int i = 0; i < num; ++i)
            {
                // Each body part encoded to a 32 bit int as 4 bytes
                // unused: b3
                // type: 0-8 (4 bits 0-15) b2
                // hits: 0-100 (7 bits 0-127) b1
                // boost: null or 0-85 (7 bits 0-127, 127 means null) b0
                int encodedBodyPart = encodedBodyParts[i];
                int type = encodedBodyPart >> 16;
                int hits = (encodedBodyPart >> 8) & 127;
                int boost = encodedBodyPart & 127;
                bodyMemoryCache[i] = new((BodyPartType)type, hits, boost == 127 ? null : (ResourceType)boost);
            }
            return bodyMemoryCache;
        }

        private int FetchTTL()
        {
            var ttlMaybe = ProxyObject.TryGetPropertyAsInt32(Names.TicksToLive);
            if (ttlMaybe != null) { return ttlMaybe.Value; }
            // This can return null on the first tick of an invader's existence, so compensate for that and hard code in 1500 for now
            if (Owner.Username == "Invader") { return 1500; }
            throw new JSException($"Creep.ticksToLive returned null");
        }

        public override string ToString()
            => $"Creep[{(Exists ? $"'{Name}'" : Id.ToString())}]({(Exists ? $"{RoomPosition}" : "DEAD")})";
    }
}
