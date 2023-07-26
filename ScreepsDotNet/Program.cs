using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet
{
    public interface ITutorialScript
    {
        void Loop();
    }

    public static partial class Program
    {
        private static ITutorialScript? tutorialScript;

        public static void Main()
        {
            
        }

        [JSExport]
        internal static void Loop()
        {
            if (tutorialScript == null)
            {
                // Change the tutorial script to solve a different tutorial here
                tutorialScript = new Tutorial7_SpawnCreeps(new Native.NativeGame());
            }
            tutorialScript.Loop();
        }
    }
}
