using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeStructureTower : NativeOwnedStructure, IStructureTower
    {
        #region Imports

        [JSImport("StructureTower.attack", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Attack([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("StructureTower.heal", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Heal([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);


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
