using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureContainer : NativeOwnedStructure, IStructureContainer
    {
        public IStore Store { get; }

        public NativeStructureContainer(JSObject proxyObject)
            : base(proxyObject)
        {
            Store = new NativeStore(proxyObject.GetPropertyAsJSObject("store")!);
        }

        public override string ToString()
            => $"StructureContainer({Id}, {Position})";
    }
}
