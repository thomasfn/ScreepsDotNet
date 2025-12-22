using ScreepsDotNet.Interop;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeBodyPart : NativeGameObject, IBodyPart
    {
        private BodyPartType? typeCache;

        public BodyPartType Type => CacheLifetime(ref typeCache) ??= proxyObject.GetPropertyAsName(Names.Type).ParseBodyPartType();

        public NativeBodyPart(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject, false)
        { }

        public override string ToString()
            => Exists ? $"BodyPart({Id}, {Position})" : $"BodyPart(DEAD)";
    }
}
