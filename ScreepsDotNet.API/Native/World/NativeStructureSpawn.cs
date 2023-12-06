using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    using BodyType = BodyType<BodyPartType>;

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeSpawning : ISpawning
    {
        #region Imports

        [JSImport("Spawning.cancel", "game/prototypes/wrapped")]
        
        internal static partial int Native_Cancel(JSObject proxyObject);

        [JSImport("Spawning.setDirections", "game/prototypes/wrapped")]
        
        internal static partial int Native_SetDirections(JSObject proxyObject, int[] directions);

        #endregion

        private readonly INativeRoot nativeRoot;
        private readonly JSObject proxyObject;

        public int NeedTime => proxyObject.GetPropertyAsInt32("needTime");

        public int RemainingTime => proxyObject.GetPropertyAsInt32("remainingTime");

        public IEnumerable<Direction> Directions =>
            (JSUtils.GetIntArrayOnObject(proxyObject, "directions") ?? Enumerable.Empty<int>())
                .Cast<Direction>();

        public string Name => proxyObject.GetPropertyAsString("name")!;

        public IStructureSpawn Spawn
        {
            get
            {
                var spawn = nativeRoot.GetOrCreateWrapperObject<IStructureSpawn>(proxyObject.GetPropertyAsJSObject("spawn"));
                if (spawn == null) { throw new InvalidOperationException($"ISpawning failed to retrieve spawn"); }
                return spawn;
            }
        }

        public NativeSpawning(INativeRoot nativeRoot, JSObject proxyObject)
        {
            this.nativeRoot = nativeRoot;
            this.proxyObject = proxyObject;
        }

        public SpawningCancelResult Cancel()
            => (SpawningCancelResult)Native_Cancel(proxyObject);

        public SpawningSetDirectionsResult SetDirections(IEnumerable<Direction> directions)
            => (SpawningSetDirectionsResult)Native_SetDirections(proxyObject, directions.Cast<int>().ToArray());
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class SpawnCreepOptionsExtensions
    {
        public static JSObject ToJS(this SpawnCreepOptions spawnCreepOptions)
        {
            using var obj = JSObject.Create();
            if (spawnCreepOptions.Memory != null) { obj.SetProperty("memory", spawnCreepOptions.Memory.ToJS()); }
            if (spawnCreepOptions.EnergyStructures != null) { JSUtils.SetObjectArrayOnObject(obj, "energyStructures", spawnCreepOptions.EnergyStructures.Select(x => x.ToJS()).ToArray()); }
            if (spawnCreepOptions.DryRun != null) { obj.SetProperty("dryRun", spawnCreepOptions.DryRun.Value); }
            if (spawnCreepOptions.Directions != null)
            {
                JSUtils.SetIntArrayOnObject(obj, "directions", spawnCreepOptions.Directions.Cast<int>().ToArray());
            }
            return obj;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureSpawn : NativeOwnedStructure, IStructureSpawn
    {
        #region Imports

        [JSImport("StructureSpawn.spawnCreep", "game/prototypes/wrapped")]
        
        internal static partial int Native_SpawnCreep(JSObject proxyObject, string[] body, string name, JSObject opts);

        [JSImport("StructureSpawn.spawnCreep", "game/prototypes/wrapped")]
        
        internal static partial int Native_SpawnCreep(JSObject proxyObject, string[] body, string name);

        [JSImport("StructureSpawn.recycleCreep", "game/prototypes/wrapped")]
        
        internal static partial int Native_RecycleCreep(JSObject proxyObject, JSObject target);

        [JSImport("StructureSpawn.renewCreep", "game/prototypes/wrapped")]
        
        internal static partial int Native_RenewCreep(JSObject proxyObject, JSObject target);

        #endregion

        private string? nameCache;

        private NativeMemoryObject? memoryCache;
        private NativeStore? storeCache;

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject("memory")!);

        public string Name => CacheLifetime(ref nameCache) ??= ProxyObject.GetPropertyAsString("name")!;

        public ISpawning? Spawning
        {
            get
            {
                var spawningObj = ProxyObject.GetPropertyAsJSObject("spawning");
                if (spawningObj == null) { return null; }
                return new NativeSpawning(nativeRoot, spawningObj);
            }
        }

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public NativeStructureSpawn(INativeRoot nativeRoot, JSObject? proxyObject, ObjectId id)
            : base(nativeRoot, proxyObject, id)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            memoryCache = null;
            storeCache = null;
        }

        public SpawnCreepResult SpawnCreep(IEnumerable<BodyPartType> body, string name, SpawnCreepOptions? opts = null)
        {
            if (opts != null)
            {
                using var optsJs = opts.Value.ToJS();
                return (SpawnCreepResult)Native_SpawnCreep(ProxyObject, body.Select(x => x.ToJS()).ToArray(), name, optsJs);
            }
            else
            {
                return (SpawnCreepResult)Native_SpawnCreep(ProxyObject, body.Select(x => x.ToJS()).ToArray(), name);
            }
        }

        public SpawnCreepResult SpawnCreep(BodyType bodyType, string name, SpawnCreepOptions? opts = null)
            => SpawnCreep(bodyType.AsBodyPartList, name, opts);

        public RecycleCreepResult RecycleCreep(ICreep target)
            => (RecycleCreepResult)Native_RecycleCreep(ProxyObject, target.ToJS());

        public RenewCreepResult RenewCreep(ICreep target)
            => (RenewCreepResult)Native_RenewCreep(ProxyObject, target.ToJS());

        public override string ToString()
            => $"StructureSpawn[{(Exists ? $"'{Name}'" : Id.ToString())}]({(Exists ? $"{RoomPosition}" : "DEAD")})";
    }
}
