using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public class NativeGame : IGame
    {
        public IUtils Utils { get; } = new NativeUtils();

        public IPathFinder PathFinder { get; } = new NativePathFinder();

        public IConstants Constants { get; } = new NativeConstants();
    }
}
