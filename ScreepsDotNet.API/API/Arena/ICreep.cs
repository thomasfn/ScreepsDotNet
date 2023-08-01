using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScreepsDotNet.API.Arena
{
    public enum BodyPartType
    {
        Attack,
        Carry,
        Heal,
        Move,
        RangedAttack,
        Work,
        Tough
    }

    public static class BodyPartTypeExtensions
    {
        public static string ToBodyString(this IEnumerable<BodyPartType> bodyParts)
            => $"[{string.Join(",", bodyParts.Select(x => x.ToString()))}]";
    }

    public readonly struct BodyPart : IEquatable<BodyPart>
    {
        public readonly BodyPartType Type;
        public readonly int Hits;

        public BodyPart(BodyPartType type, int hits)
        {
            Type = type;
            Hits = hits;
        }

        public override bool Equals(object? obj) => obj is BodyPart part && Equals(part);

        public bool Equals(BodyPart other)
            => Type == other.Type
            && Hits == other.Hits;

        public override int GetHashCode() => HashCode.Combine(Type, Hits);

        public static bool operator ==(BodyPart left, BodyPart right) => left.Equals(right);

        public static bool operator !=(BodyPart left, BodyPart right) => !(left == right);

        public override string ToString()
            => $"{Type}[{Hits}]";
    }

    /// <summary>
    /// Describes the number of body parts of a creep.
    /// </summary>
    public readonly struct BodyType : IEquatable<BodyType>
    {
        public static readonly BodyType Invalid = new();

        private readonly (BodyPartType bodyPartType, int quantity)[]? bodyPartTypes;
        private readonly int? hash;

        public ReadOnlySpan<(BodyPartType bodyPartType, int quantity)> BodyPartTypes => bodyPartTypes != null ? bodyPartTypes : ReadOnlySpan<(BodyPartType bodyPartType, int quantity)>.Empty;

        public bool IsValid => bodyPartTypes != null && bodyPartTypes.Length > 0;

        public IEnumerable<BodyPartType> AsBodyPartList
        {
            get
            {
                var seq = Enumerable.Empty<BodyPartType>();
                if (bodyPartTypes != null)
                {
                    foreach ((BodyPartType tupleBodyPartType, int quantity) in bodyPartTypes)
                    {
                        seq = seq.Concat(Enumerable.Repeat(tupleBodyPartType, quantity));
                    }
                }
                return seq;
            }
        }

        public int this[BodyPartType bodyPartType]
        {
            get
            {
                if (bodyPartTypes == null) { return 0; }
                foreach ((BodyPartType tupleBodyPartType, int quantity) in bodyPartTypes)
                {
                    if (tupleBodyPartType == bodyPartType) { return quantity; }
                }
                return 0;
            }
        }

        public BodyType(ReadOnlySpan<(BodyPartType bodyPartType, int quantity)> bodyPartTypeTuples)
        {
            bodyPartTypes = bodyPartTypeTuples.ToArray();
            Array.Sort(bodyPartTypes, (a, b) => (int)a.bodyPartType - (int)b.bodyPartType);
            hash = CalculateHash();
        }

        public BodyType(IEnumerable<BodyPartType> bodyPartTypes)
        {
            Span<(BodyPartType bodyPartType, int quantity)> bodyPartTuples = stackalloc (BodyPartType bodyPartType, int quantity)[bodyPartTypes.Count()];
            int bodyPartTuplesLength = 0;
            foreach (var bodyPartType in bodyPartTypes)
            {
                bool found = false;
                for (int i = 0; i < bodyPartTuplesLength; ++i)
                {
                    if (bodyPartTuples[i].bodyPartType == bodyPartType)
                    {
                        ++bodyPartTuples[i].quantity;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    bodyPartTuples[bodyPartTuplesLength] = (bodyPartType, 1);
                    ++bodyPartTuplesLength;
                }
            }

            this.bodyPartTypes = bodyPartTuples[..bodyPartTuplesLength].ToArray();
            Array.Sort(this.bodyPartTypes, (a, b) => (int)a.bodyPartType - (int)b.bodyPartType);
            hash = CalculateHash();
        }

        public bool Has(BodyPartType bodyPartType)
            => this[bodyPartType] > 0;

        private int CalculateHash()
        {
            if (bodyPartTypes == null || bodyPartTypes.Length == 0) { return HashCode.Combine(0); }
            int hash = bodyPartTypes[0].GetHashCode();
            for (int i = 1; i < bodyPartTypes.Length; ++i)
            {
                hash = HashCode.Combine(hash, bodyPartTypes[i].GetHashCode());
            }
            return hash;
        }

        public override bool Equals(object? obj) => obj is BodyType type && Equals(type);

        public bool Equals(BodyType other)
        {
            if (hash != other.hash) { return false; }
            if (other.bodyPartTypes == null) { return bodyPartTypes == null; }
            if (bodyPartTypes == null || bodyPartTypes.Length != other.bodyPartTypes.Length) { return false; }
            for (int i = 0; i < bodyPartTypes.Length; ++i)
            {
                if (bodyPartTypes[i] != other.bodyPartTypes[i]) { return false; }
            }
            return true;
        }

        public override int GetHashCode()
            => hash ?? HashCode.Combine(0);

        public static bool operator ==(BodyType left, BodyType right) => left.Equals(right);

        public static bool operator !=(BodyType left, BodyType right) => !(left == right);

        public override string ToString()
        {
            if (bodyPartTypes == null || bodyPartTypes.Length == 0) { return "BodyType(INVALID)"; }
            var sb = new StringBuilder();
            sb.Append("BodyType(");
            bool isFirst = true;
            foreach (var tuple in bodyPartTypes)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(tuple.quantity);
                sb.Append("x ");
                sb.Append(tuple.bodyPartType);

            }
            sb.Append(')');
            return sb.ToString();
        }
    }

    public static class BodyTypeExtensions
    {
        public static int GetSpawnCost(this IConstants constants, BodyType bodyType)
        {
            int totalCost = 0;
            foreach (var tuple in bodyType.BodyPartTypes)
            {
                totalCost += constants.GetBodyPartCost(tuple.bodyPartType) * tuple.quantity;
            }
            return totalCost;
        }
    }

    public enum CreepAttackResult
    {
        Ok = 0,
        NotOwner = -1,
        NoBodyPart = -12,
        InvalidTarget = -7,
        NotInRange = -9
    }

    public enum CreepBuildResult
    {
        Ok = 0,
        NotOwner = -1,
        NoBodyPart = -12,
        NotEnoughResources = -6,
        InvalidTarget = -7,
        NotInRange = -9
    }

    public enum CreepDropResult
    {
        Ok = 0,
        NotOwner = -1,
        InvalidArgs = -10,
        NotEnoughResources = -6,
    }

    public enum CreepHarvestResult
    {
        Ok = 0,
        NotOwner = -1,
        NoBodyPart = -12,
        InvalidTarget = -7,
        NotEnoughResources = -6,
        NotInRange = -9
    }

    public enum CreepHealResult
    {
        Ok = 0,
        NotOwner = -1,
        NoBodyPart = -12,
        InvalidTarget = -7,
        NotInRange = -9
    }

    public enum CreepMoveResult
    {
        Ok = 0,
        NotOwner = -1,
        NoBodyPart = -12,
        Tired = -11,
        InvalidArgs = -10
    }

    public enum CreepPickupResult
    {
        Ok = 0,
        NotOwner = -1,
        InvalidTarget = -7,
        Full = -8,
        NotInRange = -9,
    }

    public enum CreepPullResult
    {
        Ok = 0,
        NotOwner = -1,
        NoBodyPart = -12,
        Tired = -11,
        InvalidTarget = -7,
        InvalidArgs = -10
    }

    public enum CreepRangedMassAttackResult
    {
        Ok = 0,
        NotOwner = -1,
        NoBodyPart = -12
    }

    public enum CreepTransferResult
    {
        Ok = 0,
        NotOwner = -1,
        InvalidArgs = -10,
        InvalidTarget = -7,
        NotInRange = -9,
        Full = -8,
        NotEnoughResources = -6
    }

    public interface ICreep : IGameObject
    {
        /// <summary>
        /// An array describing the creep’s body
        /// </summary>
        IEnumerable<BodyPart> Body { get; }

        /// <summary>
        /// Gets the creep's body type.
        /// </summary>
        BodyType BodyType { get; }

        /// <summary>
        /// The movement fatigue indicator. If it is greater than zero, the creep cannot move
        /// </summary>
        double Fatigue { get; }

        /// <summary>
        /// The current amount of hit points of the creep
        /// </summary>
        int Hits { get; }

        /// <summary>
        /// The maximum amount of hit points of the creep
        /// </summary>
        int HitsMax { get; }

        /// <summary>
        /// Whether it is your creep
        /// </summary>
        bool My { get; }

        /// <summary>
        ///  A Store object that contains cargo of this creep.
        /// </summary>
        IStore Store { get; }

        /// <summary>
        /// Whether this creep is still being spawned
        /// </summary>
        bool Spawning { get; }

        /// <summary>
        /// Attack another creep in a short-ranged attack. Requires the ATTACK body part
        /// </summary>
        CreepAttackResult Attack(ICreep target);

        /// <summary>
        /// Attack a structure in a short-ranged attack. Requires the ATTACK body part
        /// </summary>
        CreepAttackResult Attack(IStructure target);

        /// <summary>
        /// Build a structure at the target construction site using carried energy.
        /// Requires WORK and CARRY body parts
        /// </summary>
        CreepBuildResult Build(IConstructionSite target);

        /// <summary>
        /// Drop a resource on the ground
        /// </summary>
        /// <param name="amount">amount The amount of resource units to be dropped. If omitted, all the available carried amount is used</param>
        CreepDropResult Drop(ResourceType resource, int? amount = null);

        /// <summary>
        /// Harvest energy from the source. Requires the WORK body part
        /// </summary>
        CreepHarvestResult Harvest(ISource target);

        /// <summary>
        /// Heal self or another creep nearby. Requires the HEAL body part.
        /// </summary>
        CreepHealResult Heal(ICreep target);

        /// <summary>
        /// Move the creep one square in the specified direction. Requires the MOVE body part
        /// </summary>
        CreepMoveResult Move(Direction direction);

        /// <summary>
        /// Find the optimal path to the target and move to it. Requires the MOVE body part
        /// </summary>
        CreepMoveResult MoveTo(IPosition target/*, FindPathOptions? options = null*/);

        /// <summary>
        /// Pick up an item (a dropped piece of resource). Requires the CARRY body part.
        /// </summary>
        CreepPickupResult Pickup(IResource target);

        /// <summary>
        /// Help another creep to follow this creep. Requires the MOVE body part
        /// </summary>
        CreepPullResult Pull(ICreep target);

        /// <summary>
        /// A ranged attack against another creep. Requires the RANGED_ATTACK body part
        /// </summary>
        CreepAttackResult RangedAttack(ICreep target);

        /// <summary>
        /// A ranged attack against a structure. Requires the RANGED_ATTACK body part
        /// </summary>
        CreepAttackResult RangedAttack(IStructure target);

        /// <summary>
        /// Heal another creep at a distance. Requires the HEAL body part
        /// </summary>
        CreepHealResult RangedHeal(ICreep target);

        /// <summary>
        /// A ranged attack against all hostile creeps or structures within 3 squares range.  Requires the RANGED_ATTACK body part
        /// </summary>
        CreepRangedMassAttackResult RangedMassAttack();

        /// <summary>
        /// Transfer resource from the creep to another object
        /// </summary>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used</param>
        CreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Transfer resource from the creep to another object
        /// </summary>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used</param>
        CreepTransferResult Transfer(ICreep target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Withdraw resources from a structure
        /// </summary>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used</param>
        CreepTransferResult Withdraw(IStructure target, ResourceType resourceType, int? amount = null);
    }
}
