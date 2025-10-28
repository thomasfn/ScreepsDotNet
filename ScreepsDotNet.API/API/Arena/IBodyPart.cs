namespace ScreepsDotNet.API.Arena
{
    /// <summary>
    /// A separate part of creep body.
    /// Step over a BodyPart by a creep to augment the creep with the body part.
    /// </summary>
    public interface IBodyPart : IGameObject
    {
        /// <summary>
        /// The type of the body part.
        /// </summary>
        BodyPartType Type { get; }
    }
}
