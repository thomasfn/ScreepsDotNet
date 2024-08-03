using System;
using System.Diagnostics.CodeAnalysis;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        private static API.World.IGame? game;
        private static API.Bot.IBot? bot;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Program))]
        public static void Main()
        {

        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Init()
        {
            try
            {
                game = new Native.World.NativeGame();
                bot = new ExampleWorldBot.BasicExample(game);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Loop()
        {
            try
            {
                game?.Tick();
                bot?.Loop();
                CheckHeap(game!.Cpu.GetHeapStatistics());
                LogGCActivity();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void CheckHeap(in API.HeapInfo heapInfo)
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
            } else if (ticksSinceLastGC > 100)
            {
                Console.WriteLine($"GC hasn't run in a while (heap usage at {heapUsageFrac * 100.0:N}%), running GC...");
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true);
                ticksSinceLastGC = 0;
            }
        }

        private static readonly int[] lastCollectCount = new int[GC.MaxGeneration];
        private static int ticksSinceLastGC = 0;

        private static void LogGCActivity()
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
