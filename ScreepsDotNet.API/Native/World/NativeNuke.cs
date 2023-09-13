using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeNuke : NativeRoomObject, INuke, IEquatable<NativeNuke?>
    {
        private readonly ObjectId id;

        private RoomCoord? launchRoomCoordCache;
        private int? timeToLandCache;

        public ObjectId Id => id;

        public RoomCoord LaunchRoomCoord => CacheLifetime(ref launchRoomCoordCache) ??= new(ProxyObject.GetPropertyAsString("launchRoomName")!);

        public int TimeToLand => CachePerTick(ref timeToLandCache) ??= ProxyObject.GetPropertyAsInt32("timeToLand");

        public NativeNuke(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            timeToLandCache = null;
        }

        public override string ToString()
            => $"Nuke[{(Exists ? RoomPosition.ToString() : "DEAD")}]";

        public override bool Equals(object? obj) => Equals(obj as NativeNuke);

        public bool Equals(NativeNuke? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeNuke? left, NativeNuke? right) => EqualityComparer<NativeNuke>.Default.Equals(left, right);

        public static bool operator !=(NativeNuke? left, NativeNuke? right) => !(left == right);
    }
}
