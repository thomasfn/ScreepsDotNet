using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureRoad : NativeStructure, IStructureRoad
    {
        private int? ticksToDecayCache;

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);

        public NativeStructureRoad(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            ticksToDecayCache = null;
        }

        public override string ToString()
            => $"StructureRoad[{(Exists ? RoomPosition.ToString() : "DEAD")}]";
    }
}
