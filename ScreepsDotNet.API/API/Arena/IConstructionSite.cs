namespace ScreepsDotNet.API.Arena
{
    public interface IConstructionSite : IGameObject
    {
        /// <summary>
        /// The current construction progress
        /// </summary>
        int Progress { get; }

        /// <summary>
        /// The total construction progress needed for the structure to be built
        /// </summary>
        int ProgressTotal { get; }

        /// <summary>
        /// The structure that will be built (when the construction site is completed)
        /// </summary>
        IStructure? Structure { get; }

        /// <summary>
        /// Whether it is your construction site
        /// </summary>
        bool My { get; }

        /// <summary>
        /// Remove this construction site
        /// </summary>
        void Remove();
    }
}
