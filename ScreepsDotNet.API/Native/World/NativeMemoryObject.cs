using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal class NativeMemoryObject : IMemoryObject
    {
        private readonly JSObject proxyObject;

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
            if (proxyObject.GetTypeOfProperty(key) != "number")
            {
                value = default;
                return false;
            }
            value = proxyObject.GetPropertyAsBoolean(key);
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
    }
}
