using System;
using System.Diagnostics.CodeAnalysis;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class NativeCallbacks
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(NativeCallbacks))]
        public static Func<RoomCoord, RoomCostSpecification>? currentRoomCallbackFunc;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(NativeCallbacks))]
        public static Func<RoomCoord, ICostMatrix, ICostMatrix?>? currentCostCallbackFunc;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(NativeCallbacks))]
        public static Func<RoomCoord, RoomCoord, double>? currentRouteCallbackFunc;

        [System.Runtime.InteropServices.UnmanagedCallersOnly(EntryPoint = "screeps:screepsdotnet/botapi#invoke-room-callback")]
        public static IntPtr InvokeRoomCallback(int roomCoordX, int roomCoordY)
        {
            if (currentRoomCallbackFunc == null) { return 0; }
            RoomCostSpecification result;
            try
            {
                result = currentRoomCallbackFunc((roomCoordX, roomCoordY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
            switch (result.SpecificationType)
            {
                case RoomCostSpecificationType.DoNotPathThroughRoom: return -1;
                case RoomCostSpecificationType.UseProvidedCostMatrix: return (result.CostMatrix as NativeCostMatrix)!.ProxyObject.JSHandle;
                default: return 0;
            }
        }

        [System.Runtime.InteropServices.UnmanagedCallersOnly(EntryPoint = "screeps:screepsdotnet/botapi#invoke-cost-callback")]
        public static IntPtr InvokeCostCallback(int roomCoordX, int roomCoordY, IntPtr costMatrixJsHandle)
        {
            if (currentCostCallbackFunc == null) { return 0; }
            var costMatrixJsObj = Interop.Native.GetJSObject(costMatrixJsHandle);
            var costMatrix = new NativeCostMatrix(costMatrixJsObj);
            ICostMatrix? result;
            try
            {
                result = currentCostCallbackFunc((roomCoordX, roomCoordY), costMatrix);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
            return (result as NativeCostMatrix)?.ProxyObject.JSHandle ?? 0;
        }

        [System.Runtime.InteropServices.UnmanagedCallersOnly(EntryPoint = "screeps:screepsdotnet/botapi#invoke-route-callback")]
        public static double InvokeRouteCallback(int roomCoordX, int roomCoordY, int fromRoomCoordX, int fromRoomCoordY)
        {
            if (currentRouteCallbackFunc == null) { return 0.0f; }
            try
            {
                return currentRouteCallbackFunc((roomCoordX, roomCoordY), (fromRoomCoordX, fromRoomCoordY));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0.0;
            }
        }
    }
}
