using ScreepsDotNet.API.World;
using System;
using ScreepsDotNet.Interop;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
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
                if (proxyObjectValidAsOf < nativeRoot.TickIndex)
                {
                    proxyObject?.Dispose();
                    proxyObject = ReacquireProxyObject();
                    proxyObjectValidAsOf = nativeRoot.TickIndex;
                    if (proxyObject == null)
                    {
                        isDead = true;
                        return false;
                    }
                    else
                    {
                        isDead = false;
                        ClearNativeCache();
                        return true;
                    }
                }
                return !isDead;
            }
        }

        public NativeObject(INativeRoot nativeRoot, JSObject? proxyObject)
        {
            this.nativeRoot = nativeRoot;
            this.proxyObject = proxyObject;
            proxyObjectValidAsOf = proxyObject != null ? nativeRoot.TickIndex : -1;
        }

        public virtual void UpdateFromDataPacket(RoomObjectDataPacket dataPacket)
        {
            if (nativeRoot.TickIndex > proxyObjectValidAsOf)
            {
                proxyObjectValidAsOf = nativeRoot.TickIndex;
                proxyObject?.Dispose();
                proxyObject = null;
                ClearNativeCache();
            }
            isDead = false;
        }

        protected void TouchProxyObject(bool objectNeededNow = true)
        {
            if (proxyObjectValidAsOf < nativeRoot.TickIndex || (proxyObject == null && objectNeededNow))
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
                    isDead = false;
                    proxyObjectValidAsOf = nativeRoot.TickIndex;
                }
            }
            else if (isDead)
            {
                throw new NativeObjectNoLongerExistsException();
            }
        }

        protected ref T CachePerTick<T>(ref T cachedObj)
        {
            TouchProxyObject(false);
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
