using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum BodyPartType
    {
        Move,
        Work,
        Carry,
        Attack,
        RangedAttack,
        Tough,
        Heal,
        Claim
    }

    public enum CreepAttackResult
    {
        Ok = 0,
        NotOwner = -1,
        Busy = -4,
        InvalidTarget = -7,
        NotInRange = -9,
        NoBodyPart = -12
    }

    public enum CreepAttackControllerResult
    {
        Ok = 0,
        NotOwner = -1,
        Busy = -4,
        InvalidTarget = -7,
        NotInRange = -9,
        Tired = -11,
        NoBodyPart = -12
    }

    public enum CreepBuildResult
    {
        Ok = 0,
        NotOwner = -1,
        Busy = -4,
        NotEnoughResources = -6,
        InvalidTarget = -7,
        NotInRange = -9,
        NoBodyPart = -12
    }

    public enum CreepOrderType
    {
        Attack,
        AttackController,
        Build
    }

    public interface ICreep : IRoomObject
    {
        /// <summary>
        /// An array describing the creep’s body.
        /// </summary>
        IEnumerable<BodyPart<BodyPartType>> Body { get; }

        /// <summary>
        /// Gets the creep's body type.
        /// </summary>
        BodyType<BodyPartType> BodyType { get; }

        /// <summary>
        /// The movement fatigue indicator. If it is greater than zero, the creep cannot move.
        /// </summary>
        int Fatigue { get; }

        /// <summary>
        /// The current amount of hit points of the creep.
        /// </summary>
        int Hits { get; }

        /// <summary>
        /// The maximum amount of hit points of the creep.
        /// </summary>
        int HitsMax { get; }

        /// <summary>
        /// A unique object identificator. You can use Game.getObjectById method to retrieve an object instance by its id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// A shorthand to Memory.creeps[creep.name]. You can use it for quick access the creep’s specific memory data object.
        /// </summary>
        object Memory { get; }

        /// <summary>
        /// Whether it is your creep or foe.
        /// </summary>
        bool My { get; }

        /// <summary>
        /// Creep’s name. You can choose the name while creating a new creep, and it cannot be changed later. This name is a hash key to access the creep via the Game.creeps object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// An object with the creep’s owner info.
        /// </summary>
        OwnerInfo Owner { get; }

        /// <summary>
        /// The text message that the creep was saying at the last tick.
        /// </summary>
        string? Saying { get; }

        /// <summary>
        /// Whether this creep is still being spawned.
        /// </summary>
        bool Spawning { get; }

        /// <summary>
        /// A Store object that contains cargo of this creep.
        /// </summary>
        IStore Store { get; }

        /// <summary>
        /// The remaining amount of game ticks after which the creep will die.
        /// </summary>
        int TicksToLive { get; }

        /// <summary>
        /// Attack another creep in a short-ranged attack.
        /// Requires the ATTACK body part. If the target is inside a rampart, then the rampart is attacked instead.
        /// The target has to be at adjacent square to the creep.
        /// If the target is a creep with ATTACK body parts and is not inside a rampart, it will automatically hit back at the attacker.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepAttackResult Attack(ICreep target);

        /// <summary>
        /// Attack a power creep in a short-ranged attack.
        /// Requires the ATTACK body part. If the target is inside a rampart, then the rampart is attacked instead.
        /// The target has to be at adjacent square to the creep.
        /// If the target is a creep with ATTACK body parts and is not inside a rampart, it will automatically hit back at the attacker.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        // CreepAttackResult Attack(IPowerCreep target);

        /// <summary>
        /// Attack a structure in a short-ranged attack.
        /// Requires the ATTACK body part. If the target is inside a rampart, then the rampart is attacked instead.
        /// The target has to be at adjacent square to the creep.
        /// If the target is a creep with ATTACK body parts and is not inside a rampart, it will automatically hit back at the attacker.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        // CreepAttackResult Attack(IStructure target);

        /// <summary>
        /// Decreases the controller's downgrade timer by 300 ticks per every CLAIM body part, or reservation timer by 1 tick per every CLAIM body part.
        /// If the controller under attack is owned, it cannot be upgraded or attacked again for the next 1,000 ticks.
        /// The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        // CreepAttackControllerResult AttackController(IStructureController target);

        /// <summary>
        /// Build a structure at the target construction site using carried energy.
        /// Requires WORK and CARRY body parts.
        /// The target has to be within 3 squares range of the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        // CreepBuildResult Build(IConstructionSite target);

        /// <summary>
        /// Cancel the order given during the current game tick.
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        void CancelOrder(CreepOrderType orderType);

        // claimController
        // dismantle
        // drop
        // generateSafeMode
        // getActiveBodyparts
        // harvest
        // heal
        // move
        // moveByPath
        // moveTo
        // notifyWhenAttacked
        // pickup
        // pull
        // rangedAttack
        // rangedHeal
        // rangedMassAttack
        // repair
        // reserveController
        // say
        // signController
        // suicide
        // transfer
        // upgradeController
        // withdraw
    }
}
