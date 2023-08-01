using System;
using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeFlag : NativeGameObject, IFlag
    {
        public NativeFlag(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"Flag({Id}, {Position})";
    }
}
