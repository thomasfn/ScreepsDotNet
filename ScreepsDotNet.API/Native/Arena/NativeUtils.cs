using System;
using System.Collections.Generic;
using System.Linq;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class PositionExtensions
    {
        public static Position ToPosition(this JSObject obj)
            => (obj.GetPropertyAsInt32(Names.X), obj.GetPropertyAsInt32(Names.Y));

        public static Position? ToPositionNullable(this JSObject? obj)
            => obj != null ? new Position?(obj.ToPosition()) : null;

        public static JSObject ToJS(this Position pos)
        {
            var obj = JSObject.Create();
            obj.SetProperty(Names.X, pos.X);
            obj.SetProperty(Names.Y, pos.Y);
            return obj;
        }

        public static JSObject ToJS(this FractionalPosition pos)
        {
            var obj = JSObject.Create();
            obj.SetProperty(Names.X, pos.X);
            obj.SetProperty(Names.Y, pos.Y);
            return obj;
        }

        public static JSObject? ToJS(this Position? pos)
            => pos != null ? pos.Value.ToJS() : null;

        public static JSObject? ToJS(this FractionalPosition? pos)
            => pos != null ? pos.Value.ToJS() : null;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeUtils : IUtils
    {
        #region Imports

        [JSImport("createConstructionSite", "game/utils")]
        internal static partial JSObject Native_CreateConstructionSite(JSObject position, JSObject prototype);

        [JSImport("createVisual", "game/visual")]
        internal static partial JSObject Native_CreateVisual(int layer, bool persistent);

        [JSImport("findClosestByPath", "game/utils")]
        internal static partial JSObject Native_FindClosestByPath(JSObject fromPos, JSObject[] positions, JSObject? options);

        [JSImport("findClosestByRange", "game/utils")]
        internal static partial JSObject Native_FindClosestByRange(JSObject fromPos, JSObject[] positions);

        [JSImport("findInRange", "game/utils")]
        internal static partial JSObject[] Native_FindInRange(JSObject fromPos, JSObject[] positions, int range);

        [JSImport("findPath", "game/utils")]
        internal static partial JSObject[] Native_FindPath(JSObject fromPos, JSObject toPos, JSObject? options);

        [JSImport("getCpuTime", "game/utils")]
        internal static partial long Native_GetCpuTime();

        [JSImport("getDirection", "game/utils")]
        internal static partial int Native_GetDirection(int dx, int dy);

        [JSImport("getHeapStatistics", "game/utils")]
        internal static partial JSObject Native_GetHeapStatistics();

        [JSImport("getObjectById", "game/utils")]
        internal static partial JSObject Native_GetObjectById(string id);

        [JSImport("getObjects", "game/utils")]
        internal static partial JSObject[] Native_GetObjects();

        [JSImport("getObjectsByPrototype", "game/utils")]
        internal static partial JSObject[] Native_GetObjectsByPrototype(JSObject prototype);

        [JSImport("getRange", "game/utils")]
        internal static partial int Native_GetRange(JSObject a, JSObject b);

        [JSImport("getTerrainAt", "game/utils")]
        internal static partial int Native_GetTerrainAt(JSObject pos);

        [JSImport("getTerrain", "game/utils")]
        internal static partial void Native_GetTerrain(int minX, int minY, int maxX, int maxY, [JSMarshalAsDataView] Span<byte> outTerrainData);

        [JSImport("getTicks", "game/utils")]
        internal static partial int Native_GetTicks();

        #endregion

        private readonly INativeRoot nativeRoot;

        public NativeUtils(INativeRoot nativeRoot)
        {
            this.nativeRoot = nativeRoot;
        }

        public CreateConstructionSiteResult CreateConstructionSite<T>(Position position) where T : class, IStructure
        {
            var resultObj = Native_CreateConstructionSite(position.ToJS(), NativeGameObjectPrototypes<T>.ConstructorObj!);
            if (resultObj == null) { throw new InvalidOperationException($"Native_CreateConstructionSite returned null or undefined"); }
            var constructionSiteObj = resultObj.GetPropertyAsJSObject(Names.Object);
            var constructionSite = constructionSiteObj.ToGameObject<IConstructionSite>(nativeRoot);
            var error = (CreateConstructionSiteError?)resultObj.TryGetPropertyAsInt32(Names.Error);
            return new CreateConstructionSiteResult(constructionSite, error);
        }

        public IVisual CreateVisual(int layer = 0, bool persistent = false)
            => new NativeVisual(Native_CreateVisual(layer, persistent));

        public T? FindClosestByPath<T>(Position fromPos, IEnumerable<T> positions, FindPathOptions? options) where T : class, IPosition
            => Native_FindClosestByPath(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray(), options.ToJS()).ToGameObject<IGameObject>(nativeRoot) as T;

        public Position? FindClosestByPath(Position fromPos, IEnumerable<Position> positions, FindPathOptions? options)
            => Native_FindClosestByPath(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray(), options.ToJS()).ToPositionNullable();

        public T? FindClosestByRange<T>(Position fromPos, IEnumerable<T> positions) where T : class, IPosition
            => Native_FindClosestByRange(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray()).ToGameObject<IGameObject>(nativeRoot) as T;

        public Position? FindClosestByRange(Position fromPos, IEnumerable<Position> positions)
            => Native_FindClosestByRange(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray()).ToPositionNullable();

        public IEnumerable<T> FindInRange<T>(Position fromPos, IEnumerable<T> positions, int range) where T : class, IPosition
            => Native_FindInRange(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray(), range)
                .Select(x => x.ToGameObject<IGameObject>(nativeRoot))
                .Where(x => x is T)
                .Cast<T>()
                .ToArray();

        public IEnumerable<Position> FindInRange(Position fromPos, IEnumerable<Position> positions, int range)
            => Native_FindInRange(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray(), range)
                .Select(x => x.ToPosition())
                .ToArray();

        public IEnumerable<Position> FindPath(Position fromPos, Position toPos, FindPathOptions? options)
            => Native_FindPath(fromPos.ToJS(), toPos.ToJS(), options.ToJS())
                .Select(x => x.ToPosition())
                .ToArray();

        public IEnumerable<Position> FindPath(Position fromPos, IPosition toPos, FindPathOptions? options)
            => Native_FindPath(fromPos.ToJS(), toPos.ToJS(), options.ToJS())
                .Select(x => x.ToPosition())
                .ToArray();

        public long GetCpuTime()
            => Native_GetCpuTime();

        public Direction GetDirection(int dx, int dy)
            => (Direction)Native_GetDirection(dx, dy);

        public HeapInfo GetHeapStatistics()
        {
            var obj = Native_GetHeapStatistics();
            return new HeapInfo(
                obj.GetPropertyAsInt32("total_heap_size"),
                obj.GetPropertyAsInt32("total_heap_size_executable"),
                obj.GetPropertyAsInt32("total_physical_size"),
                obj.GetPropertyAsInt32("total_available_size"),
                obj.GetPropertyAsInt32("used_heap_size"),
                obj.GetPropertyAsInt32("heap_size_limit"),
                obj.GetPropertyAsInt32("malloced_memory"),
                obj.GetPropertyAsInt32("peak_malloced_memory"),
                obj.GetPropertyAsInt32("does_zap_garbage"),
                obj.GetPropertyAsInt32("externally_allocated_size")
            );
        }

        public IGameObject? GetObjectById(string id)
            => Native_GetObjectById(id).ToGameObject<IGameObject>(nativeRoot);

        public IEnumerable<IGameObject> GetObjects()
            => Native_GetObjects()
                .Select(nativeRoot.GetOrCreateWrapperForObject);

        public IEnumerable<T> GetObjectsByType<T>() where T : class, IGameObject
            => Native_GetObjectsByPrototype(NativeGameObjectPrototypes<T>.ConstructorObj!)
                .Select(nativeRoot.GetOrCreateWrapperForObject)
                .Cast<T>();

        public int GetRange(IPosition a, IPosition b)
            => Native_GetRange(a.ToJS(), b.ToJS());

        public Terrain GetTerrainAt(Position pos)
            => (Terrain)Native_GetTerrainAt(pos.ToJS());

        public TerrainSlice GetTerrain(Position min, Position max)
        {
            if (min.X < 0 || min.X >= 100 || min.X > max.X) { throw new ArgumentOutOfRangeException(nameof(min)); }
            if (min.Y < 0 || min.Y >= 100 || min.Y > max.Y) { throw new ArgumentOutOfRangeException(nameof(min)); }
            if (max.X < 0 || max.X >= 100) { throw new ArgumentOutOfRangeException(nameof(max)); }
            if (max.Y < 0 || max.Y >= 100) { throw new ArgumentOutOfRangeException(nameof(max)); }
            int w = (max.X - min.X) + 1;
            int h = (max.Y - min.Y) + 1;
            Span<byte> data = stackalloc byte[w * h];
            Native_GetTerrain(min.X, min.Y, max.X, max.Y, data);
            Span<Terrain> terrainData = stackalloc Terrain[w * h];
            for (int i = 0; i < data.Length; ++i)
            {
                terrainData[i] = (Terrain)data[i];
            }
            return new(terrainData, min, max);
        }

        public int GetTicks()
            => Native_GetTicks();
    }
}
