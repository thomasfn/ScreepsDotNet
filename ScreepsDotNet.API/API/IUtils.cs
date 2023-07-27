using System;
using System.Collections.Generic;
using System.Text;

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

    public enum Terrain
    {
        Wall = 1,
        Swamp = 2,
        Plain = 0
    }

    public readonly struct HeapInfo
    {
        public readonly int TotalHeapSize;
        public readonly int TotalHeapSizeExecutable;
        public readonly int TotalPhysicalSize;
        public readonly int TotalAvailableSize;
        public readonly int UsedHeapSize;
        public readonly int HeapSizeLimit;
        public readonly int MallocedMemory;
        public readonly int PeakMallocedMemory;
        public readonly int DoesZapGarbage;
        public readonly int NumberOfNativeContexts;
        public readonly int NumberOfDetachedContexts;
        public readonly int ExternallyAllocatedSize;

        public HeapInfo(int totalHeapSize, int totalHeapSizeExecutable, int totalPhysicalSize, int totalAvailableSize, int usedHeapSize, int heapSizeLimit, int mallocedMemory, int peakMallocedMemory, int doesZapGarbage, int numberOfNativeContexts, int numberOfDetachedContexts, int externallyAllocatedSize)
        {
            TotalHeapSize = totalHeapSize;
            TotalHeapSizeExecutable = totalHeapSizeExecutable;
            TotalPhysicalSize = totalPhysicalSize;
            TotalAvailableSize = totalAvailableSize;
            UsedHeapSize = usedHeapSize;
            HeapSizeLimit = heapSizeLimit;
            MallocedMemory = mallocedMemory;
            PeakMallocedMemory = peakMallocedMemory;
            DoesZapGarbage = doesZapGarbage;
            NumberOfNativeContexts = numberOfNativeContexts;
            NumberOfDetachedContexts = numberOfDetachedContexts;
            ExternallyAllocatedSize = externallyAllocatedSize;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"TotalHeapSize: {FormatBytes(TotalHeapSize)}");
            sb.AppendLine($"TotalHeapSizeExecutable: {FormatBytes(TotalHeapSizeExecutable)}");
            sb.AppendLine($"TotalPhysicalSize: {FormatBytes(TotalPhysicalSize)}");
            sb.AppendLine($"TotalAvailableSize: {FormatBytes(TotalAvailableSize)}");
            sb.AppendLine($"UsedHeapSize: {FormatBytes(UsedHeapSize)}");
            sb.AppendLine($"HeapSizeLimit: {FormatBytes(HeapSizeLimit)}");
            sb.AppendLine($"MallocedMemory: {FormatBytes(MallocedMemory)}");
            sb.AppendLine($"PeakMallocedMemory: {FormatBytes(PeakMallocedMemory)}");
            sb.AppendLine($"DoesZapGarbage: {DoesZapGarbage}");
            sb.AppendLine($"NumberOfNativeContexts: {NumberOfNativeContexts}");
            sb.AppendLine($"NumberOfDetachedContexts: {NumberOfDetachedContexts}");
            sb.AppendLine($"ExternallyAllocatedSize: {FormatBytes(ExternallyAllocatedSize)}");
            return sb.ToString();
        }

        private static string FormatBytes(int bytes)
            => $"{bytes / 1024} KiB";
    }

    public interface IUtils
    {
        /// <summary>
        /// Create new ConstructionSite at the specified location.
        /// </summary>
        CreateConstructionSiteResult CreateConstructionSite<T>(Position position) where T : class, IStructure;

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
        long GetCpuTime();

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
        /// Gets the number of ticks passed from the start of the current game.
        /// </summary>
        /// <returns></returns>
        int GetTicks();

        
    }
}
