using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureWall : NativeStructure, IStructureWall
    {
        public NativeStructureWall(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"StructureWall({Id}, {Position})";
    }
}
