using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureRoad : NativeStructure, IStructureRoad
    {
        public NativeStructureRoad(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"StructureRoad({Id}, {Position})";
    }
}
