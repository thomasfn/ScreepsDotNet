using System.Collections.Generic;

namespace ScreepsDotNet.API.World
{
    public enum ResourceType
    {
        Energy,
        Power,

        /// <summary>H</summary>
        Hydrogen,
        /// <summary>O</summary>
        Oxygen,
        /// <summary>U</summary>
        Utrium,
        /// <summary>L</summary>
        Lemergium,
        /// <summary>K</summary>
        Keanium,
        /// <summary>Z</summary>
        Zynthium,
        /// <summary>X</summary>
        Catalyst,
        /// <summary>G</summary>
        Ghodium,

        Silicon,
        Metal,
        Biomass,
        Mist,

        /// <summary>OH</summary>
        Hydroxide,
        /// <summary>ZK</summary>
        ZynthiumKeanite,
        /// <summary>UL</summary>
        UtriumLemergite,

        /// <summary>UH</summary>
        UtriumHydride,
        /// <summary>UO</summary>
        UtriumOxide,
        /// <summary>KH</summary>
        KeaniumHydride,
        /// <summary>KO</summary>
        KeaniumOxide,
        /// <summary>LH</summary>
        LemergiumHydride,
        /// <summary>LO</summary>
        LemergiumOxide,
        /// <summary>ZH</summary>
        ZynthiumHydride,
        /// <summary>ZO</summary>
        ZynthiumOxide,
        /// <summary>GH</summary>
        GhodiumHydride,
        /// <summary>GO</summary>
        GhodiumOxide,

        /// <summary>UH2O</summary>
        UtriumAcid,
        /// <summary>UHO2</summary>
        UtriumAlkalide,
        /// <summary>KH2O</summary>
        KeaniumAcid,
        /// <summary>KHO2</summary>
        KeaniumAlkalide,
        /// <summary>LH2O</summary>
        LemergiumAcid,
        /// <summary>LHO2</summary>
        LemergiumAlkalide,
        /// <summary>ZH2O</summary>
        ZynthiumAcid,
        /// <summary>ZHO2</summary>
        ZynthiumAlkalide,
        /// <summary>GH2O</summary>
        GhodiumAcid,
        /// <summary>GHO2</summary>
        GhodiumAlkalide,

        /// <summary>XUH2O</summary>
        CatalyzedUtriumAcid,
        /// <summary>XUHO2</summary>
        CatalyzedUtriumAlkalide,
        /// <summary>XKH2O</summary>
        CatalyzedKeaniumAcid,
        /// <summary>XKHO2</summary>
        CatalyzedKeaniumAlkalide,
        /// <summary>XLH2O</summary>
        CatalyzedLemergiumAcid,
        /// <summary>XLHO2</summary>
        CatalyzedLemergiumAlkalide,
        /// <summary>XZH2O</summary>
        CatalyzedZynthiumAcid,
        /// <summary>XZHO2</summary>
        CatalyzedZynthiumAlkalide,
        /// <summary>XGH2O</summary>
        CatalyzedGhodiumAcid,
        /// <summary>XGHO2</summary>
        CatalyzedGhodiumAlkalide,

        Ops,

        UtriumBar,
        LemergiumBar,
        ZynthiumBar,
        KeaniumBar,
        GhodiumMelt,
        Oxidant,
        Reductant,
        Purifier,
        Battery,

        Composite,
        Crystal,
        Liquid,

        Wire,
        Switch,
        Transistor,
        Microchip,
        Circuit,
        Device,

        Cell,
        Phlegm,
        Tissue,
        Muscle,
        Organoid,
        Organism,

        Alloy,
        Tube,
        Fixtures,
        Frame,
        Hydraulics,
        Machine,

        Condensate,
        Concentrate,
        Extract,
        Spirit,
        Emanation,
        Essence,

        Season,
        Score,

        Unknown
    }

    /// <summary>
    /// An object that can contain resources in its cargo
    /// </summary>
    public interface IStore
    {
        /// <summary>
        /// Returns capacity of this store for the specified resource
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int? GetCapacity(ResourceType? resourceType = null);

        /// <summary>
        /// Returns the capacity used by the specified resource
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int? GetUsedCapacity(ResourceType? resourceType = null);

        /// <summary>
        /// Returns free capacity for the store
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int? GetFreeCapacity(ResourceType? resourceType = null);

        /// <summary>
        /// Gets all resource types contained within this store.
        /// </summary>
        IEnumerable<ResourceType> ContainedResourceTypes { get; }

        /// <summary>
        /// Gets or sets how much of each resource is in this store.
        /// Note that setting the resource amount will not affect GetUsedCapacity and GetFreeCapacity, and will not persist across ticks.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int this[ResourceType resourceType] { get; set; }
    }
}
