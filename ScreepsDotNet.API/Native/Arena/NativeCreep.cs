using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    internal static class BodyPartTypeExtensions
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
                _ => throw new NotImplementedException($"Unknown body part type '{bodyPartType}'"),
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
                _ => throw new NotImplementedException($"Unknown body part type '{str}'"),
            };
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeCreep : NativeGameObject, ICreep
    {
        #region Imports

        [JSImport("Creep.attack", "game/prototypes/wrapped")]
        
        internal static partial int Native_Attack(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.build", "game/prototypes/wrapped")]
        
        internal static partial int Native_Build(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.drop", "game/prototypes/wrapped")]
        
        internal static partial int Native_Drop(JSObject proxyObject, string resourceType, int? amount);

        [JSImport("Creep.harvest", "game/prototypes/wrapped")]
        
        internal static partial int Native_Harvest(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.heal", "game/prototypes/wrapped")]
        
        internal static partial int Native_Heal(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("Creep.move", "game/prototypes/wrapped")]
        
        internal static partial int Native_Move(JSObject proxyObject, int direction);

        [JSImport("Creep.moveTo", "game/prototypes/wrapped")]
        
        internal static partial int Native_MoveTo(JSObject proxyObject, JSObject targetProxyObject);

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

        [JSImport("Creep.transfer", "game/prototypes/wrapped")]
        
        internal static partial int Native_Transfer(JSObject proxyObject, JSObject targetProxyObject, string resourceType, int? amount);

        [JSImport("Creep.withdraw", "game/prototypes/wrapped")]
        
        internal static partial int Native_Withdraw(JSObject proxyObject, JSObject targetProxyObject, string resourceType, int? amount);

        #endregion

        private BodyType<BodyPartType>? cachedBodyType;

        public IEnumerable<BodyPart<BodyPartType>> Body
            => JSUtils.GetObjectArrayOnObject(ProxyObject, "body")!
                .Select(x => new BodyPart<BodyPartType>(x.GetPropertyAsString("type")!.ParseBodyPartType(), x.GetPropertyAsInt32("hits"), null))
                .ToArray();

        public BodyType<BodyPartType> BodyType => cachedBodyType ??= new(Body.Select(x => x.Type));

        public double Fatigue => ProxyObject.GetPropertyAsDouble("fatigue");

        public int Hits => ProxyObject.GetPropertyAsInt32("hits");

        public int HitsMax => ProxyObject.GetPropertyAsInt32("hitsMax");

        public bool My => ProxyObject.GetPropertyAsBoolean("my");

        public IStore Store => new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public bool Spawning => ProxyObject.GetPropertyAsBoolean("spawning");

        public NativeCreep(JSObject proxyObject)
            : base(proxyObject)
        { }

        public CreepAttackResult Attack(ICreep target)
            => (CreepAttackResult)Native_Attack(ProxyObject, target.ToJS());

        public CreepAttackResult Attack(IStructure target)
            => (CreepAttackResult)Native_Attack(ProxyObject, target.ToJS());

        public CreepBuildResult Build(IConstructionSite target)
            => (CreepBuildResult)Native_Build(ProxyObject, target.ToJS());

        public CreepDropResult Drop(ResourceType resource, int? amount = null)
            => (CreepDropResult)Native_Drop(ProxyObject, resource.ToJS(), amount);

        public CreepHarvestResult Harvest(ISource target)
            => (CreepHarvestResult)Native_Harvest(ProxyObject, target.ToJS());

        public CreepHealResult Heal(ICreep target)
            => (CreepHealResult)Native_Heal(ProxyObject, target.ToJS());

        public CreepMoveResult Move(Direction direction)
            => (CreepMoveResult)Native_Move(ProxyObject, (int)direction);

        public CreepMoveResult MoveTo(IPosition target)
            => (CreepMoveResult)Native_MoveTo(ProxyObject, target.ToJS());

        public CreepPickupResult Pickup(IResource target)
            => (CreepPickupResult)Native_Pickup(ProxyObject, target.ToJS());

        public CreepPullResult Pull(ICreep target)
            => (CreepPullResult)Native_Pull(ProxyObject, target.ToJS());

        public CreepAttackResult RangedAttack(ICreep target)
            => (CreepAttackResult)Native_RangedAttack(ProxyObject, target.ToJS());

        public CreepAttackResult RangedAttack(IStructure target)
            => (CreepAttackResult)Native_RangedAttack(ProxyObject, target.ToJS());

        public CreepHealResult RangedHeal(ICreep target)
            => (CreepHealResult)Native_RangedHeal(ProxyObject, target.ToJS());

        public CreepRangedMassAttackResult RangedMassAttack()
            => (CreepRangedMassAttackResult)Native_RangedMassAttack(ProxyObject);

        public CreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount)
            => (CreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepTransferResult Transfer(ICreep target, ResourceType resourceType, int? amount)
            => (CreepTransferResult)Native_Transfer(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepTransferResult Withdraw(IStructure target, ResourceType resourceType, int? amount)
            => (CreepTransferResult)Native_Withdraw(ProxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public override string ToString()
            => $"Creep({Id}, {Position})";
    }
}
