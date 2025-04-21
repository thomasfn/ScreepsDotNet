using ScreepsDotNet.API.Arena;
using ScreepsDotNet.Interop;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeArenaInfo : IArenaInfo
    {
        #region Imports

        [JSImport("getArenaInfo", "game")]
        internal static partial JSObject GetArenaInfo();

        #endregion

        private readonly JSObject arenaInfoObj;

        private string? name;
        private ArenaLevel? level;
        private string? season;
        private int? ticksLimit;
        private int? cpuTimeLimit;
        private int? cpuTimeLimitFirstTick;


        public string Name => name ??= arenaInfoObj.GetPropertyAsString("name")!;

        public ArenaLevel Level => level ??= (ArenaLevel)arenaInfoObj.GetPropertyAsInt32("level");

        public string Season => season ??= arenaInfoObj.GetPropertyAsString("season")!;

        public int TicksLimit => ticksLimit ??= arenaInfoObj.GetPropertyAsInt32("ticksLimit");

        public int CpuTimeLimit => cpuTimeLimit ??= arenaInfoObj.GetPropertyAsInt32("cpuTimeLimit");

        public int CpuTimeLimitFirstTick => cpuTimeLimitFirstTick ??= arenaInfoObj.GetPropertyAsInt32("cpuTimeLimitFirstTick");

        public NativeArenaInfo()
        {
            this.arenaInfoObj = GetArenaInfo();
        }
    }
}
