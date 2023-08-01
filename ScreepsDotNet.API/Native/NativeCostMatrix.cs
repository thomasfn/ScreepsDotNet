using ScreepsDotNet.API;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeCostMatrix : ICostMatrix
    {
        #region Imports

        [JSImport("createCostMatrix", "game/pathFinder")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_Ctor();

        [JSImport("CostMatrix.get", "game/pathFinder")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial byte Native_Get([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y);

        [JSImport("CostMatrix.set", "game/pathFinder")]
        internal static partial void Native_Set([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y, [JSMarshalAs<JSType.Number>] byte cost);

        [JSImport("CostMatrix.setRect", "game/pathFinder")]
        internal static partial void Native_SetRect([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int minX, [JSMarshalAs<JSType.Number>] int minY, [JSMarshalAs<JSType.Number>] int maxX, [JSMarshalAs<JSType.Number>] int maxY, [JSMarshalAs<JSType.MemoryView>] Span<byte> values);

        [JSImport("CostMatrix.get", "game/pathFinder")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_Clone([JSMarshalAs<JSType.Object>] JSObject proxyObject);

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
