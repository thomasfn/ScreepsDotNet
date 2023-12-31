﻿using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructurePowerBank : NativeOwnedStructure, IStructurePowerBank
    {
        private int? powerCache;
        private int? ticksToDecayCache;

        public int Power => CacheLifetime(ref powerCache) ??= ProxyObject.GetPropertyAsInt32("level");

        public int TicksToDecay => CacheLifetime(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32("level");

        public NativeStructurePowerBank(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            powerCache = null;
            ticksToDecayCache = null;
        }

        public override string ToString()
            => $"StructurePowerBank[{Id}]";
    }
}
