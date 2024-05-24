using System;
using System.Runtime.InteropServices;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeRoomTerrain : IRoomTerrain
    {
        #region Imports

        [JSImport("RoomTerrain.get", "game/prototypes/wrapped")]
        internal static partial int Native_Get(JSObject proxyObject, int x, int y);

        [JSImport("RoomTerrain.getRawBuffer", "game/prototypes/wrapped")]
        internal static partial void Native_GetRawBuffer(JSObject proxyObject, [JSMarshalAsDataView] Span<byte> destinationArray);

        #endregion

        private readonly JSObject proxyObject;

        public Terrain this[Position position] => (Terrain)Native_Get(proxyObject, position.X, position.Y);

        public NativeRoomTerrain(JSObject proxyObject)
        {
            this.proxyObject = proxyObject;
        }

        public void GetRawBuffer(Span<Terrain> outTerrainData)
        {
            if (outTerrainData.Length != 2500) { throw new ArgumentException("Span should be 2500 long", nameof(outTerrainData)); }
            Native_GetRawBuffer(proxyObject, MemoryMarshal.Cast<Terrain, byte>(outTerrainData));
        }

    }
}
