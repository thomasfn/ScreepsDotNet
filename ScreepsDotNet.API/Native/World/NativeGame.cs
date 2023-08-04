using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    internal interface INativeObject
    {
        void InvalidateProxyObject();
    }

    internal interface INativeRoot
    {
        JSObject GameObj { get; }

        JSObject CreepsObj { get; }

        JSObject FlagsObj { get; }

        JSObject RoomsObj { get; }

        JSObject SpawnsObj { get; }

        JSObject StructuresObj { get; }

        void BeginTracking(INativeObject nativeObject);

        void EndTracking(INativeObject nativeObject);

        JSObject GetObjectById(string id);
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

        [JSImport("get", "object")]
        [return: JSMarshalAsAttribute<JSType.Function<JSType.String, JSType.Object>>]
        internal static partial Func<string, JSObject> Native_Get_GetObjectByIdFunc([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string key);

        #endregion

        internal JSObject ProxyObject;

        private readonly NativeCpu nativeCpu;

        private readonly IList<WeakReference<INativeObject>> trackedNativeObjects = new List<WeakReference<INativeObject>>();

        private Func<string, JSObject> getObjectByIdFunc;

        private readonly NativeObjectLazyLookup<ICreep> creepLazyLookup;
        private readonly NativeObjectLazyLookup<IFlag> flagLazyLookup;
        private readonly NativeObjectLazyLookup<IRoom> roomLazyLookup;
        private readonly NativeObjectLazyLookup<IStructureSpawn> spawnLazyLookup;
        private readonly NativeObjectLazyLookup<IStructure> structureLazyLookup;

        public JSObject GameObj => ProxyObject;

        public JSObject CreepsObj { get; private set; }

        public JSObject FlagsObj { get; private set; }

        public JSObject RoomsObj { get; private set; }

        public JSObject SpawnsObj { get; private set; }

        public JSObject StructuresObj { get; private set; }

        public ICpu Cpu => nativeCpu;

        public IReadOnlyDictionary<string, ICreep> Creeps => creepLazyLookup;

        public IReadOnlyDictionary<string, IFlag> Flags => flagLazyLookup;

        public IReadOnlyDictionary<string, IRoom> Rooms => roomLazyLookup;

        public IReadOnlyDictionary<string, IStructureSpawn> Spawns => spawnLazyLookup;

        public IReadOnlyDictionary<string, IStructure> Structures => structureLazyLookup;

        public long Time => ProxyObject.GetPropertyAsInt32("time");

        public NativeGame()
        {
            ProxyObject = Native_GetGameObject();
            getObjectByIdFunc = Native_Get_GetObjectByIdFunc(ProxyObject, "getObjectById");
            CreepsObj = ProxyObject.GetPropertyAsJSObject("creeps")!;
            FlagsObj = ProxyObject.GetPropertyAsJSObject("flags")!;
            RoomsObj = ProxyObject.GetPropertyAsJSObject("rooms")!;
            SpawnsObj = ProxyObject.GetPropertyAsJSObject("spawns")!;
            StructuresObj = ProxyObject.GetPropertyAsJSObject("structures")!;
            nativeCpu = new NativeCpu(ProxyObject.GetPropertyAsJSObject("cpu")!);
            creepLazyLookup = new NativeObjectLazyLookup<ICreep>(() => CreepsObj, x => x.Name, (name, proxyObject) => new NativeCreep(this, proxyObject, name));
            flagLazyLookup = new NativeObjectLazyLookup<IFlag>(() => FlagsObj, x => x.Name, (name, proxyObject) => new NativeFlag(this, proxyObject, name));
            roomLazyLookup = new NativeObjectLazyLookup<IRoom>(() => RoomsObj, x => x.Name, (name, proxyObject) => new NativeRoom(this, proxyObject, name));
            spawnLazyLookup = new NativeObjectLazyLookup<IStructureSpawn>(() => SpawnsObj, x => x.Name, (name, proxyObject) => new NativeStructureSpawn(this, proxyObject, name));
            structureLazyLookup = new NativeObjectLazyLookup<IStructure>(() => StructuresObj, x => x.Id, (name, proxyObject) => NativeRoomObjectUtils.CreateWrapperForRoomObject<IStructure>(this, proxyObject));
            trackedNativeObjects.Add(new WeakReference<INativeObject>(creepLazyLookup));
            trackedNativeObjects.Add(new WeakReference<INativeObject>(flagLazyLookup));
            trackedNativeObjects.Add(new WeakReference<INativeObject>(roomLazyLookup));
            trackedNativeObjects.Add(new WeakReference<INativeObject>(spawnLazyLookup));
            trackedNativeObjects.Add(new WeakReference<INativeObject>(structureLazyLookup));
        }

        public void Tick()
        {
            ProxyObject.Dispose();
            ProxyObject = Native_GetGameObject();
            getObjectByIdFunc = Native_Get_GetObjectByIdFunc(ProxyObject, "getObjectById");
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
            int pruneCount = 0;
            int totalCount = trackedNativeObjects.Count;
            for (int i = totalCount - 1; i >= 0; --i)
            {
                if (trackedNativeObjects[i].TryGetTarget(out var nativeObject) && nativeObject != null)
                {
                    nativeObject.InvalidateProxyObject();
                }
                else
                {
                    trackedNativeObjects.RemoveAt(i);
                    ++pruneCount;
                }
            }
            if (pruneCount > 0)
            {
                Console.WriteLine($"NativeGame: pruned {pruneCount} of {totalCount} objects from the internal tracking list");
            }
        }

        void INativeRoot.BeginTracking(INativeObject nativeObject)
            => trackedNativeObjects.Add(new WeakReference<INativeObject>(nativeObject));

        void INativeRoot.EndTracking(INativeObject nativeObject)
        {
            for (int i = 0; i < trackedNativeObjects.Count; i++)
            {
                if (trackedNativeObjects[i].TryGetTarget(out var target) && ReferenceEquals(target, nativeObject))
                {
                    trackedNativeObjects.RemoveAt(i);
                    return;
                }
            }
        }

        JSObject INativeRoot.GetObjectById(string id)
            => getObjectByIdFunc(id);

        public T? GetObjectById<T>(string id) where T : class, IRoomObject
            => NativeRoomObjectUtils.CreateWrapperForRoomObject<T>(this, getObjectByIdFunc(id));
    }
}
