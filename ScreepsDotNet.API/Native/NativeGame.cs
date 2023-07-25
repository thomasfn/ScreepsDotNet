using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    public class NativeGame : IGame
    {
        public IUtils Utils { get; } = new NativeUtils();
    }
}
