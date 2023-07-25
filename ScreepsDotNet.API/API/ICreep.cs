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
    }

    public enum CreepAttackResult
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

        ///** A {@link Store} object that contains cargo of this creep */
        //store: Store;

        /// <summary>
        /// Whether this creep is still being spawned
        /// </summary>
        bool Spawning { get; }

        ///**
        // * Attack another creep or structure in a short-ranged attack. Requires the {@link ATTACK} body part
        // * @param target The target object
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //attack(target: Creep|Structure) : CreepAttackResult;

        /// <summary>
        /// Attack another creep in a short-ranged attack. Requires the ATTACK body part
        /// </summary>
        CreepAttackResult Attack(ICreep target);

        /// <summary>
        /// Attack another structure in a short-ranged attack. Requires the ATTACK body part
        /// </summary>
        //CreepAttackResult Attack(IStructure target);

        ///**
        // * Build a structure at the target construction site using carried energy.
        // * Requires {@link WORK} and {@link CARRY} body parts
        // * @param target The target construction site to be built
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //build(target: ConstructionSite) : CreepBuildResult;

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

        ///**
        // * Heal self or another creep nearby. Requires the {@link HEAL} body part.
        // * @param target The target creep object
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //heal(target: Creep) : CreepHealResult;

        ///**
        // * Move the creep one square in the specified direction. Requires the {@link MOVE} body part
        // * @param direction The direction to move
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //move(direction: Direction) : CreepMoveResult;

        ///**
        // * Find the optimal path to the target and move to it. Requires the {@link MOVE} body part
        // * @param target The target to move to. Can be a {@link GameObject} or any object containing x and y properties.
        // * @param options An object with additional options that are passed to {@link findPath}
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //moveTo(target: Position, options?: FindPathOptions) : CreepMoveResult;

        /// <summary>
        ///  Find the optimal path to the target and move to it. Requires the MOVE body part
        /// </summary>
        CreepMoveResult MoveTo(IPosition target/*, FindPathOptions? options = null*/);

        ///**
        // * Pick up an item (a dropped piece of resource). Requires the {@link CARRY} body part.
        // * @param target The target object to be picked up
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //pickup(target: Resource) : CreepPickupResult;

        ///**
        // * Help another creep to follow this creep. Requires the {@link MOVE} body part
        // * @param target The target creep
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //pull(target: Creep) : CreepPullResult;

        ///**
        // * A ranged attack against another creep or structure. Requires the {@link RANGED_ATTACK} body part
        // * @param target The target object to be attacked
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //rangedAttack(target: Creep|Structure) : CreepRangedAttackResult;

        ///**
        // * Heal another creep at a distance. Requires the {@link HEAL} body part
        // * @param target The target creep object
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //rangedHeal(target: Creep) : CreepRangedHealResult;

        ///**
        // * A ranged attack against all hostile creeps or structures within 3 squares range.  Requires the {@link RANGED_ATTACK} body part
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //rangedMassAttack() : CreepRangedMassAttackResult;

        ///**
        // * Transfer resource from the creep to another object
        // * @param target The target object
        // * @param resource One of the RESOURCE_* constants
        // * @param amount The amount of resources to be transferred. If omitted, all the available carried amount is used
        // */
        //transfer(target: Structure|Creep, resource: ResourceType, amount?: number) : CreepTransferResult;

        ///**
        // * Withdraw resources from a structure
        // * @param target The target structure
        // * @param resource One of the RESOURCE_* constants
        // * @param amount The amount of resources to be transferred. If omitted, all the available carried amount is used
        // * @returns Either {@link OK} or one of ERR_* error codes
        // */
        //withdraw(target: Structure, resource: ResourceType, amount?: number) : CreepWithdrawResult;
    }
}
