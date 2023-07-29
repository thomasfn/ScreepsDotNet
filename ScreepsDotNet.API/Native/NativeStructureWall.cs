using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
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
