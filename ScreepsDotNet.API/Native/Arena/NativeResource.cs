using ScreepsDotNet.Interop;
using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeResource : NativeGameObject, IResource
    {
        public int Amount => ProxyObject.GetPropertyAsInt32("amount");

        public ResourceType ResourceType => ProxyObject.GetPropertyAsString("resourceType")!.ParseResourceType();

        public NativeResource(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"Resource({Id}, {Position})";
    }
}
