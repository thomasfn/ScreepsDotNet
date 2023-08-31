using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;
using System.Runtime.InteropServices;
using System.Collections;

namespace ScreepsDotNet.Native.World
{
    internal interface INativeObject
    {
        bool Exists { get; }

        JSObject? ReacquireProxyObject();
    }

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

        JSObject GetProxyObjectById(ObjectId id);

        IWithId? GetExistingWrapperObjectById(ObjectId id);

        T? GetExistingWrapperObjectById<T>(ObjectId id) where T : class, INativeObject, IWithId;

        T? GetOrCreateWrapperObject<T>(JSObject? proxyObject) where T : class, IRoomObject;

        IEnumerable<T> GetWrapperObjectsFromCopyBuffer<T>(int cnt) where T : class, IRoomObject;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public partial class NativeGame : IGame, INativeRoot
    {
        #region Imports

        [JSImport("checkIn", "game")]
        internal static partial void Native_CheckIn();

        [JSImport("getGameObj", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetGameObject();

        [JSImport("getMemoryObj", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetMemoryObj();

        [JSImport("getRawMemoryObj", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetRawMemoryObj();

        [JSImport("game.getObjectById", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetObjectById([JSMarshalAs<JSType.String>] string id);

        [JSImport("game.notify", "game")]
        internal static partial void Native_Notify([JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.Number>] int groupInterval);

        #endregion

        internal JSObject ProxyObject;

        private readonly NativeCpu nativeCpu;
        private readonly NativeMap nativeMap;
        private readonly NativeMarket nativeMarket;
        private readonly NativePathFinder nativePathFinder;
        private readonly NativeConstants nativeConstants;
        private readonly NativeRawMemory nativeRawMemory;

        private readonly Dictionary<ObjectId, WeakReference<IWithId>> objectsByIdCache = new();

        private readonly NativeObjectLazyLookup<NativeCreep, ICreep> creepLazyLookup;
        private readonly NativeObjectLazyLookup<NativeFlag, IFlag> flagLazyLookup;
        private readonly NativeObjectLazyLookup<NativeRoom, IRoom> roomLazyLookup;
        private readonly NativeObjectLazyLookup<NativeStructureSpawn, IStructureSpawn> spawnLazyLookup;
        private readonly NativeObjectLazyLookup<NativeStructure, IStructure> structureLazyLookup;

        private long? timeCache;

        public JSObject GameObj => ProxyObject;

        public JSObject CreepsObj { get; private set; }

        public JSObject FlagsObj { get; private set; }

        public JSObject RoomsObj { get; private set; }

        public JSObject SpawnsObj { get; private set; }

        public JSObject StructuresObj { get; private set; }

        public int TickIndex { get; private set; }

        public double CpuTime => nativeCpu.GetUsed();

        public ICpu Cpu => nativeCpu;

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

        public IReadOnlyDictionary<string, IStructure> Structures => structureLazyLookup;

        public long Time => timeCache ??= ProxyObject.GetPropertyAsInt32("time");

        public NativeGame()
        {
            ProxyObject = Native_GetGameObject();
            CreepsObj = ProxyObject.GetPropertyAsJSObject("creeps")!;
            FlagsObj = ProxyObject.GetPropertyAsJSObject("flags")!;
            RoomsObj = ProxyObject.GetPropertyAsJSObject("rooms")!;
            SpawnsObj = ProxyObject.GetPropertyAsJSObject("spawns")!;
            StructuresObj = ProxyObject.GetPropertyAsJSObject("structures")!;
            nativeCpu = new NativeCpu(ProxyObject.GetPropertyAsJSObject("cpu")!);
            nativeMap = new NativeMap();
            nativeMarket = new NativeMarket(ProxyObject.GetPropertyAsJSObject("market")!);
            nativePathFinder = new NativePathFinder();
            nativeConstants = new NativeConstants();
            nativeRawMemory = new NativeRawMemory(Native_GetRawMemoryObj());
            var nativeRoot = this as INativeRoot;
            creepLazyLookup = new NativeObjectLazyLookup<NativeCreep, ICreep>(() => CreepsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeCreep>(proxyObject));
            flagLazyLookup = new NativeObjectLazyLookup<NativeFlag, IFlag>(() => FlagsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeFlag>(proxyObject));
            roomLazyLookup = new NativeObjectLazyLookup<NativeRoom, IRoom>(() => RoomsObj, x => x.Name, (name, proxyObject) => new NativeRoom(this, proxyObject, name));
            spawnLazyLookup = new NativeObjectLazyLookup<NativeStructureSpawn, IStructureSpawn>(() => SpawnsObj, x => x.Name, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeStructureSpawn>(proxyObject));
            structureLazyLookup = new NativeObjectLazyLookup<NativeStructure, IStructure>(() => StructuresObj, x => x.Id, (name, proxyObject) => nativeRoot.GetOrCreateWrapperObject<NativeStructure>(proxyObject));
        }

        public void Tick()
        {
            ++TickIndex;
            ProxyObject.Dispose();
            ProxyObject = Native_GetGameObject();
            CreepsObj.Dispose();
            CreepsObj = ProxyObject.GetPropertyAsJSObject("creeps")!;
            FlagsObj.Dispose();
            FlagsObj = ProxyObject.GetPropertyAsJSObject("flags")!;
            RoomsObj.Dispose();
            RoomsObj = ProxyObject.GetPropertyAsJSObject("rooms")!;
            SpawnsObj.Dispose();
            SpawnsObj = ProxyObject.GetPropertyAsJSObject("spawns")!;
            StructuresObj.Dispose();
            StructuresObj = ProxyObject.GetPropertyAsJSObject("structures")!;
            nativeCpu.ProxyObject.Dispose();
            nativeCpu.ProxyObject = ProxyObject.GetPropertyAsJSObject("cpu")!;
            nativeMarket.ProxyObject.Dispose();
            nativeMarket.ProxyObject = ProxyObject.GetPropertyAsJSObject("market")!;
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
            }
            Native_CheckIn();
        }

        private void PruneObjectsByIdCache()
        {
            IList<ObjectId>? pendingRemoval = null;
            foreach (var (id, obj) in objectsByIdCache)
            {
                if (!obj.TryGetTarget(out _)) { (pendingRemoval ??= new List<ObjectId>()).Add(id); }
            }
            if (pendingRemoval == null) { return; }
            Console.WriteLine($"NativeGame: pruning {pendingRemoval.Count} of {objectsByIdCache.Count} objects from the objects-by-id cache");
            foreach (var key in pendingRemoval)
            {
                objectsByIdCache.Remove(key);
            }
        }

        public T? GetObjectById<T>(string id) where T : class, IRoomObject
            => (this as INativeRoot).GetOrCreateWrapperObject<T>(Native_GetObjectById(id)) as T;

        public T? GetObjectById<T>(ObjectId id) where T : class, IRoomObject
            => GetObjectById<T>((string)id);

        public void Notify(string message, int groupInterval = 0)
            => Native_Notify(message, groupInterval);

        public ICostMatrix CreateCostMatrix()
            => new NativeCostMatrix();

        public IRoomVisual CreateRoomVisual(string? roomName = null)
            => new NativeRoomVisual(roomName);

        JSObject INativeRoot.GetProxyObjectById(ObjectId id)
            => Native_GetObjectById(id);

        IWithId? INativeRoot.GetExistingWrapperObjectById(ObjectId id)
            => (objectsByIdCache.TryGetValue(id, out var objRef) && objRef.TryGetTarget(out var obj)) ? obj : null;

        T? INativeRoot.GetExistingWrapperObjectById<T>(ObjectId id) where T : class
            => (this as INativeRoot).GetExistingWrapperObjectById(id) as T;

        T? INativeRoot.GetOrCreateWrapperObject<T>(JSObject? proxyObject) where T : class
        {
            if (proxyObject == null) { return null; }
            if (typeof(T).IsAssignableTo(typeof(IWithId)))
            {
                var id = proxyObject.GetPropertyAsString("id");
                if (!string.IsNullOrEmpty(id))
                {
                    var objId = new ObjectId(id);
                    var existingObj = (this as INativeRoot).GetExistingWrapperObjectById(objId);
                    if (existingObj is T existingObjT) { return existingObjT; }
                    if (NativeRoomObjectUtils.CreateWrapperForRoomObject(this, proxyObject, typeof(T), objId) is not T newObj) { return null; }
                    if (newObj is IWithId newObjWithId) { objectsByIdCache[objId] = new WeakReference<IWithId>(newObjWithId); }
                    return newObj;
                }
            }
            return NativeRoomObjectUtils.CreateWrapperForRoomObject(this, proxyObject, typeof(T)) as T;
        }

        internal readonly struct WrapperObjectsFromCopyBufferEnumerable<T> : IEnumerable<T> where T : class, IRoomObject
        {
            private readonly INativeRoot nativeRoot;
            private readonly Dictionary<ObjectId, WeakReference<IWithId>> objectsByIdCache;
            private readonly RoomObjectDataPacket[] dataPackets;

            public WrapperObjectsFromCopyBufferEnumerable(INativeRoot nativeRoot, Dictionary<ObjectId, WeakReference<IWithId>> objectsByIdCache, RoomObjectDataPacket[] dataPackets)
            {
                this.nativeRoot = nativeRoot;
                this.objectsByIdCache = objectsByIdCache;
                this.dataPackets = dataPackets;
            }

            public IEnumerator<T> GetEnumerator()
                => new WrapperObjectsFromCopyBufferEnumerator<T>(nativeRoot, objectsByIdCache, dataPackets);

            IEnumerator IEnumerable.GetEnumerator()
                => new WrapperObjectsFromCopyBufferEnumerator<T>(nativeRoot, objectsByIdCache, dataPackets);
        }

        internal struct WrapperObjectsFromCopyBufferEnumerator<T> : IEnumerator<T> where T : class, IRoomObject
        {
            private readonly INativeRoot nativeRoot;
            private readonly Dictionary<ObjectId, WeakReference<IWithId>> objectsByIdCache;
            private readonly RoomObjectDataPacket[] dataPackets;
            private int nextIndex;
            private bool disposed;
            private T? current;

            public T Current => disposed ? throw new ObjectDisposedException(null) : current!;

            object IEnumerator.Current => disposed ? throw new ObjectDisposedException(null) : current!;

            public WrapperObjectsFromCopyBufferEnumerator(INativeRoot nativeRoot, Dictionary<ObjectId, WeakReference<IWithId>> objectsByIdCache, RoomObjectDataPacket[] dataPackets)
            {
                this.nativeRoot = nativeRoot;
                this.objectsByIdCache = objectsByIdCache;
                this.dataPackets = dataPackets;
                nextIndex = 0;
                disposed = false;
                current = null;
            }

            public bool MoveNext()
            {
                if (disposed) { throw new ObjectDisposedException(null); }
                current = null;
                while (current == null && nextIndex < dataPackets.Length)
                {
                    ref RoomObjectDataPacket dataPacket = ref dataPackets[nextIndex++];
                    if (!dataPacket.ObjectId.IsValid) { continue; }
                    if (objectsByIdCache.TryGetValue(dataPacket.ObjectId, out var weakRef) && weakRef.TryGetTarget(out var existingObj) && existingObj is T existingObjT)
                    {
                        current = existingObjT;
                        if (existingObjT is NativeObject nativeObject) { nativeObject.UpdateFromDataPacket(dataPacket); }
                        continue;
                    }
                    var roomObject = NativeRoomObjectUtils.CreateWrapperForRoomObject<T>(nativeRoot, dataPacket);
                    if (roomObject == null) { continue; }
                    current = roomObject;
                    if (roomObject is IWithId withId)
                    {
                        objectsByIdCache.TryAdd(dataPacket.ObjectId, new WeakReference<IWithId>(withId));
                    }
                }
                return current != null;
            }

            public void Reset()
            {
                if (disposed) { throw new ObjectDisposedException(null); }
                nextIndex = 0;
                current = null;
            }

            public void Dispose()
            {
                disposed = true;
            }
        }

        IEnumerable<T> INativeRoot.GetWrapperObjectsFromCopyBuffer<T>(int cnt)
        {
            if (cnt == 0) { return Enumerable.Empty<T>(); }
            RoomObjectDataPacket[] packets = new RoomObjectDataPacket[cnt];
            ReadOnlySpan<byte> dataPacketsRaw = MemoryMarshal.Cast<RoomObjectDataPacket, byte>(packets);
            unsafe
            {
                fixed (byte* p = dataPacketsRaw)
                {
                    ScreepsDotNet_Native.DecodeRoomObjectListFromCopyBuffer(p, cnt);
                }
            }
            return new WrapperObjectsFromCopyBufferEnumerable<T>(this, objectsByIdCache, packets);
        }
    }
}
