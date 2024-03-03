using System;
using System.Runtime.CompilerServices;

namespace ScreepsDotNet.API
{
    [Flags]
    public enum Terrain : byte
    {
        Plain = 0,
        Wall = 1,
        Swamp = 2,
        Lava = 4
    }

    public static class TerrainExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsTerrain(this Terrain terrain, Terrain testTerrain)
            => (terrain & testTerrain) == testTerrain;
    }
}
