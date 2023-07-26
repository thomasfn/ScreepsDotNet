using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API
{
    public enum CreateConstructionSiteError
    {
        InvalidArgs = -10,
        InvalidTarget = -7,
        Full = -8
    }

    public readonly struct CreateConstructionSiteResult : IEquatable<CreateConstructionSiteResult>
    {
        public readonly IConstructionSite? Object;
        public readonly CreateConstructionSiteError? Error;

        public CreateConstructionSiteResult(IConstructionSite? @object, CreateConstructionSiteError? error)
        {
            Object = @object;
            Error = error;
        }

        public override bool Equals(object? obj) => obj is CreateConstructionSiteResult result && Equals(result);

        public bool Equals(CreateConstructionSiteResult other)
            => EqualityComparer<IConstructionSite?>.Default.Equals(Object, other.Object)
            && Error == other.Error;

        public override int GetHashCode() => HashCode.Combine(Object, Error);

        public static bool operator ==(CreateConstructionSiteResult left, CreateConstructionSiteResult right) => left.Equals(right);

        public static bool operator !=(CreateConstructionSiteResult left, CreateConstructionSiteResult right) => !(left == right);
    }

    public interface IUtils
    {
        /// <summary>
        /// Create new ConstructionSite at the specified location.
        /// </summary>
        CreateConstructionSiteResult CreateConstructionSite<T>(Position position) where T : class, IStructure;

        // findClosestByPath

        // findClosestByRange

        // findInRange

        // findPath

        /// <returns>CPU wall time elapsed in the current tick in nanoseconds</returns>
        long GetCpuTime();

        // getDirection

        // getHeapStatistics

        // getObjectById

        /// <summary>
        /// Get all game objects in the game
        /// </summary>
        /// <returns></returns>
        IEnumerable<IGameObject> GetObjects();

        /// <summary>
        /// Get all objects in the game with the specified prototype, for example, all creeps
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Array of objects with the specified prototype</returns>
        IEnumerable<T> GetObjectsByType<T>() where T : class, IGameObject;

        /// <summary>
        /// Get linear range between two objects. a and b may be GameObjects or any object containing x and y properties
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>a number of squares between two objects</returns>
        int GetRange(IPosition a, IPosition b);

        // getTerrainAt

        /// <returns>the number of ticks passed from the start of the current game.</returns>
        int GetTicks();

        
    }
}
