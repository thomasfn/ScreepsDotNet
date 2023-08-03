using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum SpawningCancelResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this spawn.
        /// </summary>
        NotOwner = -1,
    }

    public enum SpawningSetDirectionsResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this spawn.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The directions array is invalid.
        /// </summary>
        InvalidArgs = -10
    }

    public enum SpawnCreepResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this spawn.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// There is a creep with the same name already.
        /// </summary>
        NameExists = -3,
        /// <summary>
        /// The spawn is already in process of spawning another creep.
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The spawn and its extensions contain not enough energy to create a creep with the given body.
        /// </summary>
        NotEnoughEnergy = -6,
        /// <summary>
        /// The directions array is invalid.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// Your Room Controller level is insufficient to use this spawn.
        /// </summary>
        RclNotEnough = -14
    }

    public enum RecycleCreepResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this spawn.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The specified target object is not a creep.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target creep is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// Your Room Controller level is insufficient to use this spawn.
        /// </summary>
        RclNotEnough = -14
    }

    public enum RenewCreepResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this spawn, or the creep.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The spawn is spawning another creep
        /// </summary>
        Busy = -4,
        /// <summary>
        /// The spawn does not have enough energy.
        /// </summary>
        NotEnoughEnergy = -6,
        /// <summary>
        /// The specified target object is not a creep.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target creep's time to live timer is full.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The target creep is too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// Your Room Controller level is insufficient to use this spawn.
        /// </summary>
        RclNotEnough = -14
    }

    /// <summary>
    /// An object with additional options for the spawning process.
    /// </summary>
    public readonly struct SpawnCreepOptions
    {
        /// <summary>
        /// Memory of the new creep. If provided, it will be immediately stored into Memory.creeps[name].
        /// </summary>
        public readonly object? Memory;

        /// <summary>
        /// Array of spawns/extensions from which to draw energy for the spawning process. Structures will be used according to the array order.
        /// </summary>
        public readonly IEnumerable<IOwnedStructure>? EnergyStructures;

        /// <summary>
        /// If dryRun is true, the operation will only check if it is possible to create a creep.
        /// </summary>
        public readonly bool? DryRun;

        /// <summary>
        /// Set desired directions where the creep should move when spawned.
        /// </summary>
        public readonly IEnumerable<Direction>? Directions;

        public SpawnCreepOptions(
            object? memory = null,
            IEnumerable<IOwnedStructure>? energyStructures = null,
            bool? dryRun = null,
            IEnumerable<Direction>? directions = null
        )
        {
            Memory = memory;
            EnergyStructures = energyStructures;
            DryRun = dryRun;
            Directions = directions;
        }
    }

    /// <summary>
    /// Details of the creep being spawned currently that can be addressed by the StructureSpawn.spawning property.
    /// </summary>
    public interface ISpawning
    {
        /// <summary>
        /// An array with the spawn directions
        /// </summary>
        public IEnumerable<Direction> Directions { get; }

        /// <summary>
        /// The name of a new creep.
        /// </summary>
        public string Name {  get; }

        /// <summary>
        /// Time needed in total to complete the spawning.
        /// </summary>
        public int NeedTime {  get; }

        /// <summary>
        /// Remaining time to go.
        /// </summary>
        public int RemainingTime { get; }

        /// <summary>
        /// A link to the spawn.
        /// </summary>
        public IStructureSpawn Spawn { get; }

        /// <summary>
        /// Cancel spawning immediately. Energy spent on spawning is not returned.
        /// </summary>
        SpawningCancelResult Cancel();

        /// <summary>
        /// Set desired directions where the creep should move when spawned.
        /// </summary>
        /// <param name="directions"></param>
        /// <returns></returns>
        SpawningSetDirectionsResult SetDirections(IEnumerable<Direction> directions);
    }

    /// <summary>
    /// Spawn is your colony center. This structure can create, renew, and recycle creeps.
    /// All your spawns are accessible through Game.spawns hash list.
    /// Spawns auto-regenerate a little amount of energy each tick, so that you can easily recover even if all your creeps died.
    /// </summary>
    public interface IStructureSpawn : IOwnedStructure
    {
        /// <summary>
        /// A shorthand to Memory.spawns[spawn.name]. You can use it for quick access the spawn’s specific memory data object.
        /// </summary>
        object Memory { get; }

        /// <summary>
        /// Spawn’s name. You choose the name upon creating a new spawn, and it cannot be changed later. This name is a hash key to access the spawn via the Game.spawns object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// If the spawn is in process of spawning a new creep, this object will contain a StructureSpawn.Spawning object, or null otherwise.
        /// </summary>
        ISpawning? Spawning { get; }

        /// <summary>
        /// A Store object that contains cargo of this structure.
        /// </summary>
        IStore Store { get; }

        /// <summary>
        /// Start the creep spawning process. The required energy amount can be withdrawn from all spawns and extensions in the room.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="name"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        SpawnCreepResult SpawnCreep(IEnumerable<BodyPartType> body, string name, SpawnCreepOptions? opts = null);

        /// <summary>
        /// Kill the creep and drop up to 100% of resources spent on its spawning and boosting depending on remaining life time.
        /// The target should be at adjacent square.
        /// Energy return is limited to 125 units per body part.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        RecycleCreepResult RecycleCreep(ICreep target);

        /// <summary>
        /// Increase the remaining time to live of the target creep.
        /// The target should be at adjacent square.
        /// The target should not have CLAIM body parts.
        /// The spawn should not be busy with the spawning process.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        RenewCreepResult RenewCreep(ICreep target);
    }
}
