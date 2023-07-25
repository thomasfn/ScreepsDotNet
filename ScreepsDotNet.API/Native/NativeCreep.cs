using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;

namespace ScreepsDotNet.Native
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeCreep : NativeGameObject, ICreep
    {
        #region Imports

        [JSImport("Creep.attack", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Attack([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        [JSImport("Creep.moveTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_MoveTo([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject targetProxyObject);

        #endregion

        public IEnumerable<BodyPart> Body => throw new NotImplementedException();

        public double Fatigue => ProxyObject.GetPropertyAsDouble("fatigue");

        public int Hits => ProxyObject.GetPropertyAsInt32("hits");

        public int HitsMax => ProxyObject.GetPropertyAsInt32("hitsMax");

        public bool My => ProxyObject.GetPropertyAsBoolean("my");

        public bool Spawning => ProxyObject.GetPropertyAsBoolean("spawning");

        public NativeCreep(JSObject proxyObject)
            : base(proxyObject)
        { }

        public CreepAttackResult Attack(ICreep target)
            => (CreepAttackResult)Native_Attack(ProxyObject, target.ToJS());

        public CreepMoveResult MoveTo(IPosition target)
            => (CreepMoveResult)Native_MoveTo(ProxyObject, target.ToJS());

        public override string ToString()
            => $"Creep({Id}, {Position})";
    }
}
