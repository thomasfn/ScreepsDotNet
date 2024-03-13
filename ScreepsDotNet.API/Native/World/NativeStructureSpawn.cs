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
    internal partial class NativeSpawning : ISpawning, IDisposable
    {
        #region Imports

        [JSImport("Spawning.cancel", "game/prototypes/wrapped")]
        
        internal static partial int Native_Cancel(JSObject proxyObject);

        [JSImport("Spawning.setDirections", "game/prototypes/wrapped")]
        
        internal static partial int Native_SetDirections(JSObject proxyObject, int[] directions);

        #endregion

        private readonly INativeRoot nativeRoot;
        private readonly JSObject proxyObject;
        private bool disposedValue;

        public int NeedTime => proxyObject.GetPropertyAsInt32(Names.NeedTime);

        public int RemainingTime => proxyObject.GetPropertyAsInt32(Names.RemainingTime);

        public IEnumerable<Direction> Directions =>
            (JSUtils.GetIntArrayOnObject(proxyObject, Names.Directions) ?? Enumerable.Empty<int>())
                .Cast<Direction>();

        public string Name => proxyObject.GetPropertyAsString(Names.Name)!;

        public IStructureSpawn Spawn
        {
            get
            {
                var spawn = nativeRoot.GetOrCreateWrapperObject<IStructureSpawn>(proxyObject.GetPropertyAsJSObject(Names.Spawn));
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

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                proxyObject.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class SpawnCreepOptionsExtensions
    {
        public static JSObject ToJS(this SpawnCreepOptions spawnCreepOptions)
        {
            using var obj = JSObject.Create();
            if (spawnCreepOptions.Memory != null) { obj.SetProperty(Names.Memory, spawnCreepOptions.Memory.ToJS()); }
            if (spawnCreepOptions.EnergyStructures != null) { JSUtils.SetObjectArrayOnObject(obj, Names.EnergyStructures, spawnCreepOptions.EnergyStructures.Select(x => x.ToJS()).ToArray()); }
            if (spawnCreepOptions.DryRun != null) { obj.SetProperty(Names.DryRun, spawnCreepOptions.DryRun.Value); }
            if (spawnCreepOptions.Directions != null)
            {
                JSUtils.SetIntArrayOnObject(obj, Names.Directions, spawnCreepOptions.Directions.Cast<int>().ToArray());
            }
            return obj;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureSpawn : NativeOwnedStructureWithStore, IStructureSpawn
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

        private NativeMemoryObject? memoryCache;
        private string? nameCache;
        private NativeSpawning? spawningCache;

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject(Names.Memory)!);

        public string Name => CacheLifetime(ref nameCache) ??= ProxyObject.GetPropertyAsString(Names.Name)!;

        public ISpawning? Spawning => CachePerTick(ref spawningCache) ??= GetSpawning();

        public NativeStructureSpawn(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            memoryCache = null;
            spawningCache?.Dispose();
            spawningCache = null;
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

        private NativeSpawning? GetSpawning()
        {
            var spawningObj = ProxyObject.GetPropertyAsJSObject(Names.Spawning);
            if (spawningObj == null) { return null; }
            return new NativeSpawning(nativeRoot, spawningObj);
        }

        public override string ToString()
            => $"StructureSpawn[{(Exists ? $"'{Name}'" : Id.ToString())}]({(Exists ? $"{RoomPosition}" : "DEAD")})";
    }
}
