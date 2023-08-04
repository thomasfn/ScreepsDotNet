using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeRoomTerrain : IRoomTerrain
    {
        #region Imports

        [JSImport("RoomTerrain.get", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Get([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y);

        [JSImport("RoomTerrain.getRawBuffer", "game/prototypes/wrapped")]
        internal static partial void Native_GetRawBuffer([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.MemoryView>] Span<byte> destinationArray);

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
