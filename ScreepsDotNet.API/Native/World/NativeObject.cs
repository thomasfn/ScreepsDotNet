using ScreepsDotNet.API.World;
using System;
using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal abstract class NativeObject : INativeObject
    {
        protected readonly INativeRoot nativeRoot;

        private bool isDead = false;
        private int proxyObjectValidAsOf;
        private JSObject? proxyObject;

        public JSObject ProxyObject
        {
            get
            {
                TouchProxyObject();
                if (proxyObject == null) { throw new NativeObjectNoLongerExistsException(); }
                return proxyObject;
            }
        }

        public bool Exists
        {
            get
            {
                if (isDead) { return false; }
                if (proxyObjectValidAsOf < nativeRoot.TickIndex)
                {
                    proxyObject = ReacquireProxyObject();
                    ClearNativeCache();
                    if (proxyObject == null)
                    {
                        isDead = true;
                        return false;
                    }
                    else
                    {
                        proxyObjectValidAsOf = nativeRoot.TickIndex;
                        return true;
                    }
                }
                return proxyObject != null;
            }
        }

        public NativeObject(INativeRoot nativeRoot, JSObject? proxyObject)
        {
            this.nativeRoot = nativeRoot;
            this.proxyObject = proxyObject;
            proxyObjectValidAsOf = proxyObject != null ? nativeRoot.TickIndex : -1;
        }

        protected void TouchProxyObject()
        {
            if (isDead) { throw new NativeObjectNoLongerExistsException(); }
            if (proxyObjectValidAsOf < nativeRoot.TickIndex)
            {
                proxyObject?.Dispose();
                proxyObject = ReacquireProxyObject();
                ClearNativeCache();
                if (proxyObject == null)
                {
                    isDead = true;
                    throw new NativeObjectNoLongerExistsException();
                }
                else
                {
                    proxyObjectValidAsOf = nativeRoot.TickIndex;
                }
            }
        }

        protected ref T CachePerTick<T>(ref T cachedObj)
        {
            TouchProxyObject();
            return ref cachedObj;
        }

        protected ref T CacheLifetime<T>(ref T cachedObj)
        {
            return ref cachedObj;
        }

        protected virtual void ClearNativeCache() { }

        public abstract JSObject? ReacquireProxyObject();

        public override string ToString()
            => $"{GetType().Name}[{(isDead ? "DEAD" : ProxyObject?.ToString())}]";
    }
}
