using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.Arena
{
    public enum SpawnCreepError
    {
        NotOwner = -1,
        InvalidArgs = -10,
        NotEnoughEnergy = -6,
        Busy = -4
    }

    public enum CancelSpawnCreepResult
    {
        Ok = 0,
        NotOwner = -1
    }
    public enum SetDirectionsSpawningResult
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
    public readonly struct SpawnCreepResult : IEquatable<SpawnCreepResult>
    {
        public readonly ICreep? Object;
        public readonly SpawnCreepError? Error;

        public SpawnCreepResult(ICreep? @object, SpawnCreepError? error)
        {
            Object = @object;
            Error = error;
        }

        public override bool Equals(object? obj) => obj is SpawnCreepResult result && Equals(result);

        public bool Equals(SpawnCreepResult other)
            => EqualityComparer<ICreep?>.Default.Equals(Object, other.Object)
            && Error == other.Error;

        public override int GetHashCode() => HashCode.Combine(Object, Error);

        public static bool operator ==(SpawnCreepResult left, SpawnCreepResult right) => left.Equals(right);

        public static bool operator !=(SpawnCreepResult left, SpawnCreepResult right) => !(left == right);
    }

    /// <summary>
    /// Details of the creep being spawned currently
    /// </summary>
    public interface ISpawning
    {
        /// <summary>
        /// Time needed in total to complete the spawning
        /// </summary>
        int NeedTime { get; }

        /// <summary>
        /// Remaining time to go
        /// </summary>
        int RemainingTime { get; }

        /// <summary>
        /// The creep that being spawned
        /// </summary>
        ICreep Creep { get; }

        /// <summary>
        /// Cancel spawning immediately
        /// </summary>
        /// <returns></returns>
        CancelSpawnCreepResult Cancel();
    }

    /// <summary>
    /// This structure can create creeps. It also auto-regenerate a little amount of energy each tick
    /// </summary>
    public interface IStructureSpawn : IOwnedStructure
    {
        /// <summary>
        /// A Store object that contains cargo of this structure.
        /// </summary>
        IStore Store { get; }

        /// <summary>
        /// If the spawn is in process of spawning a new creep, this object will contain a Spawning object, or null otherwise
        /// </summary>
        ISpawning? Spawning { get; }

        /// <summary>
        /// Start the creep spawning process
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        SpawnCreepResult SpawnCreep(IEnumerable<BodyPartType> body);

        /// <summary>
        /// Start the creep spawning process
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        SpawnCreepResult SpawnCreep(BodyType<BodyPartType> bodyType);

        /// <summary>
        /// Set desired directions where the creep should move when spawned.
        /// </summary>
        /// <param name="directions"></param>
        /// <returns></returns>
        SetDirectionsSpawningResult SetDirections(IEnumerable<Direction> directions);
    }
}
