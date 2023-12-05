// Comment out the appropiate preprocessor directive below to switch the example between arena and world mode!
#define ARENA
//#define WORLD

using ScreepsDotNet.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet
{
    public interface ITutorialScript
    {
        void Loop();
    }

    public static partial class Program
    {
#if WORLD
        private static API.World.IGame? game;
#endif
#if ARENA
        private static API.Arena.IGame? game;
#endif
        private static ITutorialScript? tutorialScript;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Program))]
        public static void Main()
        {

        }

        [Interop.JSImport("test", "addTwo")]
        private static partial int AddTwoInts(int a, int b);

        [Interop.JSImport("test", "toUppercase")]
        private static partial string ToUppercase(string str);

        [Interop.JSImport("object", "create")]
        private static partial Interop.JSObject CreateObject(Interop.JSObject? prototypeObj = null);

        [Interop.JSImport("object", "setProperty")]
        private static partial void SetIntOnObject(Interop.JSObject obj, string key, int value);

        [Interop.JSImport("test", "stringify")]
        private static partial string StringifyObject(Interop.JSObject obj);

        [Interop.JSImport("test", "stringify")]
        private static partial string StringifyObject(int[] arr);

        [Interop.JSImport("test", "reverseArray")]
        private static partial byte[] ReverseArray(byte[] arr);

        [JSExport]
        public static void Init()
        {
            try
            {
                //#if WORLD
                //            game = new Native.World.NativeGame();
                //            tutorialScript = new World.BasicExample(game);
                //#endif
                //#if ARENA
                //            game = new Native.Arena.NativeGame();
                //            tutorialScript = new Arena.Tutorial10_FinalTest(game);
                //#endif
                Console.WriteLine($"10 + 20 = {AddTwoInts(10, 20)}");

                Console.WriteLine($"uppercase of 'hello world' = '{ToUppercase("hello world")}'");

                var obj = CreateObject();
                Console.WriteLine($"new object: {obj}");

                SetIntOnObject(obj, "foo", 20);

                Console.WriteLine($"as json: {StringifyObject(obj)}");

                Console.WriteLine($"[1, 2, 3] = {StringifyObject([1, 2, 3])}");

                Console.WriteLine($"reverse of [1, 2, 3] = {string.Join(", ", ReverseArray([1, 2, 3]).Select(x => x.ToString()))}");

                obj.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [JSExport]
        public static void Loop()
        {
//#if WORLD
//            game?.Tick();
//#endif
//            tutorialScript?.Loop();
//#if WORLD
//            CheckHeap(game!.Cpu.GetHeapStatistics());
//#endif
//#if ARENA
//            CheckHeap(game!.Utils.GetHeapStatistics());
//#endif
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
