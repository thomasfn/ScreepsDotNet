using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructurePortal : NativeOwnedStructure, IStructurePortal
    {
        private RoomPosition? interRoomDestinationCache;
        private PortalInterShardDestination? interShardDestinationCache;
        private int? ticksToDecayCache;

        public RoomPosition? InterRoomDestination => CacheLifetime(ref interRoomDestinationCache) ??= GetInterRoomDestination();

        public PortalInterShardDestination? InterShardDestination => CacheLifetime(ref interShardDestinationCache) ??= GetInterShardDestination();

        public int? TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetTypeOfProperty("ticksToDecay") == "number" ? ProxyObject.GetPropertyAsInt32("ticksToDecay") : null;

        public NativeStructurePortal(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        private RoomPosition? GetInterRoomDestination()
        {
            using var obj = ProxyObject.GetPropertyAsJSObject("destination");
            if (obj == null) { return null; }
            if (!obj.HasProperty("x")) { return null; }
            return obj.ToRoomPosition();
        }

        private PortalInterShardDestination? GetInterShardDestination()
        {
            using var obj = ProxyObject.GetPropertyAsJSObject("destination");
            if (obj == null) { return null; }
            var shard = obj.GetPropertyAsString("shard");
            if (string.IsNullOrEmpty(shard)) { return null; }
            var roomCoord = new RoomCoord(obj.GetPropertyAsString("room")!);
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
