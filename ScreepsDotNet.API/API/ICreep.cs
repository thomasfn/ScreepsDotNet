using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API
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
        // CreepBuildResult Build(IConstructionSite target);

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
