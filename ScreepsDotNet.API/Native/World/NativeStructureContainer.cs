using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureContainer : NativeOwnedStructure, IStructureContainer
    {
        private NativeStore? storeCache;

        public IStore Store => storeCache ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public int TicksToDecay => ProxyObject.GetPropertyAsInt32("ticksToDecay");

        public NativeStructureContainer(INativeRoot nativeRoot, JSObject proxyObject, string knownId) : base(nativeRoot, proxyObject, knownId)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache = null;
        }

        public override string ToString()
            => $"StructureContainer[{Id}]";
    }
}
