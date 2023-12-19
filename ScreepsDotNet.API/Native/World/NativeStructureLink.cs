using System;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
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

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public int Cooldown => ProxyObject.GetPropertyAsInt32("cooldown");

        public NativeStructureLink(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache?.Dispose();
            storeCache = null;
        }

        public LinkTransferEnergyResult TransferEnergy(IStructureLink target, int? amount = null)
            => (LinkTransferEnergyResult)Native_TransferEnergy(ProxyObject, target.ToJS(), amount);

        public override string ToString()
            => $"StructureLink[{Id}]";
    }
}
