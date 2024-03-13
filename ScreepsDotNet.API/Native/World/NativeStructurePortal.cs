using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructurePortal : NativeOwnedStructure, IStructurePortal
    {
        private RoomPosition? interRoomDestinationCache;
        private PortalInterShardDestination? interShardDestinationCache;
        private int? ticksToDecayCache;

        public RoomPosition? InterRoomDestination => CacheLifetime(ref interRoomDestinationCache) ??= GetInterRoomDestination();

        public PortalInterShardDestination? InterShardDestination => CacheLifetime(ref interShardDestinationCache) ??= GetInterShardDestination();

        public int? TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.TryGetPropertyAsInt32(Names.TicksToDecay);

        public NativeStructurePortal(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        private RoomPosition? GetInterRoomDestination()
        {
            using var obj = ProxyObject.GetPropertyAsJSObject(Names.Destination);
            if (obj == null) { return null; }
            if (!obj.HasProperty("x")) { return null; }
            return obj.ToRoomPosition();
        }

        private PortalInterShardDestination? GetInterShardDestination()
        {
            using var obj = ProxyObject.GetPropertyAsJSObject(Names.Destination);
            if (obj == null) { return null; }
            var shard = obj.GetPropertyAsString(Names.Shard);
            if (string.IsNullOrEmpty(shard)) { return null; }
            var roomCoord = new RoomCoord(obj.GetPropertyAsString(Names.Room)!);
            return new(shard, roomCoord);
        }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            ticksToDecayCache = null;
        }

        public override string ToString()
            => $"StructurePortal[{Id}]";
    }
}
