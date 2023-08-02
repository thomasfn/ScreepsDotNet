using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeSource : NativeGameObject, ISource
    {
        public int Energy => ProxyObject.GetPropertyAsInt32("energy");

        public int EnergyCapacity => ProxyObject.GetPropertyAsInt32("energyCapacity");

        public NativeSource(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"Source({Id}, {Position})";
    }
}
