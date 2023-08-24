using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeResource : NativeRoomObject, IResource, IEquatable<NativeResource?>
    {
        private readonly ObjectId id;

        private int? amountCache;
        private ResourceType? resourceTypeCache;

        public int Amount => CachePerTick(ref amountCache) ??= ProxyObject.GetPropertyAsInt32("amount");

        public ObjectId Id => id;

        public ResourceType ResourceType => CacheLifetime(ref resourceTypeCache) ??= ProxyObject.GetPropertyAsString("resourceType")!.ParseResourceType();

        public NativeResource(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            amountCache = null;
        }

        public override string ToString()
            => $"Resource[{(Exists ? RoomPosition.ToString() : "DEAD")}]";

        public override bool Equals(object? obj) => Equals(obj as NativeResource);

        public bool Equals(NativeResource? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeResource? left, NativeResource? right) => EqualityComparer<NativeResource>.Default.Equals(left, right);

        public static bool operator !=(NativeResource? left, NativeResource? right) => !(left == right);
    }
}
