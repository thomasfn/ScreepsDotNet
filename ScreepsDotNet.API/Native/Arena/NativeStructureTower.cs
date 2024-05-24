using ScreepsDotNet.Interop;
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

        private NativeStore? storeCache;
        private int? cooldownCache;

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(proxyObject.GetPropertyAsJSObject(Names.Store));

        public int Cooldown => CachePerTick(ref cooldownCache) ??= proxyObject.GetPropertyAsInt32(Names.Cooldown);

        public NativeStructureTower(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache?.Dispose();
            storeCache = null;
            cooldownCache = null;
        }

        public TowerActionResult Attack(ICreep target)
            => (TowerActionResult)Native_Attack(proxyObject, target.ToJS());

        public TowerActionResult Attack(IStructure target)
            => (TowerActionResult)Native_Attack(proxyObject, target.ToJS());

        public TowerActionResult Heal(ICreep target)
            => (TowerActionResult)Native_Heal(proxyObject, target.ToJS());

        public override string ToString()
            => Exists ? $"StructureTower({Id}, {Position})" : "StructureTower(DEAD)";
    }
}
