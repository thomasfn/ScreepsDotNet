using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeSource : NativeRoomObject, ISource
    {
        private readonly string id;

        public int Energy => ProxyObject.GetPropertyAsInt32("energy");

        public int EnergyCapacity => ProxyObject.GetPropertyAsInt32("energyCapacity");

        public string Id => id;

        public int TicksToRegeneration => ProxyObject.GetPropertyAsInt32("ticksToRegeneration");

        public NativeSource(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
            id = proxyObject.GetPropertyAsString("id")!;
        }

        public override void InvalidateProxyObject()
        {
            proxyObjectOrNull = nativeRoot.GetObjectById(id);
            ClearNativeCache();
        }
    }
}
