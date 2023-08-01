using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Linq;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class PositionExtensions
    {
        public static Position ToPosition(this JSObject obj)
            => (obj.GetPropertyAsInt32("x"), obj.GetPropertyAsInt32("y"));

        public static Position? ToPositionNullable(this JSObject? obj)
            => obj != null ? new Position?(obj.ToPosition()) : null;

        public static JSObject ToJS(this Position pos)
        {
            var obj = NativeGameObjectUtils.CreateObject(null);
            obj.SetProperty("x", pos.X);
            obj.SetProperty("y", pos.Y);
            return obj;
        }

        public static JSObject ToJS(this FractionalPosition pos)
        {
            var obj = NativeGameObjectUtils.CreateObject(null);
            obj.SetProperty("x", pos.X);
            obj.SetProperty("y", pos.Y);
            return obj;
        }

        public static JSObject? ToJS(this Position? pos)
            => pos != null ? pos.Value.ToJS() : null;

        public static JSObject? ToJS(this FractionalPosition? pos)
            => pos != null ? pos.Value.ToJS() : null;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeUtils : IUtils
    {
        #region Imports

        [JSImport("createConstructionSite", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_CreateConstructionSite([JSMarshalAs<JSType.Object>] JSObject position, [JSMarshalAs<JSType.Object>] JSObject prototype);

        [JSImport("createVisual", "game/visual")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_CreateVisual([JSMarshalAs<JSType.Number>] int layer, [JSMarshalAs<JSType.Boolean>] bool persistent);

        [JSImport("findClosestByPath", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_FindClosestByPath([JSMarshalAs<JSType.Object>] JSObject fromPos, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions, [JSMarshalAs<JSType.Object>] JSObject? options);

        [JSImport("findClosestByRange", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_FindClosestByRange([JSMarshalAs<JSType.Object>] JSObject fromPos, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions);

        [JSImport("findInRange", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_FindInRange([JSMarshalAs<JSType.Object>] JSObject fromPos, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] positions, [JSMarshalAs<JSType.Number>] int range);

        [JSImport("findPath", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_FindPath([JSMarshalAs<JSType.Object>] JSObject fromPos, [JSMarshalAs<JSType.Object>] JSObject toPos, [JSMarshalAs<JSType.Object>] JSObject? options);

        [JSImport("getCpuTime", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial long Native_GetCpuTime();

        [JSImport("getDirection", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetDirection([JSMarshalAs<JSType.Number>] int dx, [JSMarshalAs<JSType.Number>] int dy);

        [JSImport("getHeapStatistics", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetHeapStatistics();

        [JSImport("getObjectById", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetObjectById(string id);

        [JSImport("getObjects", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_GetObjects();

        [JSImport("getObjectsByPrototype", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_GetObjectsByPrototype([JSMarshalAs<JSType.Object>] JSObject prototype);

        [JSImport("getRange", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetRange([JSMarshalAs<JSType.Object>] JSObject a, [JSMarshalAs<JSType.Object>] JSObject b);

        [JSImport("getTerrainAt", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetTerrainAt([JSMarshalAs<JSType.Object>] JSObject pos);

        [JSImport("getTerrain", "game/utils")]
        internal static partial void Native_GetTerrain([JSMarshalAs<JSType.Number>] int minX, [JSMarshalAs<JSType.Number>] int minY, [JSMarshalAs<JSType.Number>] int maxX, [JSMarshalAs<JSType.Number>] int maxY, [JSMarshalAs<JSType.MemoryView>] Span<byte> outTerrainData);

        [JSImport("getTicks", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetTicks();

        #endregion

        public CreateConstructionSiteResult CreateConstructionSite<T>(Position position) where T : class, IStructure
        {
            var resultObj = Native_CreateConstructionSite(position.ToJS(), NativeGameObjectPrototypes<T>.ConstructorObj!);
            if (resultObj == null) { throw new InvalidOperationException($"Native_CreateConstructionSite returned null or undefined"); }
            var constructionSiteObj = resultObj.GetPropertyAsJSObject("object");
            var constructionSite = constructionSiteObj.ToGameObject<IConstructionSite>();
            CreateConstructionSiteError? error = resultObj.GetTypeOfProperty("error") == "number" ? (CreateConstructionSiteError)resultObj.GetPropertyAsInt32("error") : null;
            return new CreateConstructionSiteResult(constructionSite, error);
        }

        public IVisual CreateVisual(int layer = 0, bool persistent = false)
            => new NativeVisual(Native_CreateVisual(layer, persistent));

        public T? FindClosestByPath<T>(Position fromPos, IEnumerable<T> positions, FindPathOptions? options) where T : class, IPosition
            => Native_FindClosestByPath(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray(), options.ToJS()).ToGameObject<IGameObject>() as T;

        public Position? FindClosestByPath(Position fromPos, IEnumerable<Position> positions, FindPathOptions? options)
            => Native_FindClosestByPath(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray(), options.ToJS()).ToPositionNullable();

        public T? FindClosestByRange<T>(Position fromPos, IEnumerable<T> positions) where T : class, IPosition
            => Native_FindClosestByRange(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray()).ToGameObject<IGameObject>() as T;

        public Position? FindClosestByRange(Position fromPos, IEnumerable<Position> positions)
            => Native_FindClosestByRange(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray()).ToPositionNullable();

        public IEnumerable<T> FindInRange<T>(Position fromPos, IEnumerable<T> positions, int range) where T : class, IPosition
            => Native_FindInRange(fromPos.ToJS(), positions.Select(x => x.ToJS()).ToArray(), range)
                .Select(x => x.ToGameObject<IGameObject>())
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
                obj.GetPropertyAsInt32("number_of_native_contexts"),
                obj.GetPropertyAsInt32("number_of_detached_contexts"),
                obj.GetPropertyAsInt32("externally_allocated_size")
            );
        }

        public IGameObject? GetObjectById(string id)
            => Native_GetObjectById(id).ToGameObject<IGameObject>();

        public IEnumerable<IGameObject> GetObjects()
            => Native_GetObjects()
                .Select(NativeGameObjectUtils.CreateWrapperForObject)
                .ToArray();

        public IEnumerable<T> GetObjectsByType<T>() where T : class, IGameObject
            => Native_GetObjectsByPrototype(NativeGameObjectPrototypes<T>.ConstructorObj!)
                .Select(NativeGameObjectUtils.CreateWrapperForObject)
                .Cast<T>()
                .ToArray();

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
