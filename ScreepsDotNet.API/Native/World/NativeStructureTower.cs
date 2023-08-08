using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureTower : NativeOwnedStructure, IStructureTower
    {
        #region Imports

        [JSImport("StructureTower.attack", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Attack([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject target);

        [JSImport("StructureTower.heal", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Heal([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject target);

        [JSImport("StructureTower.repair", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Repair([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject target);

        #endregion

        private NativeStore? storeCache;

        public IStore Store => storeCache ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureTower(INativeRoot nativeRoot, JSObject proxyObject, string knownId) : base(nativeRoot, proxyObject, knownId)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache = null;
        }


        public override string ToString()
            => $"StructureTower[{Id}]";

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
    }
}
