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

        internal readonly JSObject ProxyObject;

        public IEnumerable<string> Keys => JSUtils.GetKeysOf(ProxyObject);

        public NativeMemoryObject(JSObject proxyObject)
        {
            this.ProxyObject = proxyObject;
        }

        public bool TryGetInt(string key, out int value)
        {
            if (ProxyObject.GetTypeOfProperty(key) != "number")
            {
                value = default;
                return false;
            }
            value = ProxyObject.GetPropertyAsInt32(key);
            return true;
        }

        public bool TryGetString(string key, out string value)
        {
            if (ProxyObject.GetTypeOfProperty(key) != "string")
            {
                value = string.Empty;
                return false;
            }
            var str = ProxyObject.GetPropertyAsString(key);
            value = str ?? string.Empty;
            return str != null;
        }

        public bool TryGetDouble(string key, out double value)
        {
            if (ProxyObject.GetTypeOfProperty(key) != "number")
            {
                value = default;
                return false;
            }
            value = ProxyObject.GetPropertyAsDouble(key);
            return true;
        }

        public bool TryGetBool(string key, out bool value)
        {
            if (ProxyObject.GetTypeOfProperty(key) != "boolean")
            {
                value = default;
                return false;
            }
            value = ProxyObject.GetPropertyAsBoolean(key);
            return true;
        }

        public bool TryGetObject(string key, [MaybeNullWhen(false)] out IMemoryObject value)
        {
            if (ProxyObject.GetTypeOfProperty(key) != "object")
            {
                value = default;
                return false;
            }
            var obj = ProxyObject.GetPropertyAsJSObject(key);
            if (obj == null)
            {
                value = default;
                return false;
            }
            value = new NativeMemoryObject(obj);
            return true;
        }

        public void SetValue(string key, int value)
            => ProxyObject.SetProperty(key, value);

        public void SetValue(string key, string value)
            => ProxyObject.SetProperty(key, value);

        public void SetValue(string key, double value)
            => ProxyObject.SetProperty(key, value);

        public void SetValue(string key, bool value)
            => ProxyObject.SetProperty(key, value);

        public IMemoryObject GetOrCreateObject(string key)
        {
            var obj = ProxyObject.GetPropertyAsJSObject(key);
            if (obj != null) { return new NativeMemoryObject(obj); }
            obj = JSUtils.CreateObject(null);
            ProxyObject.SetProperty(key, obj);
            return new NativeMemoryObject(obj);
        }

        public void ClearValue(string key)
            => Native_DeleteOnObject(ProxyObject, key);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class NativeMemoryObjectExtensions
    {
        public static JSObject ToJS(this IMemoryObject roomObject)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            => (roomObject as NativeMemoryObject).ProxyObject;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
