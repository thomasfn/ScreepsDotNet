using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public class NativeGame : IGame
    {
        public IUtils Utils { get; } = new NativeUtils();

        public IPathFinder PathFinder { get; } = new NativePathFinder();

        public IConstants Constants { get; } = new NativeConstants();
    }
}
