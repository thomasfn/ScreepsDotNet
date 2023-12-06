using System;
using System.Collections.Generic;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeFlag : NativeRoomObject, IFlag, IEquatable<NativeFlag?>
    {
        #region Imports

        [JSImport("Flag.remove", "game/prototypes/wrapped")]
        
        internal static partial int Native_Remove(JSObject proxyObject);

        [JSImport("Flag.setColor", "game/prototypes/wrapped")]
        
        internal static partial int Native_SetColor(JSObject proxyObject, int color, int? secondaryColor);

        [JSImport("Flag.setPosition", "game/prototypes/wrapped")]
        
        internal static partial int Native_SetPosition(JSObject proxyObject, int x, int y);

        [JSImport("Flag.setPosition", "game/prototypes/wrapped")]
        
        internal static partial int Native_SetPosition(JSObject proxyObject, JSObject pos);

        #endregion

        private readonly string name;

        protected override bool CanMove => true;

        private IMemoryObject? memoryCache;

        public FlagColor Color => (FlagColor)ProxyObject.GetPropertyAsInt32("color");

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject("memory")!);

        public string Name => name;

        public FlagColor SecondaryColor => (FlagColor)ProxyObject.GetPropertyAsInt32("secondaryColor");

        public NativeFlag(INativeRoot nativeRoot, JSObject? proxyObject)
            : base(nativeRoot, proxyObject)
        {
            name = proxyObject?.GetPropertyAsString("name") ?? string.Empty;
        }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.FlagsObj.GetPropertyAsJSObject(name);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            memoryCache = null;
        }

        public void Remove()
            => Native_Remove(ProxyObject);

        public FlagSetColorResult SetColor(FlagColor color, FlagColor? secondaryColor = null)
            => (FlagSetColorResult)Native_SetColor(ProxyObject, (int)color, (int?)secondaryColor);

        public FlagSetPositionResult SetPosition(Position position)
            => (FlagSetPositionResult)Native_SetPosition(ProxyObject, position.X, position.Y);

        public FlagSetPositionResult SetPosition(RoomPosition position)
        {
            using var posJs = position.ToJS();
            return (FlagSetPositionResult)Native_SetPosition(ProxyObject, posJs);
        }

        public override bool Equals(object? obj) => Equals(obj as NativeFlag);

        public bool Equals(NativeFlag? other) => other is not null && name == other.name;

        public override int GetHashCode() => HashCode.Combine(name);

        public static bool operator ==(NativeFlag? left, NativeFlag? right) => EqualityComparer<NativeFlag>.Default.Equals(left, right);

        public static bool operator !=(NativeFlag? left, NativeFlag? right) => !(left == right);
    }
}
