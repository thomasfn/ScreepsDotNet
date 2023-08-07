using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeConstructionSite : NativeRoomObject, IConstructionSite
    {
        #region Imports

        [JSImport("ConstructionSite.remove", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Remove([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        #endregion

        private readonly string id;

        private OwnerInfo? ownerInfoCache;
        private Type? structureTypeCache;

        public string Id => id;

        public bool My => ProxyObject.GetPropertyAsBoolean("my");

        public OwnerInfo Owner => ownerInfoCache ??= new(ProxyObject.GetPropertyAsJSObject("owner")!.GetPropertyAsString("username")!);

        public int Progress => ProxyObject.GetPropertyAsInt32("progress");

        public int ProgressTotal => ProxyObject.GetPropertyAsInt32("progressTotal");

        public Type StructureType => structureTypeCache ??= (NativeRoomObjectUtils.GetInterfaceTypeForStructureConstant(ProxyObject.GetPropertyAsString("structureType")!) ?? typeof(IStructure));

        public NativeConstructionSite(INativeRoot nativeRoot, JSObject proxyObject, string knownId)
            : base(nativeRoot, proxyObject)
        {
            id = knownId;
        }

        public NativeConstructionSite(INativeRoot nativeRoot, JSObject proxyObject)
            : this(nativeRoot, proxyObject, proxyObject.GetPropertyAsString("id")!)
        { }

        public override void InvalidateProxyObject()
        {
            proxyObjectOrNull = nativeRoot.GetObjectById(id);
            ClearNativeCache();
        }

        public void Remove()
            => Native_Remove(ProxyObject);
    }
}
