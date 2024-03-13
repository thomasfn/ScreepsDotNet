using System;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeOwnedStructure : NativeStructure, IOwnedStructure
    {
        private readonly bool ownershipCanChange;

        private bool? myCache;
        private OwnerInfo? ownerInfoCache;

        public bool My
        {
            get
            {
                if (ownershipCanChange)
                {
                    return CachePerTick(ref myCache) ??= (ProxyObject.TryGetPropertyAsBoolean(Names.My) ?? false);
                }
                else
                {
                    return CacheLifetime(ref myCache) ??= (ProxyObject.TryGetPropertyAsBoolean(Names.My) ?? false);
                }
            }
        }

        public OwnerInfo? Owner
        {
            get
            {
                if (ownershipCanChange)
                {
                    return CachePerTick(ref ownerInfoCache) ??= GetOwnerInfo();
                }
                else
                {
                    return CacheLifetime(ref ownerInfoCache) ??= GetOwnerInfo();
                }
            }
        }

        public NativeOwnedStructure(INativeRoot nativeRoot, JSObject proxyObject)
            : this(nativeRoot, proxyObject, false)
        { }

        public NativeOwnedStructure(INativeRoot nativeRoot, JSObject proxyObject, bool ownershipCanChange)
            : base(nativeRoot, proxyObject)
        {
            this.ownershipCanChange = ownershipCanChange;
        }

        private OwnerInfo? GetOwnerInfo()
        {
            using var ownerObj = ProxyObject.GetPropertyAsJSObject(Names.Owner);
            if (ownerObj == null) { return null; }
            return new(ownerObj.GetPropertyAsString(Names.Username)!);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeOwnedStructureWithStore : NativeOwnedStructure, IWithStore
    {
        private readonly NativeStore store;

        public IStore Store => store;

        public NativeOwnedStructureWithStore(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
            store = new NativeStore(nativeRoot, proxyObject);
        }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            store.ClearNativeCache();
        }

        protected override void OnGetNewProxyObject(JSObject newProxyObject)
        {
            base.OnGetNewProxyObject(newProxyObject);
            store.ProxyObject = newProxyObject;
        }

    }
}
