using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeInterShardMemory : IInterShardMemory
    {
        #region Imports

        [JSImport("interShardMemory.getLocal", "game")]
        
        internal static partial string? Native_GetLocal();

        [JSImport("interShardMemory.getRemote", "game")]
        
        internal static partial string? Native_GetRemote(string shard);

        [JSImport("interShardMemory.setLocal", "game")]
        internal static partial void Native_SetLocal(string value);

        #endregion

        public string? GetLocal()
            => Native_GetLocal();

        public string? GetRemote(string shard)
            => Native_GetRemote(shard);

        public void SetLocal(string value)
            => Native_SetLocal(value);
    }
}
