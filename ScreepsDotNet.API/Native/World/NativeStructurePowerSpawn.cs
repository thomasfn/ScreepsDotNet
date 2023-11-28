using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructurePowerSpawn : NativeOwnedStructure, IStructurePowerSpawn
    {
        #region Imports

        [JSImport("StructurePowerSpawn.processPower", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_ProcessPower([JSMarshalAs<JSType.Object>] JSObject proxyObject);

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
