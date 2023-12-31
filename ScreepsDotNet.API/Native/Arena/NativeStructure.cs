﻿using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructure : NativeGameObject, IStructure
    {
        public int Hits => ProxyObject.GetPropertyAsInt32("hits");

        public int HitsMax => ProxyObject.GetPropertyAsInt32("hitsMax");

        public NativeStructure(JSObject proxyObject)
            : base(proxyObject)
        { }

        public override string ToString()
            => $"Structure({Id}, {Position})";
    }
}
