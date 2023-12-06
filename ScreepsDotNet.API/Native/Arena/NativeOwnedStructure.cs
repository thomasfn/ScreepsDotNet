using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeOwnedStructure : NativeStructure, IOwnedStructure
    {
        public bool? My => ProxyObject.GetTypeOfProperty("my") == JSPropertyType.Boolean ? ProxyObject.GetPropertyAsBoolean("my") : null;

        public NativeOwnedStructure(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"OwnedStructure({Id}, {Position})";
    }
}
