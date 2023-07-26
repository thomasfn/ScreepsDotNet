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

        ///**
        // * Build a structure at the target construction site using carried energy.
        // * Requires {@link WORK} and {@link CARRY} body parts
        // * @param target The target construction site to be built
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //build(target: ConstructionSite) : CreepBuildResult;

        /// <summary>
        /// Attack a structure in a short-ranged attack. Requires the ATTACK body part
        /// </summary>
        // CreepBuildResult Build(IConstructionSite target);

        ///**
        // * Drop a resource on the ground
        // * @param resource One of the RESOURCE_* constants
        // * @param amount The amount of resource units to be dropped. If omitted, all the available carried amount is used
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //drop(resource: ResourceType, amount?: number) : CreepDropResult;

        ///**
        // * Harvest energy from the source. Requires the {@link WORK} body part
        // * @param target The object to be harvested
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //harvest(target: Source) : CreepHarvestResult;

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

        ///**
        // * Pick up an item (a dropped piece of resource). Requires the {@link CARRY} body part.
        // * @param target The target object to be picked up
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //pickup(target: Resource) : CreepPickupResult;

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
        CreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount);

        /// <summary>
        /// Transfer resource from the creep to another object
        /// </summary>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used</param>
        CreepTransferResult Transfer(ICreep target, ResourceType resourceType, int? amount);

        /// <summary>
        /// Withdraw resources from a structure
        /// </summary>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used</param>
        CreepTransferResult Withdraw(IStructure target, ResourceType resourceType, int? amount);
    }
}
