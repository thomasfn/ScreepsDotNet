using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeFlag : NativeGameObject, IFlag
    {
        public NativeFlag(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"Flag({Id}, {Position})";
    }
}
