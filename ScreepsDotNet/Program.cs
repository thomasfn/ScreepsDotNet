using System;
using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet
{
    public interface ITutorialScript
    {
        void Loop();
    }

    public static partial class Program
    {
        private static API.World.IGame? game;
        private static ITutorialScript? tutorialScript;

        public static void Main()
        {
            game = new Native.World.NativeGame();
            tutorialScript = new World.BasicExample(game);
        }

        [JSExport]
        internal static void Loop()
        {
            game?.Tick();
            tutorialScript?.Loop();
            if (game is API.World.IGame worldGame)
            {
                CheckHeap(worldGame.Cpu.GetHeapStatistics());
            }
            else if (game is API.Arena.IGame arenaGame)
            {
                CheckHeap(arenaGame.Utils.GetHeapStatistics());
            }
            LogGCActivity();
        }

        internal static void CheckHeap(in API.HeapInfo heapInfo)
        {
            if (ticksSinceLastGC < 10) { return; }
            var heapUsageFrac = (heapInfo.TotalHeapSize + heapInfo.ExternallyAllocatedSize) / (double)heapInfo.HeapSizeLimit;
            if (heapUsageFrac > 0.65)
            {
                Console.WriteLine($"Heap usage is high ({heapUsageFrac * 100.0:N}%), running GC...");
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true);
                ticksSinceLastGC = 0;
            }
            else if (heapUsageFrac > 0.85)
            {
                Console.WriteLine($"Heap usage is very high ({heapUsageFrac * 100.0:N}%), running aggressive GC...");
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true);
                ticksSinceLastGC = 0;
            }
        }

        private static readonly int[] lastCollectCount = new int[GC.MaxGeneration];
        private static int ticksSinceLastGC = 0;

        internal static void LogGCActivity()
        {
            bool didGC = false;
            for (int i = 0; i < GC.MaxGeneration; ++i)
            {
                int collectCount = GC.CollectionCount(i);
                if (collectCount > lastCollectCount[i])
                {
                    lastCollectCount[i] = collectCount;
                    Console.WriteLine($"Gen {i} GC happened this loop (now up to {lastCollectCount[i]} collections)");
                    didGC = true;
                }
            }
            if (!didGC) { ++ticksSinceLastGC; }
        }
    }
}
