using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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

        NativeObject? GetExistingWrapperObject(JSObject proxyObject);

        T? GetExistingWrapperObject<T>(JSObject proxyObject) where T : NativeObject;

        NativeRoom? GetExistingRoomByCoord(RoomCoord coord);

        NativeRoom? GetRoomByCoord(RoomCoord coord);

        NativeRoom? GetRoomByProxyObject(JSObject? proxyObject);

        T? GetOrCreateWrapperObject<T>(JSObject? proxyObject) where T : class, IRoomObject;

        T? GetOrCreateWrapperObject<T>(in RoomObjectMetadata roomObjectMetadata) where T : class, IRoomObject;

        IEnumerable<T> GetWrapperObjectsFromBuffer<T>(ReadOnlySpan<RoomObjectMetadata> objectMetadatas) where T : class, IRoomObject;
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

        [JSImport("powerCreep.create", "game")]
        internal static partial int Native_CreatePowerCreep(string name, Name className);

        #endregion

        internal JSObject ProxyObject;

        private readonly NativeCpu nativeCpu;
        private readonly NativeInterShardMemory nativeInterShardMemory;
        private readonly NativeMap nativeMap;
        private readonly NativeMarket nativeMarket;
        private readonly NativePathFinder nativePathFinder;
        private readonly NativeConstants nativeConstants;
        private readonly NativeRawMemory nativeRawMemory;

        private readonly Dictionary<RoomCoord, GCHandle> roomsByCoordCache = [];

        private readonly NativeObjectLazyLookup<NativeCreep, ICreep> creepLazyLookup;
        private readonly NativeObjectLazyLookup<NativeFlag, IFlag> flagLazyLookup;
        private readonly NativeObjectLazyLookup<NativePowerCreep, IPowerCreep> powerCreepLazyLookup;
        private readonly NativeObjectLazyLookup<NativeRoom, IRoom> roomLazyLookup;
        private readonly NativeObjectLazyLookup<NativeStructureSpawn, IStructureSpawn> spawnLazyLookup;
        private readonly NativeObjectLazyLookup<NativeStructure, IStructure> structureLazyLookup;

        private readonly List<NativeObject> batchRenewList = [];
        private readonly List<RoomCoord> roomCoordPendingRemovalList = [];
        private IntPtr[] batchRenewJSHandleList = new IntPtr[32];

        private long? timeCache;
        private ShardInfo? shardInfoCache;

        public JSObject GameObj => ProxyObject;

        public JSObject CreepsObj { get; private set; }

        public JSObject FlagsObj { get; private set; }

        public JSObject PowerCreepsObj { get; private set; }

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

        public IReadOnlyDictionary<string, IPowerCreep> PowerCreeps => powerCreepLazyLookup;

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
            PowerCreepsObj = ProxyObject.GetPropertyAsJSObject(Names.PowerCreeps)!;
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
            powerCreepLazyLookup = new NativeObjectLazyLookup<NativePowerCreep, IPowerCreep>(() => PowerCreepsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetExistingWrapperObject<NativePowerCreep>(proxyObject));
            roomLazyLookup = new NativeObjectLazyLookup<NativeRoom, IRoom>(() => RoomsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetRoomByCoord(new(name)));
            spawnLazyLookup = new NativeObjectLazyLookup<NativeStructureSpawn, IStructureSpawn>(() => SpawnsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeStructureSpawn>(proxyObject));
            structureLazyLookup = new NativeObjectLazyLookup<NativeStructure, IStructure>(() => StructuresObj, x => x.Id, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeStructure>(proxyObject));
            NativeRoomObjectTypes.RegisterTypesIfNeeded();
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
            PowerCreepsObj.Dispose();
            PowerCreepsObj = ProxyObject.GetPropertyAsJSObject(Names.PowerCreeps)!;
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
                PruneRoomsByCoordCache();
            }
            Native_CheckIn();
        }

        private void PruneRoomsByCoordCache()
        {
            foreach (var (coord, objRef) in roomsByCoordCache)
            {
                if (!objRef.IsAllocated || objRef.Target is not IRoom room || !room.Exists) { roomCoordPendingRemovalList.Add(coord); }
            }
            if (roomCoordPendingRemovalList.Count == 0) { return; }
            Console.WriteLine($"NativeGame: pruning {roomCoordPendingRemovalList.Count} of {roomsByCoordCache.Count} objects from the rooms-by-coord cache");
            foreach (var roomCoord in roomCoordPendingRemovalList)
            {
                roomsByCoordCache.Remove(roomCoord);
            }
            roomCoordPendingRemovalList.Clear();
        }

        public T? GetObjectById<T>(string id) where T : class, IRoomObject
        {
            var proxyObject = (this as INativeRoot).GetProxyObjectById(new(id));
            return (this as INativeRoot).GetOrCreateWrapperObject<T>(proxyObject);
        }

        public T? GetObjectById<T>(ObjectId id) where T : class, IRoomObject
        {
            var proxyObject = (this as INativeRoot).GetProxyObjectById(id);
            return (this as INativeRoot).GetOrCreateWrapperObject<T>(proxyObject);
        }

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
                if (batchRenewJSHandleList[i] == -1)
                {
                    batchRenewList[i].ReplaceProxyObject(null);
                }
                else
                {
                    batchRenewList[i].RenewProxyObject();
                }
            }
            batchRenewList.Clear();
        }

        public PowerCreepCreateResult CreatePowerCreep(string name, PowerCreepClass @class)
            => (PowerCreepCreateResult)Native_CreatePowerCreep(name, @class.ToJS());

        JSObject? INativeRoot.GetProxyObjectById(ObjectId id)
        {
            IntPtr jsHandle;
            ScreepsDotNet_Native.RawObjectId rawId = new();
            id.ToBytes(rawId.AsSpan);
            unsafe
            {
                jsHandle = ScreepsDotNet_Native.GetObjectById(&rawId);
            }
            if (jsHandle == -1) { return null; }
            return Interop.Native.GetJSObject(jsHandle);
        }

        NativeObject? INativeRoot.GetExistingWrapperObject(JSObject proxyObject)
            => proxyObject.UserData as NativeObject;

        T? INativeRoot.GetExistingWrapperObject<T>(JSObject proxyObject) where T : class
            => proxyObject.UserData as T;

        NativeRoom? INativeRoot.GetExistingRoomByCoord(RoomCoord coord)
            => (roomsByCoordCache.TryGetValue(coord, out var gcHandle) && gcHandle.Target is NativeRoom room && room.Exists) ? room : null;

        NativeRoom? INativeRoot.GetRoomByCoord(RoomCoord coord)
        {
            var room = (this as INativeRoot).GetExistingRoomByCoord(coord);
            if (room != null) { return room; }
            var proxyObject = RoomsObj.GetPropertyAsJSObject(coord.ToString());
            if (proxyObject == null) { return null; }
            room = new NativeRoom(this, proxyObject);
            proxyObject.UserData = room;
            roomsByCoordCache[coord] = GCHandle.Alloc(room, GCHandleType.WeakTrackResurrection);
            return room;
        }

        NativeRoom? INativeRoot.GetRoomByProxyObject(JSObject? proxyObject)
        {
            if (proxyObject == null) { return null; }
            var coord = new RoomCoord(proxyObject.GetPropertyAsString(Names.Name)!);
            var room = (this as INativeRoot).GetExistingWrapperObject<NativeRoom>(proxyObject);
            if (room != null) { return room; }
            room = new NativeRoom(this, proxyObject);
            roomsByCoordCache[coord] = GCHandle.Alloc(room, GCHandleType.WeakTrackResurrection);
            return room;
        }

        T? INativeRoot.GetOrCreateWrapperObject<T>(JSObject? proxyObject) where T : class
        {
            if (proxyObject == null) { return null; }
            if (proxyObject.UserData is T existingWrapperObject) { return existingWrapperObject; }
            var newWrapperObject = NativeRoomObjectTypes.CreateWrapperForRoomObject<T>(this, proxyObject);
            if (newWrapperObject == null) { return null; }
            proxyObject.UserData = newWrapperObject;
            return newWrapperObject;
        }

        T? INativeRoot.GetOrCreateWrapperObject<T>(in RoomObjectMetadata metadata) where T : class
        {
            var proxyObject = Interop.Native.GetJSObject(metadata.JSHandle);
            if (proxyObject.UserData != null) { return proxyObject.UserData as T; }
            var wrapperObject = NativeRoomObjectTypes.CreateWrapperForRoomObject(this, proxyObject, metadata, NativeRoomObjectTypes.TypeOf<T>());
            if (wrapperObject is not T newWrapperObject)
            {
                Console.WriteLine($"Failed to create wrapper object for {proxyObject} (wrong type - expecting {typeof(T)}, got {wrapperObject?.GetType()})");
                return null;
            }
            proxyObject.UserData = newWrapperObject;
            return newWrapperObject;
        }

        IEnumerable<T> INativeRoot.GetWrapperObjectsFromBuffer<T>(ReadOnlySpan<RoomObjectMetadata> objectMetadatas)
        {
            int cnt = objectMetadatas.Length;
            if (cnt == 0) { return Enumerable.Empty<T>(); }
            List<T> result = new(cnt);
            for (int i = 0; i < cnt; ++i)
            {
                var wrapperObj = (this as INativeRoot).GetOrCreateWrapperObject<T>(objectMetadatas[i]);
                if (wrapperObj == null) { continue; }
                result.Add(wrapperObj);
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
