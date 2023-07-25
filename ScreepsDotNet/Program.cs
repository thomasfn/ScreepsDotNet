using System;
using System.Runtime.InteropServices.JavaScript;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        private static Tutorial3_FirstAttack? tutorial3_FirstAttack;

        public static void Main()
        {
            
        }

        [JSExport]
        internal static void Loop()
        {
            if (tutorial3_FirstAttack == null)
            {
                tutorial3_FirstAttack = new Tutorial3_FirstAttack(new Native.NativeGame());
            }
            tutorial3_FirstAttack.Loop();
        }
    }
}
