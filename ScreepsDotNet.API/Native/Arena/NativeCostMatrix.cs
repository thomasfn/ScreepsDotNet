using System;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeCostMatrix : ICostMatrix
    {
        #region Imports

        [JSImport("createCostMatrix", "game/pathFinder")]
        internal static partial JSObject Native_Ctor();

        [JSImport("CostMatrix.get", "game/pathFinder")]
        internal static partial byte Native_Get(JSObject proxyObject, int x, int y);

        [JSImport("CostMatrix.set", "game/pathFinder")]
        internal static partial void Native_Set(JSObject proxyObject, int x, int y, byte cost);

        [JSImport("CostMatrix.setRect", "game/pathFinder")]
        internal static partial void Native_SetRect(JSObject proxyObject, int minX, int minY, int maxX, int maxY, [JSMarshalAsDataView] Span<byte> values);

        [JSImport("CostMatrix.get", "game/pathFinder")]
        internal static partial JSObject Native_Clone(JSObject proxyObject);

        #endregion

        internal readonly JSObject ProxyObject;

        private const int RoomSizeX = 100;
        private const int RoomSizeY = 100;

        private readonly byte[] localCache = new byte[RoomSizeX * RoomSizeY];

        public byte this[Position pos]
        {
            get => localCache[pos.Y * RoomSizeX + pos.X];
            set
            {
                if (localCache[pos.Y * RoomSizeX + pos.X] == value) { return; }
                Native_Set(ProxyObject, pos.X, pos.Y, value);
                localCache[pos.Y * RoomSizeX + pos.X] = value;
            }
        }

        public NativeCostMatrix(JSObject proxyObject)
        {
            ProxyObject = proxyObject;
        }

        public NativeCostMatrix()
            : this(Native_Ctor())
        { }

        public void SetRect(Position min, Position max, ReadOnlySpan<byte> values)
        {
            Span<byte> tmp = stackalloc byte[values.Length];
            values.CopyTo(tmp);
            Native_SetRect(ProxyObject, min.X, min.Y, max.X, max.Y, tmp);
        }

        public ICostMatrix Clone()
        {
            var newObj = new NativeCostMatrix(Native_Clone(ProxyObject));
            Array.Copy(localCache, newObj.localCache, localCache.Length);
            return newObj;
        }

        
    }
}
