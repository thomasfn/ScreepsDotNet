using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

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

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeCreep : NativeGameObject, ICreep
    {
        #region Imports

        [JSImport("Creep.attack", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Attack([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.build", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Build([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.drop", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Drop([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string resourceType, [JSMarshalAs<JSType.Number>] int? amount);

        [JSImport("Creep.harvest", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Harvest([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.heal", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Heal([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.move", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Move([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int direction);

        [JSImport("Creep.moveTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_MoveTo([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

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

        [JSImport("Creep.transfer", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Transfer([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject, [JSMarshalAs<JSType.String>] string resourceType, [JSMarshalAs<JSType.Number>] int? amount);

        [JSImport("Creep.withdraw", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Withdraw([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject, [JSMarshalAs<JSType.String>] string resourceType, [JSMarshalAs<JSType.Number>] int? amount);

        #endregion

        private BodyType<BodyPartType>? cachedBodyType;

        public IEnumerable<BodyPart<BodyPartType>> Body
            => NativeGameObjectUtils.GetArrayOnObject(ProxyObject, "body")!
                .Select(x => new BodyPart<BodyPartType>(x.GetPropertyAsString("type")!.ParseBodyPartType(), x.GetPropertyAsInt32("hits")))
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
