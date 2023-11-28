using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureTerminal : NativeOwnedStructure, IStructureTerminal
    {
        #region Imports

        [JSImport("StructureTerminal.send", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Send([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string resourceType, [JSMarshalAs<JSType.Number>] int amount, [JSMarshalAs<JSType.String>] string destination, [JSMarshalAs<JSType.String>] string? description);

        #endregion

        private NativeStore? storeCache;

        public int Cooldown => ProxyObject.GetPropertyAsInt32("cooldown");

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureTerminal(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache = null;
        }

        public TerminalSendResult Send(ResourceType resourceType, int amount, string destination, string? description = null)
            => (TerminalSendResult)Native_Send(ProxyObject, resourceType.ToJS(), amount, destination, description);

        public override string ToString()
            => $"StructureTerminal[{Id}]";
    }
}
