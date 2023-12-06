using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructurePowerSpawn : NativeOwnedStructure, IStructurePowerSpawn
    {
        #region Imports

        [JSImport("StructurePowerSpawn.processPower", "game/prototypes/wrapped")]
        
        internal static partial int Native_ProcessPower(JSObject proxyObject);

        #endregion

        private NativeStore? storeCache;

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructurePowerSpawn(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache = null;
        }

        public PowerSpawnProcessPowerResult ProcessPower()
            => (PowerSpawnProcessPowerResult)Native_ProcessPower(ProxyObject);

        public override string ToString()
            => $"StructurePowerSpawn[{Id}]";
    }
}
