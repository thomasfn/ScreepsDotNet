namespace ScreepsDotNet.API.Arena
{
    public interface IOwnedStructure : IStructure
    {
        /// <summary>
        /// true for your structure, false for a hostile structure, undefined for a neutral structure
        /// </summary>
        bool? My { get; }
    }
}
