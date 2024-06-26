﻿using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureLab : NativeOwnedStructureWithStore, IStructureLab
    {
        #region Imports

        [JSImport("StructureLab.boostCreep", "game/prototypes/wrapped")]
        
        internal static partial int Native_BoostCreep(JSObject proxyObject, JSObject creep, int? bodyPartsCount);

        [JSImport("StructureLab.reverseReaction", "game/prototypes/wrapped")]
        
        internal static partial int Native_ReverseReaction(JSObject proxyObject, JSObject lab1, JSObject lab2);

        [JSImport("StructureLab.runReaction", "game/prototypes/wrapped")]
        
        internal static partial int Native_RunReaction(JSObject proxyObject, JSObject lab1, JSObject lab2);

        [JSImport("StructureLab.unboostCreep", "game/prototypes/wrapped")]
        
        internal static partial int Native_UnboostCreep(JSObject proxyObject, JSObject creep);

        #endregion

        private int? cooldownCache;
        private ResourceType? mineralTypeCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32(Names.Cooldown);

        public ResourceType? MineralType => CachePerTick(ref mineralTypeCache) ??= ProxyObject.TryGetPropertyAsName(Names.MineralType)?.ParseResourceType();

        public NativeStructureLab(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            mineralTypeCache = null;
        }

        public LabBoostResult BoostCreep(ICreep creep, int? bodyPartsCount = null)
            => (LabBoostResult)Native_BoostCreep(ProxyObject, creep.ToJS(), bodyPartsCount);

        public LabReactionResult ReverseReaction(IStructureLab lab1, IStructureLab lab2)
            => (LabReactionResult)Native_ReverseReaction(ProxyObject, lab1.ToJS(), lab2.ToJS());

        public LabReactionResult RunReaction(IStructureLab lab1, IStructureLab lab2)
            => (LabReactionResult)Native_RunReaction(ProxyObject, lab1.ToJS(), lab2.ToJS());

        public LabBoostResult UnboostCreep(ICreep creep)
            => (LabBoostResult)Native_UnboostCreep(ProxyObject, creep.ToJS());

        public override string ToString()
            => $"StructureLab[{Id}]";
    }
}
