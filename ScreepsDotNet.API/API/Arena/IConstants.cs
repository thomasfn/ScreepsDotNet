namespace ScreepsDotNet.API.Arena
{
    public interface IConstants
    {
        int BodyPartHits { get; }

        int GetBodyPartCost(BodyPartType bodyPartType);
    }
}
