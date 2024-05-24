using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructurePowerSpawn : NativeOwnedStructureWithStore, IStructurePowerSpawn
    {
        #region Imports

        [JSImport("StructurePowerSpawn.processPower", "game/prototypes/wrapped")]
        internal static partial int Native_ProcessPower(JSObject proxyObject);

        #endregion

        public NativeStructurePowerSpawn(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public PowerSpawnProcessPowerResult ProcessPower()
            => (PowerSpawnProcessPowerResult)Native_ProcessPower(ProxyObject);

        public override string ToString()
            => $"StructurePowerSpawn[{Id}]";
    }
}
