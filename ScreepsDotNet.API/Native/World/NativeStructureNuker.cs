﻿using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureNuker : NativeOwnedStructure, IStructureNuker
    {
        #region Imports

        [JSImport("StructureNuker.launchNuke", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_LaunchNuke([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject pos);

        #endregion

        private int? cooldownCache;
        private NativeStore? storeCache;

        public int Cooldown => CachePerTick(ref cooldownCache) ??= ProxyObject.GetPropertyAsInt32("cooldown");

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureNuker(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            cooldownCache = null;
            storeCache = null;
        }

        public override string ToString()
            => $"StructureNuker[{Id}]";

        public NukerLaunchNukeResult LaunchNuke(RoomPosition pos)
        {
            using var roomPos = pos.ToJS();
            return (NukerLaunchNukeResult)Native_LaunchNuke(ProxyObject, roomPos);
        }
    }
}
