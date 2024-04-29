using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum PowerCreepOrderType
    {
        Drop,
        Move,
        Pickup,
        Say,
        Suicide,
        Transfer,
        Upgrade,
        UsePower,
        Withdraw,
    }

    public enum PowerCreepClass
    {
        Operator
    }

    public enum PowerType
    {
        GenerateOps = 1,
        OperateSpawn = 2,
        OperateTower = 3,
        OperateStorage = 4,
        OperateLab = 5,
        OperateExtension = 6,
        OperateObserver = 7,
        OperateTerminal = 8,
        DisruptSpawn = 9,
        DisruptTower = 10,
        DisruptSource = 11,
        Shield = 12,
        RegenSource = 13,
        RegenMineral = 14,
        DisruptTerminal = 15,
        OperatePower = 16,
        Fortify = 17,
        OperateController = 18,
        OperateFactory = 19,
    }

    public enum PowerCreepDeleteResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is spawned in the world.
        /// </summary>
        Busy = -4,
    }

    public enum PowerCreepDropResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is spawned in the world.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The creep does not have the given amount of energy.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The resourceType is not a valid RESOURCE_* constants.
        /// </summary>
        InvalidArgs = -10,
    }

    public enum PowerCreepEnableRoomResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The target is not a controller structure.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
    }

    public enum PowerCreepMoveResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is not spawned in the world.
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
    }

    public enum PowerCreepNotifyWhenAttackedResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is not spawned in the world.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// Enable argument is not a boolean value.
        /// </summary>
        InvalidArgs = -10,
    }

    public enum PowerCreepPickupResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is spawned in the world.
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

    public enum PowerCreepRenameResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// A power creep with the specified name already exists.
        /// </summary>
        NameExists = -3,
        /// <summary>
        /// The power creep is spawned in the world.
        /// </summary>
        Busy = -4,
    }

    public enum PowerCreepRenewResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is not spawned in the world.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The target is not a valid power bank object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
    }

    public enum PowerCreepSayResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is not spawned in the world.
        /// </summary>
        Busy = -4,
    }

    public enum PowerCreepSpawnResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is already spawned in the world.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The specified object is not a Power Spawn.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The power creep cannot be spawned because of the cooldown.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// Room Controller Level insufficient to use the spawn.
        /// </summary>
        RclNotEnough = -14,
    }

    public enum PowerCreepSuicideResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is not spawned in the world.
        /// </summary>
        Busy = -4,
    }

    public enum PowerCreepTransferResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is not spawned in the world.
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
        /// The target cannot receive any more resources.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The resourceType is not one of the RESOURCE_* constants, or the amount is incorrect.
        /// </summary>
        InvalidArgs = -10,
    }

    public enum PowerCreepUpgradeResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// You account Power Level is not enough.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The specified power cannot be upgraded on this creep's level, or the creep reached the maximum level.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The specified power ID is not valid.
        /// </summary>
        InvalidArgs = -10,
    }

    public enum PowerCreepUsePowerResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The creep is not spawned in the world.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The specified target is not valid.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The specified power cannot be upgraded on this creep's level, or the creep reached the maximum level.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The specified target is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The specified power ID is not valid.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// The power ability is still on cooldown.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// The creep doesn't have the specified power ability.
        /// </summary>
        NoBodypart = -12,
    }

    public enum PowerCreepWithdrawResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of the creep, or there is a hostile rampart on top of the target.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The power creep is not spawned in the world.
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
        InvalidArgs = -10,
    }

    /// <param name="Level">Current level of the power.</param>
    /// <param name="Cooldown">Cooldown ticks remaining, or undefined if the power creep is not spawned in the world.</param>
    public readonly record struct PowerState(int Level, int? Cooldown);

    /// <summary>
    /// Power Creeps are immortal "heroes" that are tied to your account and can be respawned in any PowerSpawn after death. You can upgrade their abilities ("powers") up to your account Global Power Level (see Game.gpl).
    /// </summary>
    public interface IPowerCreep : IRoomObject, IWithId, IWithName, IWithStore
    {
        /// <summary>
        /// The power creep's class, one of the POWER_CLASS constants.
        /// </summary>
        PowerCreepClass Class { get; }

        /// <summary>
        /// A timestamp when this creep is marked to be permanently deleted from the account, or undefined otherwise.
        /// </summary>
        DateTime? DeleteTime { get; }

        /// <summary>
        /// The current amount of hit points of the creep.
        /// </summary>
        int Hits { get; }

        /// <summary>
        /// The maximum amount of hit points of the creep.
        /// </summary>
        int HitsMax { get; }

        /// <summary>
        /// The power creep's level.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// A shorthand to Memory.powerCreeps[creep.name]. You can use it for quick access the creep’s specific memory data object.
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
        /// Available powers.
        /// </summary>
        IReadOnlyDictionary<PowerType, PowerState> Powers { get; }

        /// <summary>
        /// The text message that the creep was saying at the last tick.
        /// </summary>
        string? Saying { get; }

        /// <summary>
        /// The name of the shard where the power creep is spawned, or undefined.
        /// </summary>
        string? Shard { get; }

        /// <summary>
        /// The timestamp when spawning or deleting this creep will become available. Undefined if the power creep is spawned in the world
        /// </summary>
        DateTime? SpawnCooldownTime { get; }

        /// <summary>
        /// The remaining amount of game ticks after which the creep will die and become unspawned. Undefined if the creep is not spawned in the world.
        /// </summary>
        int? TicksToLive { get; }

        /// <summary>
        /// Cancel the order given during the current game tick.
        /// </summary>
        /// <param name="orderType"></param>
        /// <returns></returns>
        void CancelOrder(PowerCreepOrderType orderType);

        /// <summary>
        /// Delete the power creep permanently from your account.
        /// It should NOT be spawned in the world.
        /// The creep is not deleted immediately, but a 24-hours delete timer is started instead (see deleteTime).
        /// You can cancel deletion by calling delete(true).
        /// </summary>
        /// <param name="cancel"></param>
        /// <returns></returns>
        PowerCreepDeleteResult Delete(bool? cancel = null);

        /// <summary>
        /// Drop this resource on the ground.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        PowerCreepDropResult Drop(ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Enable powers usage in this room. The room controller should be at adjacent tile.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        PowerCreepEnableRoomResult EnableRoom(IStructureController controller);

        /// <summary>
        /// Move the creep one square in the specified direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        PowerCreepMoveResult Move(Direction direction);

        /// <summary>
        /// Move the creep using the specified predefined path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        PowerCreepMoveResult MoveByPath(IEnumerable<PathStep> path);

        /// <summary>
        /// Move the creep using the specified predefined path.
        /// </summary>
        /// <param name="serialisedPath"></param>
        /// <returns></returns>
        PowerCreepMoveResult MoveByPath(string serialisedPath);

        /// <summary>
        /// Find the optimal path to the target within the same room and move to it.
        /// A shorthand to consequent calls of pos.findPathTo() and move() methods.
        /// If the target is in another room, then the corresponding exit will be used as a target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        PowerCreepMoveResult MoveTo(Position target, MoveToOptions? opts = null);

        /// <summary>
        /// Find the optimal path to the target within the same room and move to it.
        /// A shorthand to consequent calls of pos.findPathTo() and move() methods.
        /// If the target is in another room, then the corresponding exit will be used as a target.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        PowerCreepMoveResult MoveTo(RoomPosition target, MoveToOptions? opts = null);

        /// <summary>
        /// Toggle auto notification when the creep is under attack.
        /// The notification will be sent to your account email.
        /// Turned on by default.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        PowerCreepNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled);

        /// <summary>
        /// Pick up an item (a dropped piece of energy).
        /// The target has to be at adjacent square to the creep or at the same square.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        PowerCreepPickupResult Pickup(IResource target);

        /// <summary>
        /// Rename the power creep.
        /// It must not be spawned in the world.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        PowerCreepRenameResult Rename(string name);

        /// <summary>
        /// Instantly restore time to live to the maximum using a Power Bank nearby.
        /// It has to be at adjacent tile.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        PowerCreepRenewResult Renew(IStructurePowerBank target);

        /// <summary>
        /// Instantly restore time to live to the maximum using a Power Spawn nearby.
        /// It has to be at adjacent tile.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        PowerCreepRenewResult Renew(IStructurePowerSpawn target);

        /// <summary>
        /// Display a visual speech balloon above the creep with the specified message.
        /// The message will be available for one tick.
        /// You can read the last message using the saying property.
        /// Any valid Unicode characters are allowed, including emoji.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="public"></param>
        /// <returns></returns>
        PowerCreepSayResult Say(string message, bool @public = false);

        /// <summary>
        /// Spawn this power creep in the specified Power Spawn.
        /// </summary>
        /// <param name="powerSpawn"></param>
        /// <returns></returns>
        PowerCreepSpawnResult Spawn(IStructurePowerSpawn powerSpawn);

        /// <summary>
        /// Kill the power creep immediately.
        /// It will not be destroyed permanently, but will become unspawned, so that you can spawn it again.
        /// </summary>
        /// <returns></returns>
        PowerCreepSuicideResult Suicide();

        /// <summary>
        /// Transfer resource from the creep to another object.
        /// The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used.</param>
        /// <returns></returns>
        PowerCreepTransferResult Transfer(ICreep target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Transfer resource from the creep to another object.
        /// The target has to be at adjacent square to the creep.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used.</param>
        /// <returns></returns>
        PowerCreepTransferResult Transfer(IStructure target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Upgrade the creep, adding a new power ability to it or increasing level of the existing power.
        /// You need one free Power Level in your account to perform this action.
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        PowerCreepUpgradeResult Upgrade(PowerType power);

        /// <summary>
        /// Apply one the creep's powers on the specified target.
        /// You can only use powers in rooms either without a controller, or with a power-enabled controller.
        /// Only one power can be used during the same tick, each usePower call will override the previous one.
        /// If the target has the same effect of a lower or equal level, it is overridden.
        /// If the existing effect level is higher, an error is returned.
        /// </summary>
        /// <param name="power"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        PowerCreepUsePowerResult UsePower(PowerType power, IRoomObject? target = null);

        /// <summary>
        /// Withdraw resources from a structure.
        /// The target has to be at adjacent square to the creep.
        /// Multiple creeps can withdraw from the same object in the same tick.
        /// Your creeps can withdraw resources from hostile structures/tombstones as well, in case if there is no hostile rampart on top of it.
        /// This method should not be used to transfer resources between creeps.To transfer between creeps, use the transfer method on the original creep.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used.</param>
        /// <returns></returns>
        PowerCreepWithdrawResult Withdraw(IStructure target, ResourceType resourceType, int? amount = null);

        /// <summary>
        /// Withdraw resources from a tombstone.
        /// The target has to be at adjacent square to the creep.
        /// Multiple creeps can withdraw from the same object in the same tick.
        /// Your creeps can withdraw resources from hostile structures/tombstones as well, in case if there is no hostile rampart on top of it.
        /// This method should not be used to transfer resources between creeps.To transfer between creeps, use the transfer method on the original creep.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="resourceType"></param>
        /// <param name="amount">The amount of resources to be transferred. If omitted, all the available carried amount is used.</param>
        /// <returns></returns>
        PowerCreepWithdrawResult Withdraw(ITombstone target, ResourceType resourceType, int? amount = null);
    }
}
