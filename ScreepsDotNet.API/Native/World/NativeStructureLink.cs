using System;
using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureLink : NativeOwnedStructureWithStore, IStructureLink
    {
        #region Imports

        [JSImport("StructureLink.transferEnergy", "game/prototypes/wrapped")]
        
        internal static partial int Native_TransferEnergy(JSObject proxyObject, JSObject target, int? amount);

        #endregion

        private int? cooldownCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32(Names.Cooldown);

        public NativeStructureLink(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
        }

        public LinkTransferEnergyResult TransferEnergy(IStructureLink target, int? amount = null)
            => (LinkTransferEnergyResult)Native_TransferEnergy(ProxyObject, target.ToJS(), amount);

        public override string ToString()
            => $"StructureLink[{Id}]";
    }
}
