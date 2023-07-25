using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public class NativeGame : IGame
    {
        public IUtils Utils { get; } = new NativeUtils();
    }
}
