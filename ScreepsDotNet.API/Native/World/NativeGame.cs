using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

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

        JSObject GetProxyObjectById(string id);

        IWithId? GetExistingWrapperObjectById(string id);

        T? GetExistingWrapperObjectById<T>(string id) where T : class, INativeObject, IWithId;

        T? GetOrCreateWrapperObject<T>(JSObject? proxyObject) where T : class, IRoomObject;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public partial class NativeGame : IGame, INativeRoot
    {
        #region Imports

        [JSImport("getGameObj", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetGameObject();

        [JSImport("getKeysOf", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.String>>]
        internal static partial string[] Native_GetKeysOf([JSMarshalAs<JSType.Object>] JSObject obj);

        [JSImport("game.getObjectById", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetObjectById([JSMarshalAs<JSType.String>] string id);

        [JSImport("game.notify", "game")]
        internal static partial void Native_Notify([JSMarshalAs<JSType.String>] string message, [JSMarshalAs<JSType.Number>] int groupInterval);

        #endregion

        internal JSObject ProxyObject;

        private readonly NativeCpu nativeCpu;
        private readonly NativeMap nativeMap;
        private readonly NativePathFinder nativePathFinder;
        private readonly NativeConstants nativeConstants;

        private readonly IDictionary<string, WeakReference<IWithId>> objectsByIdCache = new Dictionary<string, WeakReference<IWithId>>();

        private readonly NativeObjectLazyLookup<NativeCreep, ICreep> creepLazyLookup;
        private readonly NativeObjectLazyLookup<NativeFlag, IFlag> flagLazyLookup;
        private readonly NativeObjectLazyLookup<NativeRoom, IRoom> roomLazyLookup;
        private readonly NativeObjectLazyLookup<NativeStructureSpawn, IStructureSpawn> spawnLazyLookup;
        private readonly NativeObjectLazyLookup<NativeStructure, IStructure> structureLazyLookup;

        public JSObject GameObj => ProxyObject;

        public JSObject CreepsObj { get; private set; }

        public JSObject FlagsObj { get; private set; }

        public JSObject RoomsObj { get; private set; }

        public JSObject SpawnsObj { get; private set; }

        public JSObject StructuresObj { get; private set; }

        public int TickIndex { get; private set; }

        public ICpu Cpu => nativeCpu;

        public IMap Map => nativeMap;

        public IPathFinder PathFinder => nativePathFinder;

        public IConstants Constants => nativeConstants;

        public IReadOnlyDictionary<string, ICreep> Creeps => creepLazyLookup;

        public IReadOnlyDictionary<string, IFlag> Flags => flagLazyLookup;

        public IReadOnlyDictionary<string, IRoom> Rooms => roomLazyLookup;

        public IReadOnlyDictionary<string, IStructureSpawn> Spawns => spawnLazyLookup;

        public IReadOnlyDictionary<string, IStructure> Structures => structureLazyLookup;

        public long Time => ProxyObject.GetPropertyAsInt32("time");

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
            nativePathFinder = new NativePathFinder();
            nativeConstants = new NativeConstants();
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
            if (TickIndex % 10 == 0)
            {
                // TODO: Do we want a more sophisticated way of doing this, e.g. detect when a GC happened?
                PruneObjectsByIdCache();
            }
        }

        private void PruneObjectsByIdCache()
        {
            var pendingRemoval = new HashSet<string>();
            foreach (var (name, obj) in objectsByIdCache)
            {
                if (!obj.TryGetTarget(out _)) { pendingRemoval.Add(name); }
            }
            if (pendingRemoval.Count > 0)
            {
                Console.WriteLine($"NativeGame: pruning {pendingRemoval.Count} of {objectsByIdCache.Count} objects from the objects-by-id cache");
            }
            foreach (var key in pendingRemoval)
            {
                objectsByIdCache.Remove(key);
            }
        }

        public T? GetObjectById<T>(string id) where T : class, IRoomObject
            => (this as INativeRoot).GetOrCreateWrapperObject<T>(Native_GetObjectById(id)) as T;

        public void Notify(string message, int groupInterval = 0)
            => Native_Notify(message, groupInterval);

        public ICostMatrix CreateCostMatrix()
            => new NativeCostMatrix();

        public IRoomVisual CreateRoomVisual(string? roomName = null)
            => new NativeRoomVisual(roomName);

        JSObject INativeRoot.GetProxyObjectById(string id)
            => Native_GetObjectById(id);

        IWithId? INativeRoot.GetExistingWrapperObjectById(string id)
            => (objectsByIdCache.TryGetValue(id, out var objRef) && objRef.TryGetTarget(out var obj)) ? obj : null;

        T? INativeRoot.GetExistingWrapperObjectById<T>(string id) where T : class
            => (this as INativeRoot).GetExistingWrapperObjectById(id) as T;

        T? INativeRoot.GetOrCreateWrapperObject<T>(JSObject? proxyObject) where T : class
        {
            if (proxyObject == null) { return null; }
            if (typeof(T).IsAssignableTo(typeof(IWithId)))
            {
                var id = proxyObject.GetPropertyAsString("id");
                if (id != null)
                {
                    var existingObj = (this as INativeRoot).GetExistingWrapperObjectById(id);
                    if (existingObj is T existingObjT) { return existingObjT; }
                    if (NativeRoomObjectUtils.CreateWrapperForRoomObject(this, proxyObject, typeof(T), id) is not T newObj) { return null; }
                    if (newObj is IWithId newObjWithId) { objectsByIdCache[newObjWithId.Id] = new WeakReference<IWithId>(newObjWithId); }
                    return newObj;
                }
            }
            return NativeRoomObjectUtils.CreateWrapperForRoomObject(this, proxyObject, typeof(T)) as T;
        }
    }
}
