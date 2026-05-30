using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeScore : NativeRoomObjectWithId, IScore
    {
        private int? scoreCache;
        private int? ticksToDecayCache;

        public int Score => CacheLifetime(ref scoreCache) ??= ProxyObject.GetPropertyAsInt32(Names.Score);

        public int TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);

        public NativeScore(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            ticksToDecayCache = null;
        }

        public override string ToString()
            => $"Score[{(Exists ? RoomPosition.ToString() : "DEAD")}]";
    }
}
