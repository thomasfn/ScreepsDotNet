namespace ScreepsDotNet.API
{
    public interface IConstants
    {
        int BodyPartHits { get; }

        int GetBodyPartCost(BodyPartType bodyPartType);
    }
}
