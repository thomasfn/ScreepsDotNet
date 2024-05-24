using System;
using System.Collections.Generic;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructure : NativeRoomObjectWithId, IStructure
    {
        #region Imports

        [JSImport("Structure.destroy", "game/prototypes/wrapped")]
        
        internal static partial int Native_Destroy(JSObject proxyObject);

        [JSImport("Structure.isActive", "game/prototypes/wrapped")]
        
        internal static partial bool Native_IsActive(JSObject proxyObject);

        [JSImport("Structure.notifyWhenAttacked", "game/prototypes/wrapped")]
        
        internal static partial int Native_NotifyWhenAttacked(JSObject proxyObject, bool enabled);

        #endregion

        private int? hitsCache;
        private int? hitsMaxCache;

        public int Hits => CachePerTick(ref hitsCache) ??= ProxyObject.TryGetPropertyAsInt32(Names.Hits) ?? 0;

        public int HitsMax => CachePerTick(ref hitsMaxCache) ??= ProxyObject.TryGetPropertyAsInt32(Names.HitsMax) ?? 0;

        public NativeStructure(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            hitsCache = null;
            hitsMaxCache = null;
        }

        public StructureDestroyResult Destroy()
            => (StructureDestroyResult)Native_Destroy(ProxyObject);

        public bool IsActive()
            => Native_IsActive(ProxyObject);

        public StructureNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled)
            => (StructureNotifyWhenAttackedResult)Native_NotifyWhenAttacked(ProxyObject, enabled);

        public override string ToString()
            => $"NativeStructure[{Id}]";
    }
}
