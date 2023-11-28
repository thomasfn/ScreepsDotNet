using System.Runtime.InteropServices.JavaScript;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeInterShardMemory : IInterShardMemory
    {
        #region Imports

        [JSImport("interShardMemory.getLocal", "game")]
        [return: JSMarshalAsAttribute<JSType.String>]
        internal static partial string? Native_GetLocal();

        [JSImport("interShardMemory.getRemote", "game")]
        [return: JSMarshalAsAttribute<JSType.String>]
        internal static partial string? Native_GetRemote([JSMarshalAs<JSType.String>] string shard);

        [JSImport("interShardMemory.setLocal", "game")]
        internal static partial void Native_SetLocal([JSMarshalAs<JSType.String>] string value);

        #endregion

        public string? GetLocal()
            => Native_GetLocal();

        public string? GetRemote(string shard)
            => Native_GetRemote(shard);

        public void SetLocal(string value)
            => Native_SetLocal(value);
    }
}
