using ScreepsDotNet.API;
using System;
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
        internal static partial int Native_Get([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y);

        [JSImport("CostMatrix.set", "game/pathFinder")]
        internal static partial void Native_Set([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y, [JSMarshalAs<JSType.Number>] int cost);

        [JSImport("CostMatrix.get", "game/pathFinder")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_Clone([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        #endregion

        internal readonly JSObject ProxyObject;

        private const int RoomSizeX = 100;
        private const int RoomSizeY = 100;

        private readonly byte[] localCache = new byte[RoomSizeX * RoomSizeY];

        public int this[Position pos]
        {
            get => localCache[pos.Y * RoomSizeX + pos.X];
            set
            {
                value = Math.Clamp(value, 0, 255);
                if (localCache[pos.Y * RoomSizeX + pos.X] == (byte)value) { return; }
                Native_Set(ProxyObject, pos.X, pos.Y, value);
                localCache[pos.Y * RoomSizeX + pos.X] = (byte)value;
            }
        }

        public NativeCostMatrix(JSObject proxyObject)
        {
            ProxyObject = proxyObject;
        }

        public NativeCostMatrix()
            : this(Native_Ctor())
        { }

        public ICostMatrix Clone()
        {
            var newObj = new NativeCostMatrix(Native_Clone(ProxyObject));
            Array.Copy(localCache, newObj.localCache, localCache.Length);
            return newObj;
        }
    }
}
