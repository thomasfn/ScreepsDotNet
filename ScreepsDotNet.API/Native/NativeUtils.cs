using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Linq;

using ScreepsDotNet.API;
using System;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeUtils : IUtils
    {
        #region Imports

        [JSImport("getCpuTime", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial long Native_GetCpuTime();

        [JSImport("getObjects", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_GetObjects();

        [JSImport("getObjectsByPrototype", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_GetObjectsByPrototype([JSMarshalAs<JSType.Object>] JSObject prototype);

        [JSImport("getObjects", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetRange([JSMarshalAs<JSType.Object>] JSObject a, [JSMarshalAs<JSType.Object>] JSObject b);

        [JSImport("getTicks", "game/utils")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetTicks();

        #endregion

        public long GetCpuTime()
            => Native_GetCpuTime();

        public IEnumerable<IGameObject> GetObjects()
            => Native_GetObjects()
                .Select(NativeGameObjectUtils.CreateWrapperForObject)
                .ToArray();

        public IEnumerable<T> GetObjectsByType<T>() where T : class, IGameObject
            => Native_GetObjectsByPrototype(NativeGameObjectPrototypes<T>.ConstructorObj!)
                .Select(NativeGameObjectUtils.CreateWrapperForObject)
                .Cast<T>()
                .ToArray();

        public int GetRange(IPosition a, IPosition b)
            => Native_GetRange(a.ToJS(), b.ToJS());


        public int GetTicks()
            => Native_GetTicks();
    }
}
