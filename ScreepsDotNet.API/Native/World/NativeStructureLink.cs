﻿using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureLink : NativeOwnedStructure, IStructureLink
    {
        #region Imports

        [JSImport("StructureLink.transferEnergy", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_TransferEnergy([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject target, [JSMarshalAs<JSType.Number>] int? amount);

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
            storeCache = null;
        }

        public LinkTransferEnergyResult TransferEnergy(IStructureLink target, int? amount = null)
            => (LinkTransferEnergyResult)Native_TransferEnergy(ProxyObject, target.ToJS(), amount);

        public override string ToString()
            => $"StructureLink[{Id}]";
    }
}
