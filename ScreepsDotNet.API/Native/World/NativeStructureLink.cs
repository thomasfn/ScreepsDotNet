using System;
using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureLink : NativeOwnedStructure, IStructureLink
    {
        #region Imports

        [JSImport("StructureLink.transferEnergy", "game/prototypes/wrapped")]
        
        internal static partial int Native_TransferEnergy(JSObject proxyObject, JSObject target, int? amount);

        #endregion

        private NativeStore? storeCache;
        private int? cooldownCache;

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject(Names.Store));

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32(Names.Cooldown);

        public NativeStructureLink(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache?.Dispose();
            storeCache = null;
            cooldownCache = null;
        }

        public LinkTransferEnergyResult TransferEnergy(IStructureLink target, int? amount = null)
            => (LinkTransferEnergyResult)Native_TransferEnergy(ProxyObject, target.ToJS(), amount);

        public override string ToString()
            => $"StructureLink[{Id}]";
    }
}
