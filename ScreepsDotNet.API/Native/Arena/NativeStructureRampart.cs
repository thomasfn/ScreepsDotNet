using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureRampart : NativeOwnedStructure, IStructureRampart
    {
        public NativeStructureRampart(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"StructureRampart({Id}, {Position})";
    }
}
