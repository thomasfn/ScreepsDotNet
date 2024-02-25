using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureRampart : NativeOwnedStructure, IStructureRampart
    {
        public NativeStructureRampart(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public override string ToString()
            => Exists ? $"StructureRampart({Id}, {Position})" : "StructureRampart(DEAD)";
    }
}
