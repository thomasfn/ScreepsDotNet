using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureRampart : NativeOwnedStructure, IStructureRampart
    {
        #region Imports

        [JSImport("StructureRampart.setPublic", "game/prototypes/wrapped")]
        
        internal static partial int Native_SetPublic(JSObject proxyObject, bool isPublic);

        #endregion

        public bool IsPublic => ProxyObject.GetPropertyAsBoolean("isPublic");

        public int TicksToDecay => ProxyObject.GetPropertyAsInt32("ticksToDecay");

        public NativeStructureRampart(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        public RampartSetPublicResult SetPublic(bool isPublic)
            => (RampartSetPublicResult)Native_SetPublic(ProxyObject, isPublic);
    }
}
