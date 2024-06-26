﻿using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructurePowerBank : NativeOwnedStructure, IStructurePowerBank
    {
        private int? powerCache;
        private int? ticksToDecayCache;

        public int Power => CacheLifetime(ref powerCache) ??= ProxyObject.GetPropertyAsInt32(Names.Power);

        public int TicksToDecay => CacheLifetime(ref ticksToDecayCache) ??= ProxyObject.GetPropertyAsInt32(Names.TicksToDecay);

        public NativeStructurePowerBank(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
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
