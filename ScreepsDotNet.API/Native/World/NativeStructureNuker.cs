using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureNuker : NativeOwnedStructure, IStructureNuker
    {
        #region Imports

        [JSImport("StructureNuker.launchNuke", "game/prototypes/wrapped")]
        
        internal static partial int Native_LaunchNuke(JSObject proxyObject, JSObject pos);

        #endregion

        private int? cooldownCache;
        private NativeStore? storeCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32(Names.Cooldown);

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject(Names.Store));

        public NativeStructureNuker(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            storeCache?.Dispose();
            storeCache = null;
        }

        public override string ToString()
            => $"StructureNuker[{Id}]";

        public NukerLaunchNukeResult LaunchNuke(RoomPosition pos)
        {
            using var roomPos = pos.ToJS();
            return (NukerLaunchNukeResult)Native_LaunchNuke(ProxyObject, roomPos);
        }
    }
}
