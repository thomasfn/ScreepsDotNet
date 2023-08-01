using System;
using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeOwnedStructure : NativeStructure, IOwnedStructure
    {
        public bool? My => ProxyObject.GetTypeOfProperty("my") == "boolean" ? ProxyObject.GetPropertyAsBoolean("my") : null;

        public NativeOwnedStructure(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"OwnedStructure({Id}, {Position})";
    }
}
