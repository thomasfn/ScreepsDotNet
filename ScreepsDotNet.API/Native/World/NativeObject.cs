using System;
using System.Runtime.CompilerServices;

using ScreepsDotNet.API.World;
using ScreepsDotNet.Interop;

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

        public NativeObject(INativeRoot nativeRoot, JSObject proxyObject)
        {
            this.nativeRoot = nativeRoot;
            this.proxyObject = proxyObject;
            proxyObjectValidAsOf = nativeRoot.TickIndex;
        }

        public void RenewProxyObject()
        {
            isDead = false;
            OnRenewProxyObject();
            proxyObjectValidAsOf = nativeRoot.TickIndex;
            ClearNativeCache();
        }

        public void ReplaceProxyObject(JSObject? newProxyObject)
        {
            proxyObjectValidAsOf = nativeRoot.TickIndex;
            ClearNativeCache();
            if (newProxyObject == proxyObject) { return; }
            if (proxyObject != null)
            {
                if (newProxyObject == null) { OnLoseProxyObject(proxyObject); }
                proxyObject.UserData = null;
                proxyObject.Dispose();
            }
            proxyObject = newProxyObject;
            if (proxyObject != null && !proxyObject.IsDisposed)
            {
                proxyObject.UserData = this;
                isDead = false;
                OnGetNewProxyObject(proxyObject);
            }
            else
            {
                isDead = true;
            }
        }

        protected virtual void OnLoseProxyObject(JSObject proxyObject) { }

        protected virtual void OnGetNewProxyObject(JSObject newProxyObject) { }

        protected virtual void OnRenewProxyObject() { }

        private void TryRenewOrReacquire()
        {
            if (proxyObject != null && !proxyObject.IsDisposed)
            {
                if (ScreepsDotNet_Native.RenewObject(proxyObject.JSHandle) != 0)
                {
                    ReplaceProxyObject(null);
                }
                else
                {
                    RenewProxyObject();
                }
            }
            else
            {
                ReplaceProxyObject(ReacquireProxyObject());
            }
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
