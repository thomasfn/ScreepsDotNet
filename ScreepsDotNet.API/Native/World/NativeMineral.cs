using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeMineral : NativeRoomObject, IMineral, IEquatable<NativeMineral?>
    {
        private readonly string id;

        public int Density => ProxyObject.GetPropertyAsInt32("density");

        public int MineralAmount => ProxyObject.GetPropertyAsInt32("mineralAmount");

        public ResourceType MineralType => ProxyObject.GetPropertyAsString("mineralType")!.ParseResourceType();

        public string Id => id;

        public int TicksToRegeneration => ProxyObject.GetPropertyAsInt32("ticksToRegeneration");

        public NativeMineral(INativeRoot nativeRoot, JSObject proxyObject, string knownId)
            : base(nativeRoot, proxyObject)
        {
            id = knownId;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        public override string ToString()
            => $"Mineral[{RoomPosition}]";

        public override bool Equals(object? obj) => Equals(obj as NativeMineral);

        public bool Equals(NativeMineral? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeMineral? left, NativeMineral? right) => EqualityComparer<NativeMineral>.Default.Equals(left, right);

        public static bool operator !=(NativeMineral? left, NativeMineral? right) => !(left == right);
    }
}
