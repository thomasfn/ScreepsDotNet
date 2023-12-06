using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class JSUtils
    {
        #region Imports

        [JSImport("getConstructorOf", "object")]
        internal static partial JSObject GetConstructorOf(JSObject obj);

        [JSImport("setProperty", "__object")]
        internal static partial void SetObjectArrayOnObject(JSObject obj, string key, JSObject[] val);

        [JSImport("getProperty", "__object")]
        internal static partial JSObject[]? GetObjectArrayOnObject(JSObject obj, string key);

        [JSImport("setProperty", "__object")]
        internal static partial void SetIntArrayOnObject(JSObject obj, string key, int[] val);

        [JSImport("getProperty", "__object")]
        internal static partial int[]? GetIntArrayOnObject(JSObject obj, string key);

        [JSImport("setProperty", "__object")]
        internal static partial void SetStringArrayOnObject(JSObject obj, string key, string[] val);

        [JSImport("getProperty", "__object")]
        internal static partial string[]? GetStringArrayOnObject(JSObject obj, string key);

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
            var obj = JSObject.Create();
            obj.SetProperty("x", pos.X);
            obj.SetProperty("y", pos.Y);
            return obj;
        }

        public static JSObject ToJS(this FractionalPosition pos)
        {
            var obj = JSObject.Create();
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
