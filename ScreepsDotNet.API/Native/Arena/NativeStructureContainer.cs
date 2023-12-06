using ScreepsDotNet.Interop;
using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureContainer : NativeOwnedStructure, IStructureContainer
    {
        public IStore Store => new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureContainer(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"StructureContainer({Id}, {Position})";
    }
}
