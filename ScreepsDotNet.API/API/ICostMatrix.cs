namespace ScreepsDotNet.API
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
        int this[Position pos] { get; set; }

        /// <summary>
        /// Clones this CostMatrix.
        /// </summary>
        /// <returns></returns>
        ICostMatrix Clone();
    }
}
