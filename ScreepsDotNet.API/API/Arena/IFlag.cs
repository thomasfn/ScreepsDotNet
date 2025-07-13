namespace ScreepsDotNet.API.Arena
{
    public interface IFlag : IGameObject
    {
        /// <summary>
        /// true for your flag, false for a hostile flag, undefined for a neutral flag
        /// </summary>
        bool? My { get; }
    }
}
