using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    internal static class BodyPartTypeExtensions
    {
        private static readonly Name[] bodyPartTypeToName =
        [
            Name.Create("move"),
            Name.Create("work"),
            Name.Create("carry"),
            Name.Create("attack"),
            Name.Create("ranged_attack"),
            Name.Create("heal"),
            Name.Create("tough"),
        ];

        private static readonly Dictionary<Name, BodyPartType> nameToBodyPartType = [];

        static BodyPartTypeExtensions()
        {
            for (int i = 0; i < bodyPartTypeToName.Length; ++i)
            {
                nameToBodyPartType.Add(bodyPartTypeToName[i], (BodyPartType)i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Name ToJS(this BodyPartType bodyPartType)
            => bodyPartTypeToName[(int)bodyPartType];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BodyPartType ParseBodyPartType(this Name str)
            => nameToBodyPartType.TryGetValue(str, out var bodyPartType) ? bodyPartType : throw new Exception($"Unknown body part type '{str}'");
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
        internal static partial int Native_Drop(JSObject proxyObject, Name resourceType, int? amount);

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
        internal static partial int Native_Transfer(JSObject proxyObject, JSObject targetProxyObject, Name resourceType, int? amount);

        [JSImport("Creep.withdraw", "game/prototypes/wrapped")]
        internal static partial int Native_Withdraw(JSObject proxyObject, JSObject targetProxyObject, Name resourceType, int? amount);

        [JSImport("Creep.getEncodedBody", "game/prototypes/wrapped")]
        internal static partial int Native_GetEncodedBody(JSObject proxyObject, IntPtr outEncodedBodyParts);

        #endregion

        private BodyPart<BodyPartType>[]? bodyMemoryCache;
        private BodyPart<BodyPartType>[]? bodyCache;
        private BodyType<BodyPartType>? bodyTypeCache;
        private int? fatigueCache;
        private int? hitsCache;
        private int? hitsMaxCache;
        private bool? myCache;
        private NativeStore? storeCache;
        private bool? spawningCache;

        public IEnumerable<BodyPart<BodyPartType>> Body => CachePerTick(ref bodyCache) ??= FetchBody();

        public BodyType<BodyPartType> BodyType => CacheLifetime(ref bodyTypeCache) ??= new(Body.Select(x => x.Type));

        public int Fatigue => CachePerTick(ref fatigueCache) ??= proxyObject.GetPropertyAsInt32(Names.Fatigue);

        public int Hits => CachePerTick(ref hitsCache) ??= proxyObject.GetPropertyAsInt32(Names.Hits);

        public int HitsMax => CacheLifetime(ref hitsMaxCache) ??= proxyObject.GetPropertyAsInt32(Names.HitsMax);

        public bool My => CacheLifetime(ref myCache) ??= proxyObject.GetPropertyAsBoolean(Names.My);

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(proxyObject.GetPropertyAsJSObject(Names.Store));

        public bool Spawning => CachePerTick(ref spawningCache) ??= proxyObject.GetPropertyAsBoolean(Names.Spawning);

        public NativeCreep(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, true)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            bodyCache = null;
            fatigueCache = null;
            hitsCache = null;
            storeCache?.Dispose();
            storeCache = null;
            spawningCache = null;
        }

        public CreepAttackResult Attack(ICreep target)
            => (CreepAttackResult)Native_Attack(proxyObject, target.ToJS());

        public CreepAttackResult Attack(IStructure target)
            => (CreepAttackResult)Native_Attack(proxyObject, target.ToJS());

        public CreepBuildResult Build(IConstructionSite target)
            => (CreepBuildResult)Native_Build(proxyObject, target.ToJS());

        public CreepDropResult Drop(ResourceType resource, int? amount = null)
            => (CreepDropResult)Native_Drop(proxyObject, resource.ToJS(), amount);

        public CreepHarvestResult Harvest(ISource target)
            => (CreepHarvestResult)Native_Harvest(proxyObject, target.ToJS());

        public CreepHealResult Heal(ICreep target)
            => (CreepHealResult)Native_Heal(proxyObject, target.ToJS());

        public CreepMoveResult Move(Direction direction)
            => (CreepMoveResult)Native_Move(proxyObject, (int)direction);

        public CreepMoveResult MoveTo(IPosition target)
            => (CreepMoveResult)Native_MoveTo(proxyObject, target.ToJS());

        public CreepPickupResult Pickup(IResource target)
            => (CreepPickupResult)Native_Pickup(proxyObject, target.ToJS());

        public CreepPullResult Pull(ICreep target)
            => (CreepPullResult)Native_Pull(proxyObject, target.ToJS());

        public CreepAttackResult RangedAttack(ICreep target)
            => (CreepAttackResult)Native_RangedAttack(proxyObject, target.ToJS());

        public CreepAttackResult RangedAttack(IStructure target)
            => (CreepAttackResult)Native_RangedAttack(proxyObject, target.ToJS());

        public CreepHealResult RangedHeal(ICreep target)
            => (CreepHealResult)Native_RangedHeal(proxyObject, target.ToJS());

        public CreepRangedMassAttackResult RangedMassAttack()
            => (CreepRangedMassAttackResult)Native_RangedMassAttack(proxyObject);

        public CreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount)
            => (CreepTransferResult)Native_Transfer(proxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepTransferResult Transfer(ICreep target, ResourceType resourceType, int? amount)
            => (CreepTransferResult)Native_Transfer(proxyObject, target.ToJS(), resourceType.ToJS(), amount);

        public CreepTransferResult Withdraw(IStructure target, ResourceType resourceType, int? amount)
            => (CreepTransferResult)Native_Withdraw(proxyObject, target.ToJS(), resourceType.ToJS(), amount);

        private BodyPart<BodyPartType>[] FetchBody()
        {
            Span<short> encodedBodyParts = stackalloc short[50];
            int num;
            unsafe
            {
                fixed (short* encodedBodyPartsPtr = encodedBodyParts)
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
                // Each body part encoded to a 16 bit int as 2 bytes
                // type: 0-8 (4 bits 0-15) b1
                // hits: 0-100 (7 bits 0-127) b0
                int encodedBodyPart = encodedBodyParts[i];
                int type = encodedBodyPart >> 8;
                int hits = encodedBodyPart & 127;
                bodyMemoryCache[i] = new((BodyPartType)type, hits, null);
            }
            return bodyMemoryCache;
        }

        public override string ToString()
            => Exists ? $"Creep({Id}, {Position})" : $"Creep(DEAD)";
    }
}
