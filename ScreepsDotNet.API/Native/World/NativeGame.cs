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

        JSObject RoomsObj { get; }

        JSObject SpawnsObj { get; }

        JSObject CreepsObj { get; }

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

        public JSObject GameObj => ProxyObject;

        public JSObject RoomsObj { get; private set; }

        public JSObject SpawnsObj { get; private set; }

        public JSObject CreepsObj { get; private set; }

        public ICpu Cpu => nativeCpu;

        public IEnumerable<IRoom> Rooms
        {
            get
            {
                var result = new List<IRoom>();
                var keys = Native_GetKeysOf(RoomsObj);
                foreach (var roomName in keys)
                {
                    result.Add(new NativeRoom(this, RoomsObj.GetPropertyAsJSObject(roomName)!, roomName));
                }
                return result;
            }
        }

        public long Time => ProxyObject.GetPropertyAsInt32("time");

        

        public NativeGame()
        {
            ProxyObject = Native_GetGameObject();
            getObjectByIdFunc = Native_Get_GetObjectByIdFunc(ProxyObject, "getObjectById");
            RoomsObj = ProxyObject.GetPropertyAsJSObject("rooms")!;
            SpawnsObj = ProxyObject.GetPropertyAsJSObject("spawns")!;
            CreepsObj = ProxyObject.GetPropertyAsJSObject("creeps")!;
            nativeCpu = new NativeCpu(ProxyObject.GetPropertyAsJSObject("cpu")!);
        }

        public void Tick()
        {
            ProxyObject.Dispose();
            ProxyObject = Native_GetGameObject();
            getObjectByIdFunc = Native_Get_GetObjectByIdFunc(ProxyObject, "getObjectById");
            RoomsObj.Dispose();
            RoomsObj = ProxyObject.GetPropertyAsJSObject("rooms")!;
            SpawnsObj.Dispose();
            SpawnsObj = ProxyObject.GetPropertyAsJSObject("spawns")!;
            CreepsObj.Dispose();
            CreepsObj = ProxyObject.GetPropertyAsJSObject("creeps")!;
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
