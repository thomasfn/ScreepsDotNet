using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class NativePathFinderExtensions
    {
        public static JSObject ToJS(this PathStep pathStep)
        {
            var obj = NativeRoomObjectUtils.CreateObject(null);
            obj.SetProperty("x", pathStep.X);
            obj.SetProperty("dx", pathStep.DX);
            obj.SetProperty("y", pathStep.Y);
            obj.SetProperty("dy", pathStep.DY);
            obj.SetProperty("direction", (int)pathStep.Direction);
            return obj;
        }
    }
}
