using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeFlag : NativeGameObject, IFlag
    {
        private bool? myCache;

        public bool? My => CacheLifetime(ref myCache) ??= proxyObject.TryGetPropertyAsBoolean(Names.My);

        public NativeFlag(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, false)
        { }

        public override string ToString()
            => Exists ? $"Flag({Id}, {Position})" : $"Flag(DEAD)";
    }
}
