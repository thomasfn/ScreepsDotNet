using ScreepsDotNet.Interop;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureTower : NativeOwnedStructureWithStore, IStructureTower
    {
        #region Imports

        [JSImport("StructureTower.attack", "game/prototypes/wrapped")]
        
        internal static partial int Native_Attack(JSObject proxyObject, JSObject target);

        [JSImport("StructureTower.heal", "game/prototypes/wrapped")]
        
        internal static partial int Native_Heal(JSObject proxyObject, JSObject target);

        [JSImport("StructureTower.repair", "game/prototypes/wrapped")]
        
        internal static partial int Native_Repair(JSObject proxyObject, JSObject target);

        #endregion

        public NativeStructureTower(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public TowerActionResult Attack(ICreep target)
            => (TowerActionResult)Native_Attack(ProxyObject, target.ToJS());

        //public TowerActionResult Attack(IPowerCreep target)
        //    => (TowerActionResult)Native_Attack(ProxyObject, target.ToJS());

        public TowerActionResult Attack(IStructure target)
            => (TowerActionResult)Native_Attack(ProxyObject, target.ToJS());

        public TowerActionResult Heal(ICreep target)
            => (TowerActionResult)Native_Heal(ProxyObject, target.ToJS());

        //public TowerActionResult Heal(IPowerCreep target)
        //    => (TowerActionResult)Native_Heal(ProxyObject, target.ToJS());

        public TowerActionResult Repair(IStructure target)
            => (TowerActionResult)Native_Repair(ProxyObject, target.ToJS());

        public override string ToString()
            => $"StructureTower[{Id}]";
    }
}
