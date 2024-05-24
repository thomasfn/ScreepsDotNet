using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureObserver : NativeOwnedStructure, IStructureObserver
    {
        #region Imports

        [JSImport("StructureObserver.observeRoom", "game/prototypes/wrapped")]
        
        internal static partial int Native_ObserveRoom(JSObject proxyObject, string roomName);

        #endregion

        public NativeStructureObserver(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public ObserverObserveRoomResult ObserveRoom(RoomCoord roomCoord)
            => (ObserverObserveRoomResult)Native_ObserveRoom(ProxyObject, roomCoord.ToString());

        public override string ToString()
            => $"StructureObserver[{Id}]";
    }
}
