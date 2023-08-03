using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public partial class NativeGame : IGame
    {
        #region Imports

        [JSImport("getGameObj", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetGameObject();

        #endregion

        internal JSObject ProxyObject;

        private readonly NativeCpu nativeCpu;

        public ICpu Cpu => nativeCpu;

        public long Time => ProxyObject.GetPropertyAsInt32("time");

        public NativeGame()
        {
            ProxyObject = Native_GetGameObject();
            nativeCpu = new NativeCpu(ProxyObject.GetPropertyAsJSObject("cpu")!);
        }

        public void Tick()
        {
            ProxyObject = Native_GetGameObject();
            nativeCpu.ProxyObject = ProxyObject.GetPropertyAsJSObject("cpu")!;
        }


    }
}
