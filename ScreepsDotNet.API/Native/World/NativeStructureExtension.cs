using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureExtension : NativeOwnedStructure, IStructureExtension
    {
        private NativeStore? storeCache;

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureExtension(INativeRoot nativeRoot, JSObject proxyObject, string knownId) : base(nativeRoot, proxyObject, knownId)
        { }

        public NativeStructureExtension(INativeRoot nativeRoot, string id, RoomPosition? roomPos)
            : base(nativeRoot, id, roomPos)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache = null;
        }

        public override string ToString()
            => $"StructureExtension[{Id}]";
    }
}
