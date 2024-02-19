using ScreepsDotNet.API.World;
using ScreepsDotNet.Interop;
using System;
using System.Runtime.CompilerServices;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal abstract class NativeObject
    {
        protected readonly INativeRoot nativeRoot;

        private bool isDead = false;
        private int proxyObjectValidAsOf;
        private JSObject? proxyObject;

        public JSObject ProxyObject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                TouchProxyObject();
                if (proxyObject == null) { throw new NativeObjectNoLongerExistsException(); }
                return proxyObject;
            }
        }

        public IntPtr? ProxyObjectJSHandle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => proxyObject?.JSHandle;
        }

        public bool Stale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => proxyObjectValidAsOf < nativeRoot.TickIndex;
        }

        public bool Exists
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (Stale)
                {
                    TryRenewOrReacquire();
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

        public void NotifyBatchRenew(bool failed)
        {
            proxyObjectValidAsOf = nativeRoot.TickIndex;
            ClearNativeCache();
            if (failed)
            {
                proxyObject?.Dispose();
                proxyObject = null;
                isDead = true;
            }
        }

        public void ReplaceProxyObject(JSObject? newProxyObject)
        {
            if (newProxyObject == proxyObject) { return; }
            proxyObject?.Dispose();
            proxyObject = newProxyObject;
            proxyObjectValidAsOf = nativeRoot.TickIndex;
            ClearNativeCache();
            isDead = proxyObject == null || proxyObject.IsDisposed;
        }

        private void TryRenewOrReacquire()
        {
            if (proxyObject != null && !proxyObject.IsDisposed)
            {
                if (ScreepsDotNet_Native.RenewObject(proxyObject.JSHandle) != 0)
                {
                    proxyObject.Dispose();
                    proxyObject = null;
                }
            }
            else
            {
                proxyObject = ReacquireProxyObject();
            }
            proxyObjectValidAsOf = nativeRoot.TickIndex;
            ClearNativeCache();
            isDead = proxyObject == null || proxyObject.IsDisposed;
        }

        protected void TouchProxyObject()
        {
            if (Stale)
            {
                TryRenewOrReacquire();
            }
            if (isDead)
            {
                throw new NativeObjectNoLongerExistsException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ref T CachePerTick<T>(ref T cachedObj)
        {
            TouchProxyObject();
            return ref cachedObj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
