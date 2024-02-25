using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeFlag : NativeGameObject, IFlag
    {
        public NativeFlag(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, false)
        { }

        public override string ToString()
            => Exists ? $"Flag({Id}, {Position})" : $"Flag(DEAD)";
    }
}
