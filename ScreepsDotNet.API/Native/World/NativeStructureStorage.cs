using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureStorage : NativeOwnedStructureWithStore, IStructureStorage
    {
        public NativeStructureStorage(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public override string ToString()
            => $"StructureStorage[{Id}]";

    }
}
