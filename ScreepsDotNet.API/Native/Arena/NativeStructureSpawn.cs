using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeSpawning : ISpawning, IDisposable
    {
        #region Imports

        [JSImport("Spawning.cancel", "game/prototypes/wrapped")]
        
        internal static partial int? Native_Cancel(JSObject proxyObject);

        public CancelSpawnCreepResult Cancel()
        {
            throw new NotImplementedException();
        }

        #endregion

        private readonly INativeRoot nativeRoot;
        private readonly JSObject proxyObject;
        private bool disposedValue;

        private int? needTimeCache;
        private int? remainingTimeCache;
        private NativeCreep? creepCache;

        public int NeedTime
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return needTimeCache ??= proxyObject.GetPropertyAsInt32(Names.NeedTime);
            }
        }

        public int RemainingTime
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return remainingTimeCache ??= proxyObject.GetPropertyAsInt32(Names.RemainingTime);
            }
        }

        public ICreep Creep
        {
            get
            {
                ObjectDisposedException.ThrowIf(disposedValue, this);
                return (creepCache ??= nativeRoot.GetOrCreateWrapperForObject<NativeCreep>(proxyObject.GetPropertyAsJSObject(Names.Creep)!))!;
            }
        }

        public NativeSpawning(INativeRoot nativeRoot, JSObject proxyObject)
        {
            this.nativeRoot = nativeRoot;
            this.proxyObject = proxyObject;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    proxyObject.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureSpawn : NativeOwnedStructure, IStructureSpawn
    {
        #region Imports

        [JSImport("StructureSpawn.spawnCreep", "game/prototypes/wrapped")]
        internal static partial JSObject Native_SpawnCreep(JSObject proxyObject, Name[] bodyParts);

        #endregion

        private NativeStore? storeCache;
        private NativeSpawning? spawningCache;

        public IStore Store => CachePerTick(ref storeCache) ??= new NativeStore(proxyObject.GetPropertyAsJSObject(Names.Store));

        public ISpawning? Spawning => CachePerTick(ref spawningCache) ??= FetchSpawning();

        public NativeStructureSpawn(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            storeCache?.Dispose();
            storeCache = null;
            spawningCache?.Dispose();
            spawningCache = null;
        }

        public SpawnCreepResult SpawnCreep(IEnumerable<BodyPartType> body)
        {
            using var resultObj = Native_SpawnCreep(proxyObject, body.Select(x => x.ToJS()).ToArray()) ?? throw new InvalidOperationException($"StructureSpawn.spawnCreep returned null or undefined");
            var creepObj = resultObj.GetPropertyAsJSObject(Names.Object);
            int? error = resultObj.TryGetPropertyAsInt32(Names.Error);
            return new SpawnCreepResult(creepObj != null ? nativeRoot.GetOrCreateWrapperForObject<NativeCreep>(creepObj) : null, (SpawnCreepError?)error);
        }

        public SpawnCreepResult SpawnCreep(BodyType<BodyPartType> bodyType)
            => SpawnCreep(bodyType.AsBodyPartList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeSpawning? FetchSpawning()
        {
            var spawningObj = proxyObject.GetPropertyAsJSObject(Names.Spawning);
            if (spawningObj == null) { return null; }
            return new NativeSpawning(nativeRoot, spawningObj);
        }

        public override string ToString()
            => $"StructureSpawn({Id}, {Position})";

    }
}
