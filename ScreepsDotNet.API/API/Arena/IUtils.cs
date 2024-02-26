using System;
using System.Collections.Generic;

namespace ScreepsDotNet.API.Arena
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

    public enum Terrain
    {
        Wall = 1,
        Swamp = 2,
        Plain = 0
    }

    /// <summary>
    /// Represents a rectangular view of the room's terrain.
    /// </summary>
    public readonly struct TerrainSlice
    {
        private readonly Terrain[] data;

        /// <summary>
        /// The top-left coordinate of the slice.
        /// </summary>
        public readonly Position Min;

        /// <summary>
        /// The bottom-right coordinate of the slice.
        /// </summary>
        public readonly Position Max;

        /// <summary>
        /// Gets the size of the slice.
        /// </summary>
        public (int w, int h) Size => (Max.X - Min.X + 1, Max.Y - Min.Y + 1);

        /// <summary>
        /// Gets the terrain at the specified position.
        /// </summary>
        /// <param name="position">Position in world-space (e.g. not relative to the Min)</param>
        /// <returns></returns>
        public Terrain this[Position position] => data[(position.Y - Min.Y) * (Max.X - Min.X + 1) + position.X - Min.X];

        public TerrainSlice(ReadOnlySpan<Terrain> data, Position min, Position max)
        {
            this.data = data.ToArray();
            Min = min;
            Max = max;
        }
    }

    public interface IUtils
    {
        /// <summary>
        /// Create new ConstructionSite at the specified location.
        /// </summary>
        CreateConstructionSiteResult CreateConstructionSite<T>(Position position) where T : class, IStructure;

        /// <summary>
        /// Creates a new empty instance of Visual.
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="persistent"></param>
        /// <returns></returns>
        IVisual CreateVisual(int layer = 0, bool persistent = false);

        /// <summary>
        /// Find a position with the shortest path from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromPos"></param>
        /// <param name="positions"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        T? FindClosestByPath<T>(Position fromPos, IEnumerable<T> positions, FindPathOptions? options) where T : class, IPosition;

        /// <summary>
        /// Find a position with the shortest path from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromPos"></param>
        /// <param name="positions"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Position? FindClosestByPath(Position fromPos, IEnumerable<Position> positions, FindPathOptions? options);

        /// <summary>
        /// Find a position with the shortest linear distance from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromPos"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        T? FindClosestByRange<T>(Position fromPos, IEnumerable<T> positions) where T : class, IPosition;

        /// <summary>
        /// Find a position with the shortest linear distance from this game object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromPos"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        Position? FindClosestByRange(Position fromPos, IEnumerable<Position> positions);

        /// <summary>
        /// Find all objects in the specified linear range
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromPos"></param>
        /// <param name="positions"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        IEnumerable<T> FindInRange<T>(Position fromPos, IEnumerable<T> positions, int range) where T : class, IPosition;

        /// <summary>
        /// Find all objects in the specified linear range
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="positions"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        IEnumerable<Position> FindInRange(Position fromPos, IEnumerable<Position> positions, int range);

        /// <summary>
        /// Find an optimal path between fromPos and toPos.
        /// Unlike searchPath avoid all obstacles by default (unless costMatrix is specified).
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        IEnumerable<Position> FindPath(Position fromPos, Position toPos, FindPathOptions? options);

        /// <summary>
        /// Find an optimal path between fromPos and toPos.
        /// Unlike searchPath avoid all obstacles by default (unless costMatrix is specified).
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        IEnumerable<Position> FindPath(Position fromPos, IPosition toPos, FindPathOptions? options);

        /// <summary>
        /// Get CPU wall time elapsed in the current tick in nanoseconds
        /// </summary>
        /// <returns></returns>
        double GetCpuTime();

        /// <summary>
        /// Get linear direction by differences of x and y.
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        Direction GetDirection(int dx, int dy);

        /// <summary>
        /// Use this method to get heap statistics for your virtual machine.
        /// </summary>
        /// <returns></returns>
        HeapInfo GetHeapStatistics();

        /// <summary>
        /// Get an object with the specified unique ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IGameObject? GetObjectById(string id);

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

        /// <summary>
        /// Get the terrain at the given position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        Terrain GetTerrainAt(Position pos);

        /// <summary>
        /// Gets a slice containing all terrain between two positions.
        /// More efficient than several calls to GetTerrainAt for larger slices.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        TerrainSlice GetTerrain(Position min, Position max);

        /// <summary>
        /// Gets the number of ticks passed from the start of the current game.
        /// </summary>
        /// <returns></returns>
        int GetTicks();


    }
}
