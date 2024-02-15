using System;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureContainer : NativeOwnedStructure, IStructureContainer
    {
        private NativeStore? storeCache;
        private int? ticksToDecayCache;

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject(Names.Store));

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);

        public NativeStructureContainer(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache?.Dispose();
            storeCache = null;
            ticksToDecayCache = null;
        }

        public override string ToString()
            => $"StructureContainer[{Id}]";
    }
}
