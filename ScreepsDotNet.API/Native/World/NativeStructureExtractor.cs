﻿using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureExtractor : NativeOwnedStructure, IStructureExtractor
    {
        public int Cooldown => ProxyObject.GetPropertyAsInt32("cooldown");

        public NativeStructureExtractor(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }
    }
}
