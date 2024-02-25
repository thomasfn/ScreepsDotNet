using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeOwnedStructure : NativeStructure, IOwnedStructure
    {
        private bool? myCache;

        public bool? My => CacheLifetime(ref myCache) ??= proxyObject.TryGetPropertyAsBoolean(Names.My);

        public NativeOwnedStructure(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public override string ToString()
            => Exists ? $"OwnedStructure({Id}, {Position})" : "OwnedStructure(DEAD)";
    }
}
