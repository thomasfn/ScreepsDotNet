using System;
using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        public static void Main()
        {
            
        }

        [JSExport]
        internal static void Loop()
        {
            var game = new Native.NativeGame();
            Console.WriteLine($"Hello from C#! Current tick: {game.Utils.GetTicks()}");
            Console.WriteLine($"CPU usage this tick: {game.Utils.GetCpuTime() / 1000000.0:N} ms");
        }
    }
}
