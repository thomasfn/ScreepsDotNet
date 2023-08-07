using System;

namespace ScreepsDotNet.API.World
{
    public interface IConstants
    {
        int GetBodyPartCost(BodyPartType bodyPartType);

        int GetConstructionCost<T>() where T : IStructure;

        int GetConstructionCost(Type structureType);

        int GetAsInt(string key);

        double GetAsDouble(string key);
    }
}
