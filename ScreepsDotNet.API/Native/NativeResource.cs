using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
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
