using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static partial class JSUtils
    {
        #region Imports

        [JSImport("getConstructorOf", "object")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetConstructorOf([JSMarshalAs<JSType.Object>] JSObject obj);

        [JSImport("getKeysOf", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.String>>]
        internal static partial string[] GetKeysOf([JSMarshalAs<JSType.Object>] JSObject obj);

        [JSImport("create", "object")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject CreateObject([JSMarshalAs<JSType.Object>] JSObject? prototype);

        [JSImport("set", "object")]
        internal static partial void SetObjectArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] val);

        [JSImport("get", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[]? GetObjectArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key);

        [JSImport("set", "object")]
        internal static partial void SetIntArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key, [JSMarshalAs<JSType.Array<JSType.Number>>] int[] val);

        [JSImport("get", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Number>>]
        internal static partial int[]? GetIntArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key);

        #endregion

        #region Extensions

        internal static void DisposeAll(this IEnumerable<IDisposable> sequence)
        {
            foreach (var item in sequence)
            {
                item.Dispose();
            }
        }

        public static Position ToPosition(this JSObject obj)
           => (obj.GetPropertyAsInt32("x"), obj.GetPropertyAsInt32("y"));

        public static Position? ToPositionNullable(this JSObject? obj)
            => obj != null ? new Position?(obj.ToPosition()) : null;

        public static JSObject ToJS(this Position pos)
        {
            var obj = CreateObject(null);
            obj.SetProperty("x", pos.X);
            obj.SetProperty("y", pos.Y);
            return obj;
        }

        public static JSObject ToJS(this FractionalPosition pos)
        {
            var obj = CreateObject(null);
            obj.SetProperty("x", pos.X);
            obj.SetProperty("y", pos.Y);
            return obj;
        }

        public static JSObject? ToJS(this Position? pos)
            => pos != null ? pos.Value.ToJS() : null;

        public static JSObject? ToJS(this FractionalPosition? pos)
            => pos != null ? pos.Value.ToJS() : null;

        #endregion
    }
}
