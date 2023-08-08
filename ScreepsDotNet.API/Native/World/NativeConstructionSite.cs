using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeConstructionSite : NativeRoomObject, IConstructionSite, IEquatable<NativeConstructionSite?>
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

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.GetProxyObjectById(id);

        public void Remove()
            => Native_Remove(ProxyObject);

        public override bool Equals(object? obj) => Equals(obj as NativeConstructionSite);

        public bool Equals(NativeConstructionSite? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeConstructionSite? left, NativeConstructionSite? right) => EqualityComparer<NativeConstructionSite>.Default.Equals(left, right);

        public static bool operator !=(NativeConstructionSite? left, NativeConstructionSite? right) => !(left == right);

        public override string ToString()
            => $"ConstructionSite[{id}]";
    }
}
