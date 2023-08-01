using System;

namespace ScreepsDotNet.API.Arena
{
    /// <summary>
    /// Container for custom navigation cost data.
    /// If a non-0 value is found in the CostMatrix then that value will be used instead of the default terrain cost.
    /// </summary>
    public interface ICostMatrix
    {
        /// <summary>
        /// Get or set the cost of a position in this CostMatrix.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        byte this[Position pos] { get; set; }

        /// <summary>
        /// Sets the cost of a rectangle of positions in this CostMatrix.
        /// More efficient than setting individually, especially for larger rectangles.
        /// </summary>
        /// <param name="min">Inclusive minimum of the rectangle</param>
        /// <param name="max">Inclusive maximum of the rectangle</param>
        /// <param name="values">min * max values (layouted as a sequence of rows)</param>
        void SetRect(Position min, Position max, ReadOnlySpan<byte> values);

        /// <summary>
        /// Clones this CostMatrix.
        /// </summary>
        /// <returns></returns>
        ICostMatrix Clone();
    }
}
