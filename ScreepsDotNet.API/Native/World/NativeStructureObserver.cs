using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureObserver : NativeOwnedStructure, IStructureObserver
    {
        #region Imports

        [JSImport("StructureObserver.observeRoom", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_ObserveRoom([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string roomName);

        #endregion

        public NativeStructureObserver(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        public override string ToString()
            => $"StructureObserver[{Id}]";

        public ObserverObserveRoomResult ObserveRoom(RoomCoord roomCoord)
            => (ObserverObserveRoomResult)Native_ObserveRoom(ProxyObject, roomCoord.ToString());
    }
}
