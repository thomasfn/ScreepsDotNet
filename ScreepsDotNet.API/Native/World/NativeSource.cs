using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeSource : NativeRoomObject, ISource, IEquatable<NativeSource?>
    {
        private readonly string id;

        public int Energy => ProxyObject.GetPropertyAsInt32("energy");

        public int EnergyCapacity => ProxyObject.GetPropertyAsInt32("energyCapacity");

        public string Id => id;

        public int TicksToRegeneration => ProxyObject.GetPropertyAsInt32("ticksToRegeneration");

        public NativeSource(INativeRoot nativeRoot, JSObject proxyObject, string knownId)
            : base(nativeRoot, proxyObject)
        {
            id = knownId;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        public override string ToString()
            => $"Source[{RoomPosition}]";

        public override bool Equals(object? obj) => Equals(obj as NativeSource);

        public bool Equals(NativeSource? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeSource? left, NativeSource? right) => EqualityComparer<NativeSource>.Default.Equals(left, right);

        public static bool operator !=(NativeSource? left, NativeSource? right) => !(left == right);
    }
}
