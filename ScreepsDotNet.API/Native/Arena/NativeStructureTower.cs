using ScreepsDotNet.Interop;
using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureTower : NativeOwnedStructure, IStructureTower
    {
        #region Imports

        [JSImport("StructureTower.attack", "game/prototypes/wrapped")]
        
        internal static partial int Native_Attack(JSObject proxyObject, JSObject targetProxyObject);

        [JSImport("StructureTower.heal", "game/prototypes/wrapped")]
        
        internal static partial int Native_Heal(JSObject proxyObject, JSObject targetProxyObject);


        #endregion

        public IStore Store => new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public int Cooldown => ProxyObject.GetPropertyAsInt32("cooldown");

        public NativeStructureTower(JSObject proxyObject)
            : base(proxyObject)
        { }

        public TowerActionResult Attack(ICreep target)
            => (TowerActionResult)Native_Attack(ProxyObject, target.ToJS());

        public TowerActionResult Attack(IStructure target)
            => (TowerActionResult)Native_Attack(ProxyObject, target.ToJS());

        public TowerActionResult Heal(ICreep target)
            => (TowerActionResult)Native_Heal(ProxyObject, target.ToJS());

        public override string ToString()
            => $"StructureTower({Id}, {Position})";
    }
}
