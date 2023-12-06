﻿using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeSpawning : ISpawning
    {
        #region Imports

        [JSImport("Spawning.cancel", "game/prototypes/wrapped")]
        
        internal static partial int? Native_Cancel(JSObject proxyObject);

        public CancelSpawnCreepResult Cancel()
        {
            throw new NotImplementedException();
        }

        #endregion

        private readonly JSObject ProxyObject;

        public int NeedTime => ProxyObject.GetPropertyAsInt32("needTime");

        public int RemainingTime => ProxyObject.GetPropertyAsInt32("remainingTime");

        public ICreep Creep => (NativeGameObjectUtils.CreateWrapperForObject(ProxyObject.GetPropertyAsJSObject("creep")!) as ICreep)!;

        public NativeSpawning(JSObject proxyObject)
        {
            ProxyObject = proxyObject;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeStructureSpawn : NativeOwnedStructure, IStructureSpawn
    {
        #region Imports

        [JSImport("StructureSpawn.spawnCreep", "game/prototypes/wrapped")]
        
        internal static partial JSObject Native_SpawnCreep(JSObject proxyObject, string[] bodyParts);

        #endregion

        public IStore Store => new NativeStore(ProxyObject.GetPropertyAsJSObject("store"));

        public ISpawning? Spawning
        {
            get
            {
                var spawningObj = ProxyObject.GetPropertyAsJSObject("spawning");
                if (spawningObj == null) { return null; }
                return new NativeSpawning(spawningObj);
            }
        }

        public NativeStructureSpawn(JSObject proxyObject)
            : base(proxyObject)
        { }

        public SpawnCreepResult SpawnCreep(IEnumerable<BodyPartType> body)
        {
            var resultObj = Native_SpawnCreep(ProxyObject, body.Select(x => x.ToJS()).ToArray());
            if (resultObj == null) { throw new InvalidOperationException($"StructureSpawn.spawnCreep returned null or undefined"); }
            var creepObj = resultObj.GetPropertyAsJSObject("object");
            int? error = resultObj.GetTypeOfProperty("error") == JSPropertyType.Number ? resultObj.GetPropertyAsInt32("error") : null;
            return new SpawnCreepResult(creepObj != null ? NativeGameObjectUtils.CreateWrapperForObject(creepObj) as ICreep : null, (SpawnCreepError?)error);
        }

        public SpawnCreepResult SpawnCreep(BodyType<BodyPartType> bodyType)
            => SpawnCreep(bodyType.AsBodyPartList);

        public override string ToString()
            => $"StructureSpawn({Id}, {Position})";

    }
}
