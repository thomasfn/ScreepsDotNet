using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using System.Linq;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeObjectLazyLookup<T> : IReadOnlyDictionary<string, T>, INativeObject where T : class
    {
        #region Imports

        [JSImport("getKeysOf", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.String>>]
        internal static partial string[] Native_GetKeysOf([JSMarshalAs<JSType.Object>] JSObject obj);

        #endregion

        private readonly Func<JSObject> proxyObjectReacquireFn;
        private readonly Func<T, string> getObjectKeyFn;
        private readonly Func<string, JSObject, T?> constructObjectFn;

        private JSObject proxyObject;

        private ISet<string>? keysCache;
        private readonly IDictionary<string, T> mapCache = new Dictionary<string, T>();

        private ISet<string> KeySet => keysCache ??= new HashSet<string>(Native_GetKeysOf(proxyObject));

        public T this[string key] => throw new NotImplementedException();

        public IEnumerable<string> Keys => KeySet;

        public IEnumerable<T> Values => this.Select(pair => pair.Value);

        public int Count => KeySet.Count;

        public NativeObjectLazyLookup(Func<JSObject> proxyObjectReacquireFn, Func<T, string> getObjectKeyFn, Func<string, JSObject, T?> constructObjectFn)
        {
            this.proxyObjectReacquireFn = proxyObjectReacquireFn;
            this.getObjectKeyFn = getObjectKeyFn;
            this.constructObjectFn = constructObjectFn;
            proxyObject = proxyObjectReacquireFn();
        }

        public bool ContainsKey(string key)
            => KeySet.Contains(key);

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
#pragma warning disable CS8604 // Possible null reference argument.
            => KeySet
                .Select(key => new KeyValuePair<string, T>(key, TryGetValue(key, out var value) ? value : null))
                .Where(pair => pair.Value != null)
                .GetEnumerator();
#pragma warning restore CS8604 // Possible null reference argument.

        public void InvalidateProxyObject()
        {
            proxyObject = proxyObjectReacquireFn();
            keysCache = null;
            mapCache.Clear();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
        {
            if (!KeySet.Contains(key))
            {
                value = null;
                return false;
            }
            if (mapCache.TryGetValue(key, out value)) { return true; }
            var obj = proxyObject.GetPropertyAsJSObject(key);
            if (obj == null)
            {
                value = null;
                return false;
            }
            var newObj = constructObjectFn(key, obj);
            if (newObj == null)
            {
                value = null;
                return false;
            }
            mapCache.Add(key, newObj);
            value = newObj;
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
