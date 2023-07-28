using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet
{
    public interface ITutorialScript
    {
        void Loop();
    }

    public static partial class Program
    {
        private static IGame? game;
        private static ITutorialScript? tutorialScript;

        public static void Main()
        {
            game = new Native.NativeGame();
            tutorialScript = new Tutorial10_FinalTest(game);
            RunGC();
            LogGCActivity();
        }

        private static void RunGC()
        {
            var timeBefore = game!.Utils.GetCpuTime();
            GC.Collect(2, GCCollectionMode.Forced, true);
            var timeAfter = game!.Utils.GetCpuTime();
            Console.WriteLine($"GC in {(timeAfter - timeBefore) / 1000000.0:N} ms");
        }

        [JSExport]
        internal static void Loop()
        {
            tutorialScript?.Loop();
            LogGCActivity();
        }

        private static int lastGen0CollectCount = GC.CollectionCount(0);
        private static int lastGen1CollectCount = GC.CollectionCount(1);
        private static int lastGen2CollectCount = GC.CollectionCount(2);

        internal static void LogGCActivity()
        {
            int gen0CollectCount = GC.CollectionCount(0);
            if (gen0CollectCount > lastGen0CollectCount)
            {
                lastGen0CollectCount = gen0CollectCount;
                Console.WriteLine($"Gen 0 GC happened this loop (now up to {lastGen0CollectCount} collections)");
            }
            int gen1CollectCount = GC.CollectionCount(1);
            if (gen1CollectCount > lastGen1CollectCount)
            {
                lastGen1CollectCount = gen1CollectCount;
                Console.WriteLine($"Gen 1 GC happened this loop (now up to {lastGen1CollectCount} collections)");
            }
            int gen2CollectCount = GC.CollectionCount(2);
            if (gen2CollectCount > lastGen2CollectCount)
            {
                lastGen2CollectCount = gen2CollectCount;
                Console.WriteLine($"Gen 2 GC happened this loop (now up to {lastGen2CollectCount} collections)");
            }
        }
    }
}
