namespace ScreepsDotNet.API.World
{
    public enum LabBoostResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this lab.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The mineral containing in the lab cannot boost any of the creep's body parts.
        /// </summary>
        NotFound = -5,
        /// <summary>
        /// The lab does not have enough energy or minerals.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The targets is not valid creep object.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The targets are too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// Room Controller Level insufficient to use the factory.
        /// </summary>
        RclNotEnough = -14
    }

    public enum LabReactionResult
    {
        /// <summary>
        /// The operation has been scheduled successfully.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// You are not the owner of this lab.
        /// </summary>
        NotOwner = -1,
        /// <summary>
        /// The source lab does not have enough resources.
        /// </summary>
        NotEnoughResources = -6,
        /// <summary>
        /// The targets are not valid lab objects.
        /// </summary>
        InvalidTarget = -7,
        /// <summary>
        /// The target cannot receive any more resource.
        /// </summary>
        Full = -8,
        /// <summary>
        /// The targets are too far away.
        /// </summary>
        NotInRange = -9,
        /// <summary>
        /// The reaction cannot be reversed into this resources.
        /// </summary>
        InvalidArgs = -10,
        /// <summary>
        /// The lab is still cooling down.
        /// </summary>
        Tired = -11,
        /// <summary>
        /// Room Controller Level insufficient to use this structure.
        /// </summary>
        RclNotEnough = -14
    }

    /// <summary>
    /// Produces mineral compounds from base minerals, boosts and unboosts creeps.
    /// </summary>
    public interface IStructureLab : IOwnedStructure, IWithStore
    {
        /// <summary>
        /// The type of minerals containing in the lab.
        /// Labs can contain only one mineral type at the same time.
        /// </summary>
        ResourceType MineralType { get; }

        /// <summary>
        /// Boosts creep body parts using the containing mineral compound.
        /// The creep has to be at adjacent square to the lab.
        /// </summary>
        /// <param name="creep">The target creep.</param>
        /// <param name="bodyPartsCount">The number of body parts of the corresponding type to be boosted. Body parts are always counted left-to-right for TOUGH, and right-to-left for other types. If undefined, all the eligible body parts are boosted.</param>
        /// <returns></returns>
        LabBoostResult BoostCreep(ICreep creep, int? bodyPartsCount = null);

        /// <summary>
        /// Breaks mineral compounds back into reagents.
        /// The same output labs can be used by many source labs.
        /// </summary>
        /// <param name="lab1"></param>
        /// <param name="lab2"></param>
        /// <returns></returns>
        LabReactionResult ReverseReaction(IStructureLab lab1, IStructureLab lab2);

        /// <summary>
        /// Produce mineral compounds using reagents from two other labs.
        /// The same input labs can be used by many output labs.
        /// </summary>
        /// <param name="lab1"></param>
        /// <param name="lab2"></param>
        /// <returns></returns>
        LabReactionResult RunReaction(IStructureLab lab1, IStructureLab lab2);

        /// <summary>
        /// Immediately remove boosts from the creep and drop 50% of the mineral compounds used to boost it onto the ground regardless of the creep's remaining time to live.
        /// The creep has to be at adjacent square to the lab.
        /// Unboosting requires cooldown time equal to the total sum of the reactions needed to produce all the compounds applied to the creep.
        /// </summary>
        /// <param name="creep"></param>
        /// <returns></returns>
        LabBoostResult UnboostCreep(ICreep creep);
    }
}
