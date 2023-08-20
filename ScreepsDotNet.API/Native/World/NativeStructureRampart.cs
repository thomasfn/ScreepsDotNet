using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureRampart : NativeOwnedStructure, IStructureRampart
    {
        #region Imports

        [JSImport("StructureRampart.setPublic", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_SetPublic([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Boolean>] bool isPublic);

        #endregion

        public bool IsPublic => ProxyObject.GetPropertyAsBoolean("isPublic");

        public int TicksToDecay => ProxyObject.GetPropertyAsInt32("ticksToDecay");

        public NativeStructureRampart(INativeRoot nativeRoot, JSObject proxyObject, string knownId) : base(nativeRoot, proxyObject, knownId)
        { }

        public NativeStructureRampart(INativeRoot nativeRoot, string id, RoomPosition? roomPos)
            : base(nativeRoot, id, roomPos)
        { }

        public RampartSetPublicResult SetPublic(bool isPublic)
            => (RampartSetPublicResult)Native_SetPublic(ProxyObject, isPublic);
    }
}
