using System;
using System.Collections.Generic;

using ScreepsDotNet.API.Arena;
using ScreepsDotNet.Interop;

namespace ScreepsDotNet.Native.Arena
{
    internal interface INativeRoot
    {
        int TickIndex { get; }

        NativeGameObject GetOrCreateWrapperForObject(JSObject proxyObject);

        T? GetOrCreateWrapperForObject<T>(JSObject proxyObject) where T : NativeGameObject;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    public class NativeGame : IGame, INativeRoot
    {
        private readonly Dictionary<JSObject, WeakReference<NativeGameObject>> nativeObjectCache = [];
        private readonly List<JSObject> pendingRemoval = [];

        private int tickIndex;

        public IUtils Utils { get; }

        public IPathFinder PathFinder { get; } = new NativePathFinder();

        public IConstants Constants { get; } = new NativeConstants();

        int INativeRoot.TickIndex => tickIndex;

        public NativeGame()
        {
            Utils = new NativeUtils(this);
        }

        public void Tick()
        {
            ++tickIndex;
            if (tickIndex % 10 == 0)
            {
                PruneObjectsByIdCache();
            }
        }

        private void PruneObjectsByIdCache()
        {
            pendingRemoval.Clear();
            foreach (var (proxyObject, wrapperObjectRef) in nativeObjectCache)
            {
                if (!wrapperObjectRef.TryGetTarget(out _))
                {
                    pendingRemoval.Add(proxyObject);
                }
            }
            if (pendingRemoval.Count == 0) { return; }
            Console.WriteLine($"NativeGame: pruning {pendingRemoval.Count} of {nativeObjectCache.Count} objects from the native object cache");
            foreach (var key in pendingRemoval)
            {
                nativeObjectCache.Remove(key);
            }
        }

        NativeGameObject INativeRoot.GetOrCreateWrapperForObject(JSObject proxyObject)
        {
            if (nativeObjectCache.TryGetValue(proxyObject, out var wrapperObjectRef))
            {
                if (wrapperObjectRef.TryGetTarget(out var wrapperObject))
                {
                    return wrapperObject;
                }
                else
                {
                    wrapperObject = NativeGameObjectUtils.CreateWrapperForObject(this, proxyObject);
                    wrapperObjectRef.SetTarget(wrapperObject);
                    return wrapperObject;
                }
            }
            else
            {
                var wrapperObject = NativeGameObjectUtils.CreateWrapperForObject(this, proxyObject);
                nativeObjectCache.Add(proxyObject, new WeakReference<NativeGameObject>(wrapperObject));
                return wrapperObject;
            }
        }

        T? INativeRoot.GetOrCreateWrapperForObject<T>(JSObject proxyObject) where T : class
            => (this as INativeRoot).GetOrCreateWrapperForObject(proxyObject) as T;
    }
}
