using ScreepsDotNet.API.World;
using System;
using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal abstract class NativeObject : INativeObject
    {
        protected readonly INativeRoot nativeRoot;

        protected JSObject? proxyObjectOrNull;

        public JSObject ProxyObject
        {
            get
            {
                if (proxyObjectOrNull == null) { throw new NativeObjectNoLongerExistsException(); }
                return proxyObjectOrNull;
            }
            set
            {
                proxyObjectOrNull = value;
                ClearNativeCache();
            }
        }

        public NativeObject(INativeRoot nativeRoot, JSObject proxyObject)
        {
            this.nativeRoot = nativeRoot;
            proxyObjectOrNull = proxyObject;
            nativeRoot.BeginTracking(this);
        }

        protected virtual void ClearNativeCache() { }

        public abstract void InvalidateProxyObject();

        public override string ToString()
            => $"NativeObject[{proxyObjectOrNull}]";
    }
}
