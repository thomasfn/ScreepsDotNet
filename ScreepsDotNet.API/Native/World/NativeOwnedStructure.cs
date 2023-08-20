using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeOwnedStructure : NativeStructure, IOwnedStructure
    {
        private bool? myCache;
        private OwnerInfo? ownerInfoCache;

        public bool My => CacheLifetime(ref myCache) ??= ProxyObject.GetPropertyAsBoolean("my");

        public OwnerInfo Owner => CacheLifetime(ref ownerInfoCache) ??= new(ProxyObject.GetPropertyAsJSObject("owner")!.GetPropertyAsString("username")!);

        public NativeOwnedStructure(INativeRoot nativeRoot, JSObject proxyObject, string knownId)
            : base(nativeRoot, proxyObject, knownId)
        { }

        public NativeOwnedStructure(INativeRoot nativeRoot, string id, RoomPosition? roomPos)
            : base(nativeRoot, id, roomPos)
        { }
    }
}
