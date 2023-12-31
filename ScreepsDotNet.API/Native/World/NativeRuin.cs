﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeRuin : NativeRoomObject, IRuin, IEquatable<NativeRuin?>
    {
        private readonly ObjectId id;

        private long? destroyTimeCache;
        private NativeStore? storeCache;
        private IStructure? structureCache;
        private int? ticksToDecayCache;

        public long DestroyTime => CacheLifetime(ref destroyTimeCache) ??= ProxyObject.GetPropertyAsInt32("destroyTime");

        public ObjectId Id => id;

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public IStructure? Structure => CacheLifetime(ref structureCache) ??= nativeRoot.GetOrCreateWrapperObject<IStructure>(ProxyObject.GetPropertyAsJSObject("structure"));

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32("ticksToDecayCache");

        public NativeRuin(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject)
        {
            this.id = id;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache = null;
            ticksToDecayCache = null;
        }

        public override string ToString()
            => $"Ruin[{(Exists ? RoomPosition.ToString() : "DEAD")}]";

        public override bool Equals(object? obj) => Equals(obj as NativeRuin);

        public bool Equals(NativeRuin? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeRuin? left, NativeRuin? right) => EqualityComparer<NativeRuin>.Default.Equals(left, right);

        public static bool operator !=(NativeRuin? left, NativeRuin? right) => !(left == right);
    }
}
