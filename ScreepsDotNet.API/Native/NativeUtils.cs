using ScreepsDotNet.API;
using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet.Native
{
    internal partial class NativeUtils : IUtils
    {
        #region Imports

        [JSImport("getCpuTime", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial long Native_GetCpuTime();

        [JSImport("getTicks", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetTicks();

        #endregion

        public long GetCpuTime()
            => Native_GetCpuTime();


        public int GetTicks()
            => Native_GetTicks();
    }
}
