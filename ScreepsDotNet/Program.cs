// Comment out the appropiate preprocessor directive below to switch the example between arena and world mode!
//#define ARENA
#define WORLD

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using ScreepsDotNet.Interop;

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

        //[Interop.JSImport("test", "addTwo")]
        //private static partial int AddTwoInts(int a, int b);

        //[Interop.JSImport("test", "toUppercase")]
        //private static partial string ToUppercase(string str);

        //[Interop.JSImport("test", "stringify")]
        //private static partial string StringifyObject(Interop.JSObject obj);

        //[Interop.JSImport("test", "stringify")]
        //private static partial string StringifyObject(int[] arr);

        //[Interop.JSImport("test", "stringify")]
        //private static partial string StringifyObject(string[] arr);

        //[Interop.JSImport("test", "reverseArray")]
        //private static partial string[] ReverseArray(ImmutableArray<string> arr);

        //[Interop.JSImport("test", "reverseArray")]
        //private static partial ImmutableArray<int> ReverseArray(ReadOnlySpan<int> arr);

        //[Interop.JSImport("test", "fillBuffer")]
        //private static partial void FillBuffer([JSMarshalAsDataView] Span<byte> arr);

        //public static void Init()
        //{
        //    try
        //    {

        //        Console.WriteLine($"10 + 20 = {AddTwoInts(10, 20)}");

        //        Console.WriteLine($"uppercase of 'hello world' = '{ToUppercase("hello world")}'");

        //        using var obj = Interop.JSObject.Create();
        //        Console.WriteLine($"new object: {obj}");

        //        obj.SetProperty("foo", 20);

        //        Console.WriteLine($"as json: {StringifyObject(obj)}");

        //        Console.WriteLine($"type of foo: {obj.GetTypeOfProperty("foo")}");

        //        Console.WriteLine($"[1, 2, 3] = {StringifyObject([1, 2, 3])}");

        //        Console.WriteLine($"reverse of [1, 2, 3] = [{string.Join(", ", ReverseArray([1, 2, 3]).Select(x => x.ToString()))}]");

        //        Span<byte> buffer = stackalloc byte[10];
        //        FillBuffer(buffer);

        //        Console.WriteLine($"filled buffer from js = [{string.Join(", ", buffer.ToImmutableArray().Select(x => x.ToString()))}]");

        //        Console.WriteLine($"['a', 'bc', 'def'] = {StringifyObject(["a", "bc", "def"])}");

        //        Console.WriteLine($"reverse of ['a', 'bc', 'def'] = [{string.Join(", ", ReverseArray(["a", "bc", "def"]))}]");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Init()
        {
            try
            {
#if WORLD
                game = new Native.World.NativeGame();
                tutorialScript = new World.BasicExample(game);
#endif
#if ARENA
                game = new Native.Arena.NativeGame();
                tutorialScript = new Arena.Tutorial10_FinalTest(game);
#endif
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
#if WORLD
                game?.Tick();
#endif
                tutorialScript?.Loop();
#if WORLD
                CheckHeap(game!.Cpu.GetHeapStatistics());
#endif
#if ARENA
                CheckHeap(game!.Utils.GetHeapStatistics());
#endif
                LogGCActivity();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
