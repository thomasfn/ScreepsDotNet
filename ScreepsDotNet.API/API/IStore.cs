namespace ScreepsDotNet.API
{
    public enum ResourceType
    {
        Energy,
        Power,

        Hydrogen,
        Oxygen,
        Utrium,
        Lemergium,
        Keanium,
        Zynthium,
        Catalyst,
        Ghodium,

        Silicon,
        Metal,
        Biomass,
        Mist,

        Hydroxide,
        ZynthiumKeanite,
        UtriumLemergite,

        UtriumHydride,
        UtriumOxide,
        KeaniumHydride,
        KeaniumOxide,
        LemergiumHydride,
        LemergiumOxide,
        ZynthiumHydride,
        ZynthiumOxide,
        GhodiumHydride,
        GhodiumOxide,

        UtriumAcid,
        UtriumAlkalide,
        KeaniumAcid,
        KeaniumAlkalide,
        LemergiumAcid,
        LemergiumAlkalide,
        ZynthiumAcid,
        ZynthiumAlkalide,
        GhodiumAcid,
        GhodiumAlkalide,

        CatalyzedUtriumAcid,
        CatalyzedUtriumAlkalide,
        CatalyzedKeaniumAcid,
        CatalyzedKeaniumAlkalide,
        CatalyzedLemergiumAcid,
        CatalyzedLemergiumAlkalide,
        CatalyzedZynthiumAcid,
        CatalyzedZynthiumAlkalide,
        CatalyzedGhodiumAcid,
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
        /// Gets or sets how much of each resource is in this store.
        /// Note that setting the resource amount will not affect GetUsedCapacity and GetFreeCapacity, and will not persist across ticks.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        int this[ResourceType resourceType] { get; set; }
    }
}
