using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeOwnedStructure : NativeStructure, IOwnedStructure
    {
        public bool My => ProxyObject.GetPropertyAsBoolean("my");

        public OwnerInfo Owner => new(ProxyObject.GetPropertyAsJSObject("owner")!.GetPropertyAsString("username")!);

        public NativeOwnedStructure(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }
    }
}
