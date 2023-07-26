using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
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
