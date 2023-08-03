using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructure : NativeRoomObject, IStructure, IEquatable<NativeStructure?>
    {
        #region Imports

        [JSImport("Structure.destroy", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Destroy([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("Structure.isActive", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Boolean>]
        internal static partial bool Native_IsActive([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("Structure.notifyWhenAttacked", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_NotifyWhenAttacked([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Boolean>] bool enabled);

        #endregion

        private readonly string id;

        public int Hits => ProxyObject.GetPropertyAsInt32("hits");

        public int HitsMax => ProxyObject.GetPropertyAsInt32("hitsMax");

        public string Id => id;

        public NativeStructure(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
            id = proxyObject.GetPropertyAsString("id")!;
        }

        public override void InvalidateProxyObject()
        {
            ProxyObject = nativeRoot.GetObjectById(id);
        }

        public StructureDestroyResult Destroy()
            => (StructureDestroyResult)Native_Destroy(ProxyObject);

        public bool IsActive()
            => Native_IsActive(ProxyObject);

        public StructureNotifyWhenAttackedResult NotifyWhenAttacked(bool enabled)
            => (StructureNotifyWhenAttackedResult)Native_NotifyWhenAttacked(ProxyObject, enabled);

        public override bool Equals(object? obj) => Equals(obj as NativeStructure);

        public bool Equals(NativeStructure? other) => other is not null && id == other.id;

        public override int GetHashCode() => HashCode.Combine(id);

        public static bool operator ==(NativeStructure? left, NativeStructure? right) => EqualityComparer<NativeStructure>.Default.Equals(left, right);

        public static bool operator !=(NativeStructure? left, NativeStructure? right) => !(left == right);

        public override string ToString()
            => $"NativeStructure[{id}]";
    }
}
