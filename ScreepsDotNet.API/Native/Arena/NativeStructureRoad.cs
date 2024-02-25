using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureRoad : NativeStructure, IStructureRoad
    {
        public NativeStructureRoad(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public override string ToString()
            => Exists ? $"StructureRoad({Id}, {Position})" : "StructureRoad(DEAD)";
    }
}
