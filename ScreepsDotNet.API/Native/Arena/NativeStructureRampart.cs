using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureRampart : NativeOwnedStructure, IStructureRampart
    {
        public NativeStructureRampart(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"StructureRampart({Id}, {Position})";
    }
}
