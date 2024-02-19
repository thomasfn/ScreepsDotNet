using System;
using System.Collections.Generic;
using System.Linq;

using ScreepsDotNet.Interop;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    internal interface INativeRoot
    {
        JSObject GameObj { get; }

        JSObject CreepsObj { get; }

        JSObject FlagsObj { get; }

        JSObject RoomsObj { get; }

        JSObject SpawnsObj { get; }

        JSObject StructuresObj { get; }

        int TickIndex { get; }

        double CpuTime { get; }

        JSObject? GetProxyObjectById(ObjectId id);

        IWithId? GetExistingWrapperObjectById(ObjectId id);

        NativeRoom? GetExistingRoomByCoord(RoomCoord coord);

        NativeRoom? GetRoomByCoord(RoomCoord coord);

        NativeRoom? GetRoomByProxyObject(JSObject? proxyObject);

        T? GetExistingWrapperObjectById<T>(ObjectId id) where T : NativeObject, IWithId;

        T? GetOrCreateWrapperObject<T>(JSObject? proxyObject) where T : class, IRoomObject;

        IEnumerable<T> GetWrapperObjectsFromBuffers<T>(ReadOnlySpan<ScreepsDotNet_Native.RawObjectId> rawObjectIds, ReadOnlySpan<RoomObjectMetadata> objectMetadatas) where T : class, IRoomObject;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    public partial class NativeGame : IGame, INativeRoot
    {
        #region Imports

        [JSImport("checkIn", "game")]
        internal static partial void Native_CheckIn();

        [JSImport("getGameObj", "game")]
        internal static partial JSObject Native_GetGameObject();

        [JSImport("getMemoryObj", "game")]
        internal static partial JSObject Native_GetMemoryObj();

        [JSImport("getRawMemoryObj", "game")]
        internal static partial JSObject Native_GetRawMemoryObj();

        [JSImport("game.getObjectById", "game")]
        internal static partial JSObject? Native_GetObjectById(string id);

        [JSImport("game.notify", "game")]
        internal static partial void Native_Notify(string message, int groupInterval);

        #endregion

        internal JSObject ProxyObject;

        private readonly NativeCpu nativeCpu;
        private readonly NativeInterShardMemory nativeInterShardMemory;
        private readonly NativeMap nativeMap;
        private readonly NativeMarket nativeMarket;
        private readonly NativePathFinder nativePathFinder;
        private readonly NativeConstants nativeConstants;
        private readonly NativeRawMemory nativeRawMemory;

        private readonly Dictionary<ObjectId, WeakReference<IWithId>> objectsByIdCache = new();
        private readonly Dictionary<RoomCoord, WeakReference<NativeRoom>> roomsByCoordCache = new();

        private readonly NativeObjectLazyLookup<NativeCreep, ICreep> creepLazyLookup;
        private readonly NativeObjectLazyLookup<NativeFlag, IFlag> flagLazyLookup;
        private readonly NativeObjectLazyLookup<NativeRoom, IRoom> roomLazyLookup;
        private readonly NativeObjectLazyLookup<NativeStructureSpawn, IStructureSpawn> spawnLazyLookup;
        private readonly NativeObjectLazyLookup<NativeStructure, IStructure> structureLazyLookup;

        private readonly List<NativeObject> batchRenewList = [];
        private IntPtr[] batchRenewJSHandleList = new IntPtr[32];

        private long? timeCache;
        private ShardInfo? shardInfoCache;

        public JSObject GameObj => ProxyObject;

        public JSObject CreepsObj { get; private set; }

        public JSObject FlagsObj { get; private set; }

        public JSObject RoomsObj { get; private set; }

        public JSObject SpawnsObj { get; private set; }

        public JSObject StructuresObj { get; private set; }

        public int TickIndex { get; private set; }

        public double CpuTime => nativeCpu.GetUsed();

        public ICpu Cpu => nativeCpu;

        public IInterShardMemory InterShardMemory => nativeInterShardMemory;

        public IMap Map => nativeMap;

        public IMarket Market => nativeMarket;

        public IPathFinder PathFinder => nativePathFinder;

        public IConstants Constants => nativeConstants;

        public IMemoryObject Memory => new NativeMemoryObject(Native_GetMemoryObj());

        public IRawMemory RawMemory => nativeRawMemory;

        public IReadOnlyDictionary<string, ICreep> Creeps => creepLazyLookup;

        public IReadOnlyDictionary<string, IFlag> Flags => flagLazyLookup;

        public IReadOnlyDictionary<string, IRoom> Rooms => roomLazyLookup;

        public IReadOnlyDictionary<string, IStructureSpawn> Spawns => spawnLazyLookup;

        public ShardInfo Shard => shardInfoCache ??= GetShardInfo();

        public IReadOnlyDictionary<string, IStructure> Structures => structureLazyLookup;

        public long Time => timeCache ??= ProxyObject.GetPropertyAsInt32(Names.Time);

        public NativeGame()
        {
            ProxyObject = Native_GetGameObject();
            CreepsObj = ProxyObject.GetPropertyAsJSObject(Names.Creeps)!;
            FlagsObj = ProxyObject.GetPropertyAsJSObject(Names.Flags)!;
            RoomsObj = ProxyObject.GetPropertyAsJSObject(Names.Rooms)!;
            SpawnsObj = ProxyObject.GetPropertyAsJSObject(Names.Spawns)!;
            StructuresObj = ProxyObject.GetPropertyAsJSObject(Names.Structures)!;
            nativeCpu = new NativeCpu(ProxyObject.GetPropertyAsJSObject(Names.Cpu)!);
            nativeInterShardMemory = new NativeInterShardMemory();
            nativeMap = new NativeMap();
            nativeMarket = new NativeMarket(ProxyObject.GetPropertyAsJSObject(Names.Market)!);
            nativePathFinder = new NativePathFinder();
            nativeConstants = new NativeConstants();
            nativeRawMemory = new NativeRawMemory(Native_GetRawMemoryObj());
            var nativeRoot = this as INativeRoot;
            creepLazyLookup = new NativeObjectLazyLookup<NativeCreep, ICreep>(() => CreepsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeCreep>(proxyObject));
            flagLazyLookup = new NativeObjectLazyLookup<NativeFlag, IFlag>(() => FlagsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeFlag>(proxyObject));
            roomLazyLookup = new NativeObjectLazyLookup<NativeRoom, IRoom>(() => RoomsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetRoomByCoord(new(name)));
            spawnLazyLookup = new NativeObjectLazyLookup<NativeStructureSpawn, IStructureSpawn>(() => SpawnsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeStructureSpawn>(proxyObject));
            structureLazyLookup = new NativeObjectLazyLookup<NativeStructure, IStructure>(() => StructuresObj, x => x.Id, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeStructure>(proxyObject));
        }

        public void Tick()
        {
            ++TickIndex;
            ProxyObject.Dispose();
            ProxyObject = Native_GetGameObject();
            CreepsObj.Dispose();
            CreepsObj = ProxyObject.GetPropertyAsJSObject(Names.Creeps)!;
            FlagsObj.Dispose();
            FlagsObj = ProxyObject.GetPropertyAsJSObject(Names.Flags)!;
            RoomsObj.Dispose();
            RoomsObj = ProxyObject.GetPropertyAsJSObject(Names.Rooms)!;
            SpawnsObj.Dispose();
            SpawnsObj = ProxyObject.GetPropertyAsJSObject(Names.Spawns)!;
            StructuresObj.Dispose();
            StructuresObj = ProxyObject.GetPropertyAsJSObject(Names.Structures)!;
            nativeCpu.ProxyObject.Dispose();
            nativeCpu.ProxyObject = ProxyObject.GetPropertyAsJSObject(Names.Cpu)!;
            nativeMarket.ProxyObject.Dispose();
            nativeMarket.ProxyObject = ProxyObject.GetPropertyAsJSObject(Names.Market)!;
            nativeRawMemory.ProxyObject.Dispose();
            nativeRawMemory.ProxyObject = Native_GetRawMemoryObj();
            creepLazyLookup.InvalidateProxyObject();
            flagLazyLookup.InvalidateProxyObject();
            roomLazyLookup.InvalidateProxyObject();
            spawnLazyLookup.InvalidateProxyObject();
            structureLazyLookup.InvalidateProxyObject();
            timeCache = null;
            if (TickIndex % 10 == 0)
            {
                // TODO: Do we want a more sophisticated way of doing this, e.g. detect when a GC happened?
                PruneObjectsByIdCache();
                PruneRoomsByCoordCache();
            }
            Native_CheckIn();
        }

        private void PruneObjectsByIdCache()
        {
            List<ObjectId>? pendingRemoval = null;
            foreach (var (id, objRef) in objectsByIdCache)
            {
                if (!objRef.TryGetTarget(out _)) { (pendingRemoval ??= new()).Add(id); }
            }
            if (pendingRemoval == null) { return; }
            Console.WriteLine($"NativeGame: pruning {pendingRemoval.Count} of {objectsByIdCache.Count} objects from the objects-by-id cache");
            foreach (var key in pendingRemoval)
            {
                objectsByIdCache.Remove(key);
            }
        }

        private void PruneRoomsByCoordCache()
        {
            List<RoomCoord>? pendingRemoval = null;
            foreach (var (coord, objRef) in roomsByCoordCache)
            {
                if (!objRef.TryGetTarget(out var room) || !room.Exists) { (pendingRemoval ??= new()).Add(coord); }
            }
            if (pendingRemoval == null) { return; }
            Console.WriteLine($"NativeGame: pruning {pendingRemoval.Count} of {roomsByCoordCache.Count} objects from the rooms-by-coord cache");
            foreach (var key in pendingRemoval)
            {
                roomsByCoordCache.Remove(key);
            }
        }

        public T? GetObjectById<T>(string id) where T : class, IRoomObject
            => (this as INativeRoot).GetOrCreateWrapperObject<T>(Native_GetObjectById(id));

        public T? GetObjectById<T>(ObjectId id) where T : class, IRoomObject
            => GetObjectById<T>((string)id);

        public void Notify(string message, int groupInterval = 0)
            => Native_Notify(message, groupInterval);

        public ICostMatrix CreateCostMatrix()
            => new NativeCostMatrix();

        public IRoomVisual CreateRoomVisual(string? roomName = null)
            => new NativeRoomVisual(roomName);

        public IMemoryObject CreateMemoryObject()
            => new NativeMemoryObject(JSObject.Create());

        public void BatchRenewObjects(IEnumerable<IRoomObject> roomObjects)
        {
            int cnt = 0;
            foreach (var roomObject in roomObjects)
            {
                var nativeObject = (roomObject as NativeObject)!;
                var jsHandle = nativeObject.ProxyObjectJSHandle;
                if (jsHandle == null) { continue; }
                if (!nativeObject.Stale) { continue; }
                batchRenewList.Add(nativeObject);
                if (cnt >= batchRenewJSHandleList.Length - 1)
                {
                    Array.Resize(ref batchRenewJSHandleList, batchRenewJSHandleList.Length * 2);
                }
                batchRenewJSHandleList[cnt] = jsHandle.Value;
                ++cnt;
            }
            if (cnt == 0) { return; }
            unsafe
            {
                fixed (IntPtr* batchRenewJSHandleListPtr = batchRenewJSHandleList)
                {
                    ScreepsDotNet_Native.BatchRenewObjects(batchRenewJSHandleListPtr, cnt);
                }
            }
            for (int i = 0; i < cnt; ++i)
            {
                batchRenewList[i].NotifyBatchRenew(batchRenewJSHandleList[i] == -1);
            }
            batchRenewList.Clear();
        }

        JSObject? INativeRoot.GetProxyObjectById(ObjectId id)
        {
            IntPtr jsHandle;
            unsafe
            {
                jsHandle = ScreepsDotNet_Native.GetObjectById(&id);
            }
            if (jsHandle == -1) { return null; }
            return Interop.Native.GetJSObject(jsHandle);
        }

        IWithId? INativeRoot.GetExistingWrapperObjectById(ObjectId id)
            => (objectsByIdCache.TryGetValue(id, out var objRef) && objRef.TryGetTarget(out var obj)) ? obj : null;

        T? INativeRoot.GetExistingWrapperObjectById<T>(ObjectId id) where T : class
            => (this as INativeRoot).GetExistingWrapperObjectById(id) as T;

        NativeRoom? INativeRoot.GetExistingRoomByCoord(RoomCoord coord)
            => (roomsByCoordCache.TryGetValue(coord, out var objRef) && objRef.TryGetTarget(out var obj) && obj.Exists) ? obj : null;

        NativeRoom? INativeRoot.GetRoomByCoord(RoomCoord coord)
        {
            var room = (this as INativeRoot).GetExistingRoomByCoord(coord);
            if (room != null) { return room; }
            var proxyObject = RoomsObj.GetPropertyAsJSObject(coord.ToString());
            if (proxyObject == null) { return null; }
            room = new NativeRoom(this, proxyObject);
            roomsByCoordCache[coord] = new WeakReference<NativeRoom>(room);
            return room;
        }

        NativeRoom? INativeRoot.GetRoomByProxyObject(JSObject? proxyObject)
        {
            if (proxyObject == null) { return null; }
            var coord = new RoomCoord(proxyObject.GetPropertyAsString(Names.Name)!);
            var room = (this as INativeRoot).GetExistingRoomByCoord(coord);
            if (room != null) { return room; }
            room = new NativeRoom(this, proxyObject);
            roomsByCoordCache[coord] = new WeakReference<NativeRoom>(room);
            return room;
        }

        T? INativeRoot.GetOrCreateWrapperObject<T>(JSObject? proxyObject) where T : class
        {
            if (proxyObject == null) { return null; }
            if (typeof(T).IsAssignableTo(typeof(IWithId)))
            {
                var id = proxyObject.GetPropertyAsString(Names.Id);
                if (!string.IsNullOrEmpty(id))
                {
                    var objId = new ObjectId(id);
                    var existingObj = (this as INativeRoot).GetExistingWrapperObjectById(objId);
                    if (existingObj is NativeObject existingNativeObj)
                    {
                        // If there's already a wrapper, we need to give it this new proxy object and ensure it disposes of the old one to prevent a memory leak
                        existingNativeObj.ReplaceProxyObject(proxyObject);
                    }
                    if (existingObj is T existingObjT) { return existingObjT; }
                    if (existingObj != null)
                    {
                        // Do not create another wrapper if one exists but was the wrong type (should be be throwing or reporting this error?)
                        return null;
                    }
                    if (NativeRoomObjectUtils.CreateWrapperForRoomObject(this, proxyObject, typeof(T), objId) is not T newObj) { return null; }
                    if (newObj is IWithId newObjWithId) { objectsByIdCache[objId] = new WeakReference<IWithId>(newObjWithId); }
                    return newObj;
                }
            }
            return NativeRoomObjectUtils.CreateWrapperForRoomObject(this, proxyObject, typeof(T)) as T;
        }

        IEnumerable<T> INativeRoot.GetWrapperObjectsFromBuffers<T>(ReadOnlySpan<ScreepsDotNet_Native.RawObjectId> rawObjectIds, ReadOnlySpan<RoomObjectMetadata> objectMetadatas)
        {
            int cnt = Math.Min(rawObjectIds.Length, objectMetadatas.Length);
            if (cnt == 0) { return Enumerable.Empty<T>(); }
            Span<ObjectId> objectIds = stackalloc ObjectId[cnt];
            unsafe
            {
                fixed (ScreepsDotNet_Native.RawObjectId* rawObjectIdsPtr = rawObjectIds)
                {
                    fixed (ObjectId* objectIdsPtr = objectIds)
                    {
                        ScreepsDotNet_Native.DecodeObjectIds(rawObjectIdsPtr, cnt, objectIdsPtr);
                    }
                }
            }
            List<T> result = new(cnt);
            for (int i = 0; i < cnt; ++i)
            {
                ref ObjectId objectId = ref objectIds[i];
                if (!objectId.IsValid) { continue; }
                if (objectsByIdCache.TryGetValue(objectId, out var weakRef) && weakRef.TryGetTarget(out var existingObj) && existingObj is T existingObjT)
                {
                    result.Add(existingObjT);
                    continue;
                }
                var roomObject = NativeRoomObjectUtils.CreateWrapperForRoomObject<T>(this, objectId, objectMetadatas[i]);
                if (roomObject == null) { continue; }
                if (roomObject is IWithId withId)
                {
                    objectsByIdCache.TryAdd(objectId, new WeakReference<IWithId>(withId));
                }
                result.Add(roomObject);
            }
            return result;
        }

        private ShardInfo GetShardInfo()
        {
            using var shardObj = GameObj.GetPropertyAsJSObject(Names.Shard)!;
            return new(
                name: shardObj.GetPropertyAsString(Names.Name)!,
                type: shardObj.GetPropertyAsString(Names.Type)!,
                ptr: shardObj.GetPropertyAsBoolean(Names.Ptr)!
            );
        }
    }
}
