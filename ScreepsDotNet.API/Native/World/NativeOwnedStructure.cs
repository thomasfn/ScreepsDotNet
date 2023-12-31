﻿using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
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
                    return CachePerTick(ref myCache) ??= ProxyObject.GetPropertyAsBoolean("my");
                }
                else
                {
                    return CacheLifetime(ref myCache) ??= ProxyObject.GetPropertyAsBoolean("my");
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

        public NativeOwnedStructure(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : this(nativeRoot, proxyObject, id, false)
        { }

        public NativeOwnedStructure(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id, bool ownershipCanChange)
            : base(nativeRoot, proxyObject, id)
        {
            this.ownershipCanChange = ownershipCanChange;
        }

        public override void UpdateFromDataPacket(RoomObjectDataPacket dataPacket)
        {
            base.UpdateFromDataPacket(dataPacket);
            myCache = dataPacket.My;
        }

        private OwnerInfo? GetOwnerInfo()
        {
            using var ownerObj = ProxyObject.GetPropertyAsJSObject("owner");
            if (ownerObj == null) { return null; }
            return new(ownerObj.GetPropertyAsString("username")!);
        }
    }
}
