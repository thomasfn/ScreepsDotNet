using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureRoad : NativeStructure, IStructureRoad
    {
        public NativeStructureRoad(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"StructureRoad({Id}, {Position})";
    }
}
