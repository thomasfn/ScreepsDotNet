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
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid attackable object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are no ATTACK body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepAttackControllerResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid owned or reserved controller object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// You have to wait until the next attack is possible.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// There are not enough CLAIM body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepBuildResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The creep does not have any carried energy.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The target is not a valid construction site object or the structure cannot be built here (probably because of a creep at the same square).
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are no WORK body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepClaimControllerResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid neutral controller object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// You cannot claim more than 3 rooms in the Novice Area.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are not enough CLAIM body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12,
        /// <summary>
        /// Your Global Control Level is not enough.
        /// </summary>
        GclNotEnough = -15
    }

    public enum CreepDismantleResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid structure object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are no WORK body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepDropResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The creep does not have the given amount of resources.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The resourceType is not a valid RESOURCE_* constants.
        /// </summary>
        InvalidArgs = -10
    }

    public enum CreepGenerateSafeModeResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The creep does not have the given amount of resources.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The target is not a valid controller object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The resourceType is not a valid RESOURCE_* constants.
        /// </summary>
        InvalidArgs = -10
    }

    public enum CreepHarvestResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// Extractor not found. You must build an extractor structure to harvest minerals.
        /// </summary>
        NotFound = -5,
        /// <summary>
        /// The target does not contain any harvestable energy or mineral.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The target is not a valid source or mineral object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The extractor or the deposit is still cooling down.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// There are no WORK body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepHealResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid creep object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are no HEAL body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepMoveResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The provided direction is incorrect.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// The fatigue indicator of the creep is non-zero.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// There are no HEAL body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepNotifyWhenAttackedResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// enable argument is not a boolean value.
        /// </summary>
        InvalidArgs = -10,
    }

    public enum CreepPickupResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid object to pick up.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The creep cannot receive any more resource.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
    }

    public enum CreepPullResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target provided is invalid.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
    }

    public enum CreepRangedAttackResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid attackable object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are no RANGED_ATTACK body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepRangedMassAttackResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// There are no RANGED_ATTACK body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepRepairResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The creep does not have any carried energy.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The target is not a valid structure object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are no WORK body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepReserveControllerResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid neutral controller object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are no CLAIM body parts in this creep’s body.
        /// </summary>
        NoBodyPart = -12
    }

    public enum CreepSayResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4
    }

    public enum CreepSignControllerResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9
    }

    public enum CreepSuicideResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4
    }

    public enum CreepTransferResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The creep does not have the given amount of resources.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The target is not a valid object to pick up.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The creep cannot receive any more resource.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The resourceType is not one of the RESOURCE_* constants, or the amount is incorrect.
        /// </summary>
        InvalidArgs = -10
    }

    public enum CreepUpgradeControllerResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The creep does not have any carried energy.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The target is not a valid controller object, or the controller upgrading is blocked.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// There are no WORK body parts in this creep’s body.
        /// </summary>
        NoBodypart = -10
    }

    public enum CreepWithdrawResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is still being spawned.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target does not have the given amount of resources.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The target is not a valid object which can contain the specified resource.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The creep's carry is full.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The resourceType is not one of the RESOURCE_* constants, or the amount is incorrect.
        /// </summary>
        InvalidArgs = -10
    }

    public enum CreepOrderType
    {
        Attack,
        AttackController,
        Build,
        ClaimController,
        Dismantle,
        Drop,
        GenerateSafeMode,
        Harvest,
        Heal,
        Move,
        Pickup,
        Pull,
        RangedAttack,
        RangedMassAttack,
        Repair,
        ReserveController,
        Say,
        SignController,
        Suicide,
        Transfer,
        UpgradeController,
        Withdraw
    }

    /// <param name="ReusePath">
    /// This option enables reusing the path found along multiple game ticks.
    /// It allows to save CPU time, but can result in a slightly slower creep reaction behavior.
    /// The path is stored into the creep's memory to the _move property.
    /// The reusePath value defines the amount of ticks which the path should be reused for.
    /// The default value is 5.
    /// Increase the amount to save more CPU, decrease to make the movement more consistent.
    /// Set to 0 if you want to disable path reusing.
    /// </param>
    /// <param name="SerializeMemory">
    /// If reusePath is enabled and this option is set to true, the path will be stored in memory in the short serialized form using Room.serializePath.
    /// The default value is true.
    /// </param>
    /// <param name="NoPathFinding">
    /// If this option is set to true, moveTo method will return ERR_NOT_FOUND if there is no memorized path to reuse.
    /// This can significantly save CPU time in some cases.
    /// The default value is false.
    /// </param>
    /// <param name="VisualizePathStyle">
    /// Draw a line along the creep’s path using RoomVisual.poly.
    /// You can provide either an empty object or custom style parameters.
    /// </param>
    /// <param name="FindPathOptions">
    /// Any options supported by Room.findPath method.
    /// </param>
    public readonly record struct MoveToOptions
    (
        int ReusePath = 5,
        bool SerializeMemory = true,
        bool NoPathFinding = false,
        PolyVisualStyle? VisualizePathStyle = null,
        FindPathOptions? FindPathOptions = null
    );

    /// <summary>
    /// Creeps are your units. Creeps can move, harvest energy, construct structures, attack another creeps, and perform other actions. Each creep consists of up to 50 body parts.
    /// </summary>
    public interface ICreep : IRoomObject, IWithId, IWithName, IWithStore
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
        /// A shorthand to Memory.creeps[creep.name]. You can use it for quick access the creep’s specific memory data object.
        /// </summary>
        IMemoryObject Memory { get; }

        /// <summary>
        /// Whether it is your creep or foe.
        /// </summary>
        bool My { get; }

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
        CreepAttackResult Attack(IPowerCreep target);

        /// <summary>
        /// Attack a structure in a short-ranged attack.
        /// Requires the ATTACK body part. If the target is inside a rampart, then the rampart is attacked instead.
        /// The target has to be at adjacent square to the creep.
        /// If the target is a creep with ATTACK body parts and is not inside a rampart, it will automatically hit back at the attacker.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepAttackResult Attack(IStructure target);

        /// <summary>
        /// Decreases the controller's downgrade timer by 300 ticks per every CLAIM body part, or reservation timer by 1 tick per every CLAIM body part.
        /// If the controller under attack is owned, it cannot be upgraded or attacked again for the next 1,000 ticks.
        /// The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepAttackControllerResult AttackController(IStructureController target);

        /// <summary>
        /// Build a structure at the target construction site using carried energy.
        /// Requires WORK and CARRY body parts.
        /// The target has to be within 3 squares range of the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepBuildResult Build(IConstructionSite target);

        /// <summary>
        /// Cancel the order given during the current game tick.
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        void CancelOrder(CreepOrderType orderType);

        /// <summary>
        /// Claims a neutral controller under your control.
        /// Requires the CLAIM body part.
        /// The target has to be at adjacent square to the creep.
        /// You need to have the corresponding Global Control Level in order to claim a new room.
        /// If you don't have enough GCL, consider reserving this room instead.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        CreepClaimControllerResult ClaimController(IStructureController controller);

        /// <summary>
        /// Dismantles any structure that can be constructed (even hostile) returning 50% of the energy spent on its repair.
        /// Requires the WORK body part.
        /// If the creep has an empty CARRY body part, the energy is put into it; otherwise it is dropped on the ground.
        /// The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepDismantleResult Dismantle(IStructure target);

        /// <summary>
        /// Drop this resource on the ground.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount">The amount of resource units to be dropped. If omitted, all the available carried amount is used.</param>
        /// <returns></returns>
        CreepDropResult Drop(ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Add one more available safe mode activation to a room controller.
        /// The creep has to be at adjacent square to the target room controller and have 1000 ghodium resource.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        CreepGenerateSafeModeResult GenerateSafeMode(IStructureController controller);

        /// <summary>
        /// Get the quantity of live body parts of the given type. Fully damaged parts do not count.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        int GetActiveBodyparts(BodyPartType type);

        /// <summary>
        /// Harvest energy from the source.
        /// Requires the WORK body part.
        /// If the creep has an empty CARRY body part, the harvested resource is put into it; otherwise it is dropped on the ground.
        /// The target has to be at an adjacent square to the creep.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        CreepHarvestResult Harvest(ISource source);

        /// <summary>
        /// Harvest resources from minerals.
        /// Requires the WORK body part.
        /// If the creep has an empty CARRY body part, the harvested resource is put into it; otherwise it is dropped on the ground.
        /// The target has to be at an adjacent square to the creep.
        /// </summary>
        /// <param name="mineral"></param>
        /// <returns></returns>
        CreepHarvestResult Harvest(IMineral mineral);

        /// <summary>
        /// Harvest resources from deposits.
        /// Requires the WORK body part.
        /// If the creep has an empty CARRY body part, the harvested resource is put into it; otherwise it is dropped on the ground.
        /// The target has to be at an adjacent square to the creep.
        /// </summary>
        /// <param name="deposit"></param>
        /// <returns></returns>
        CreepHarvestResult Harvest(IDeposit deposit);

        /// <summary>
        /// Heal self or another creep.
        /// It will restore the target creep’s damaged body parts function and increase the hits counter.
        /// Requires the HEAL body part.
        /// The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepHealResult Heal(ICreep target);

        /// <summary>
        /// Heal a power creep.
        /// It will restore the target creep’s damaged body parts function and increase the hits counter.
        /// Requires the HEAL body part.
        /// The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepHealResult Heal(IPowerCreep target);

        /// <summary>
        /// Move the creep one square in the specified direction.
        /// Requires the MOVE body part, or another creep nearby pulling the creep.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        CreepMoveResult Move(Direction direction);

        /// <summary>
        /// Move the creep using the specified predefined path. Requires the MOVE body part.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        CreepMoveResult MoveByPath(IEnumerable<PathStep> path);

        /// <summary>
        /// Move the creep using the specified predefined path. Requires the MOVE body part.
        /// </summary>
        /// <param name="serialisedPath"></param>
        /// <returns></returns>
        CreepMoveResult MoveByPath(string serialisedPath);

        /// <summary>
        /// Find the optimal path to the target within the same room and move to it.
        /// A shorthand to consequent calls of pos.findPathTo() and move() methods.
        /// If the target is in another room, then the corresponding exit will be used as a target. Requires the MOVE body part.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        CreepMoveResult MoveTo(Position target, MoveToOptions? opts = null);

        /// <summary>
        /// Find the optimal path to the target within the same room and move to it.
        /// A shorthand to consequent calls of pos.findPathTo() and move() methods.
        /// If the target is in another room, then the corresponding exit will be used as a target. Requires the MOVE body part.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        CreepMoveResult MoveTo(RoomPosition target, MoveToOptions? opts = null);

        /// <summary>
        /// Toggle auto notification when the creep is under attack. The notification will be sent to your account email. Turned on by default.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        CreepNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled);

        /// <summary>
        /// Pick up an item (a dropped piece of energy).
        /// Requires the CARRY body part.
        /// The target has to be at adjacent square to the creep or at the same square.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepPickupResult Pickup(IResource target);

        /// <summary>
        /// Help another creep to follow this creep.
        /// The fatigue generated for the target's move will be added to the creep instead of the target.
        /// Requires the MOVE body part.
        /// The target has to be at adjacent square to the creep.
        /// The creep must move elsewhere, and the target must move towards the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepPullResult Pull(ICreep target);

        /// <summary>
        /// A ranged attack against another creep or structure.
        /// Requires the RANGED_ATTACK body part.
        /// If the target is inside a rampart, the rampart is attacked instead.
        /// The target has to be within 3 squares range of the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepRangedAttackResult RangedAttack(ICreep target);

        /// <summary>
        /// A ranged attack against another creep or structure.
        /// Requires the RANGED_ATTACK body part.
        /// If the target is inside a rampart, the rampart is attacked instead.
        /// The target has to be within 3 squares range of the creep.
        /// </summary>
        /// <param name="target"></param>
        CreepRangedAttackResult RangedAttack(IPowerCreep target);

        /// <summary>
        /// A ranged attack against another creep or structure.
        /// Requires the RANGED_ATTACK body part.
        /// If the target is inside a rampart, the rampart is attacked instead.
        /// The target has to be within 3 squares range of the creep.
        /// </summary>
        /// <param name="target"></param>
        CreepRangedAttackResult RangedAttack(IStructure target);

        /// <summary>
        /// Heal another creep at a distance.
        /// It will restore the target creep’s damaged body parts function and increase the hits counter.
        /// Requires the HEAL body part.
        /// The target has to be within 3 squares range of the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepHealResult RangedHeal(ICreep target);

        /// <summary>
        /// Heal a power creep at a distance.
        /// It will restore the target creep’s damaged body parts function and increase the hits counter.
        /// Requires the HEAL body part.
        /// The target has to be within 3 squares range of the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepHealResult RangedHeal(IPowerCreep target);

        /// <summary>
        /// A ranged attack against all hostile creeps or structures within 3 squares range.
        /// Requires the RANGED_ATTACK body part.
        /// The attack power depends on the range to each target.
        /// Friendly units are not affected.
        /// </summary>
        /// <returns></returns>
        CreepRangedMassAttackResult RangedMassAttack();

        /// <summary>
        /// Repair a damaged structure using carried energy.
        /// Requires the WORK and CARRY body parts.
        /// The target has to be within 3 squares range of the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepRepairResult Repair(IStructure target);

        /// <summary>
        /// Temporarily block a neutral controller from claiming by other players and restore energy sources to their full capacity.
        /// Each tick, this command increases the counter of the period during which the controller is unavailable by 1 tick per each CLAIM body part.
        /// The maximum reservation period to maintain is 5,000 ticks.
        /// The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepReserveControllerResult ReserveController(IStructureController target);

        /// <summary>
        /// Display a visual speech balloon above the creep with the specified message.
        /// The message will be available for one tick.
        /// You can read the last message using the saying property.
        /// Any valid Unicode characters are allowed, including emoji.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sayPublic"></param>
        /// <returns></returns>
        CreepSayResult Say(string message, bool sayPublic = false);

        /// <summary>
        /// Sign a controller with an arbitrary text visible to all players.
        /// This text will appear in the room UI, in the world map, and can be accessed via the API.
        /// You can sign unowned and hostile controllers.
        /// The target has to be at adjacent square to the creep. Pass an empty string to remove the sign.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        CreepSignControllerResult SignController(IStructureController target, string text);

        /// <summary>
        /// Kill the creep immediately.
        /// </summary>
        /// <returns></returns>
        CreepSuicideResult Suicide();

        /// <summary>
        /// Transfer resource from the creep to another object. The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        CreepTransferResult Transfer(ICreep target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Transfer resource from the creep to another object. The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        CreepTransferResult Transfer(IPowerCreep target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Transfer resource from the creep to another object. The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        CreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Transfer resource from the creep to another object. The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        CreepTransferResult Transfer(IScoreCollector target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Upgrade your controller to the next level using carried energy.
        /// Upgrading controllers raises your Global Control Level in parallel.
        /// Requires WORK and CARRY body parts.
        /// The target has to be within 3 squares range of the creep.
        /// 
        /// A fully upgraded level 8 controller can't be upgraded over 15 energy units per tick regardless of creeps abilities.
        /// The cumulative effect of all the creeps performing upgradeController in the current tick is taken into account.
        /// This limit can be increased by using ghodium mineral boost.
        /// 
        /// Upgrading the controller raises its ticksToDowngrade timer by 100.
        /// The timer must be full in order for controller to be levelled up.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        CreepUpgradeControllerResult UpgradeController(IStructureController target);

        /// <summary>
        /// Withdraw resources from a structure.
        /// The target has to be at adjacent square to the creep.
        /// Multiple creeps can withdraw from the same object in the same tick.
        /// Your creeps can withdraw resources from hostile structures/tombstones as well, in case if there is no hostile rampart on top of it.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        CreepWithdrawResult Withdraw(IStructure target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Withdraw resources from a tombstone.
        /// The target has to be at adjacent square to the creep.
        /// Multiple creeps can withdraw from the same object in the same tick.
        /// Your creeps can withdraw resources from hostile structures/tombstones as well, in case if there is no hostile rampart on top of it.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        CreepWithdrawResult Withdraw(ITombstone target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Withdraw resources from a ruin.
        /// The target has to be at adjacent square to the creep.
        /// Multiple creeps can withdraw from the same object in the same tick.
        /// Your creeps can withdraw resources from hostile structures/tombstones as well, in case if there is no hostile rampart on top of it.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        CreepWithdrawResult Withdraw(IRuin target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Withdraw resources from a score container.
        /// The target has to be at adjacent square to the creep.
        /// Multiple creeps can withdraw from the same object in the same tick.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        CreepWithdrawResult Withdraw(IScoreContainer target, ResourceType resourceType, int? amount = null);
    }
}
