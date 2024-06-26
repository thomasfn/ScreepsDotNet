﻿using System;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// An object which provides fast access to room terrain data.
    /// </summary>
    public interface IRoomTerrain
    {
        /// <summary>
        /// Get terrain type at the specified room position by (x,y) coordinates.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Terrain this[Position position] { get; }

        /// <summary>
        /// Get copy of underlying static terrain buffer.
        /// </summary>
        /// <param name="outTerrainData">Must have length of 2500</param>
        void GetRawBuffer(Span<Terrain> outTerrainData);
    }
}
