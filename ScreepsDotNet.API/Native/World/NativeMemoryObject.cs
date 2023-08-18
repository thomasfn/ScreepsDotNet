using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeMemoryObject : IMemoryObject
    {
        #region Imports

        [JSImport("deleteOnObject", "object")]
        internal static partial void Native_DeleteOnObject([JSMarshalAs<JSType.Object>] JSObject target, [JSMarshalAs<JSType.String>] string key);

        #endregion

        private readonly JSObject proxyObject;

        public IEnumerable<string> Keys => JSUtils.GetKeysOf(proxyObject);

        public NativeMemoryObject(JSObject proxyObject)
        {
            this.proxyObject = proxyObject;
        }

        public bool TryGetInt(string key, out int value)
        {
            if (proxyObject.GetTypeOfProperty(key) != "number")
            {
                value = default;
                return false;
            }
            value = proxyObject.GetPropertyAsInt32(key);
            return true;
        }

        public bool TryGetString(string key, out string value)
        {
            if (proxyObject.GetTypeOfProperty(key) != "string")
            {
                value = string.Empty;
                return false;
            }
            var str = proxyObject.GetPropertyAsString(key);
            value = str ?? string.Empty;
            return str != null;
        }

        public bool TryGetDouble(string key, out double value)
        {
            if (proxyObject.GetTypeOfProperty(key) != "number")
            {
                value = default;
                return false;
            }
            value = proxyObject.GetPropertyAsDouble(key);
            return true;
        }

        public bool TryGetBool(string key, out bool value)
        {
            if (proxyObject.GetTypeOfProperty(key) != "boolean")
            {
                value = default;
                return false;
            }
            value = proxyObject.GetPropertyAsBoolean(key);
            return true;
        }

        public bool TryGetObject(string key, [MaybeNullWhen(false)] out IMemoryObject value)
        {
            if (proxyObject.GetTypeOfProperty(key) != "object")
            {
                value = default;
                return false;
            }
            var obj = proxyObject.GetPropertyAsJSObject(key);
            if (obj == null)
            {
                value = default;
                return false;
            }
            value = new NativeMemoryObject(obj);
            return true;
        }

        public void SetValue(string key, int value)
            => proxyObject.SetProperty(key, value);

        public void SetValue(string key, string value)
            => proxyObject.SetProperty(key, value);

        public void SetValue(string key, double value)
            => proxyObject.SetProperty(key, value);

        public void SetValue(string key, bool value)
            => proxyObject.SetProperty(key, value);

        public IMemoryObject GetOrCreateObject(string key)
        {
            var obj = proxyObject.GetPropertyAsJSObject(key);
            if (obj != null) { return new NativeMemoryObject(obj); }
            obj = JSUtils.CreateObject(null);
            proxyObject.SetProperty(key, obj);
            return new NativeMemoryObject(obj);
        }

        public void ClearValue(string key)
            => Native_DeleteOnObject(proxyObject, key);
    }
}
