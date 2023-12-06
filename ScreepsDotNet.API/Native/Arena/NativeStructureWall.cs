using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureWall : NativeStructure, IStructureWall
    {
        public NativeStructureWall(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"StructureWall({Id}, {Position})";
    }
}
