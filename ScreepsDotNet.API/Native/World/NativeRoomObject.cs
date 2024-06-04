using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
    internal struct RoomObjectMetadata
    {
        public int TypeId;
        public IntPtr JSHandle;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal abstract partial class NativeRoomObject : NativeObject, IRoomObject
    {
        #region Imports

        [JSImport("getProperty", "__object")]
        internal static partial JSObject[] Native_GetEffects(JSObject proxyObject, Name key);

        #endregion

        private UserDataStorage userDataStorage;

        protected RoomPosition? positionCache;
        protected Effect[]? effectsCache;

        protected virtual bool CanMove { get => false; }

        public IEnumerable<Effect> Effects => CachePerTick(ref effectsCache) ??= FetchEffects();

        public RoomPosition RoomPosition
        {
            get
            {
                if (CanMove)
                {
                    return CachePerTick(ref positionCache) ??= FetchRoomPosition();
                }
                else
                {
                    return CacheLifetime(ref positionCache) ??= FetchRoomPosition();
                }
            }
        }

        public IRoom? Room
        {
            get
            {
                var roomObj = ProxyObject.GetPropertyAsJSObject(Names.Room);
                if (roomObj == null) { return null; }
                return nativeRoot.GetRoomByProxyObject(roomObj);
            }
        }

        public NativeRoomObject(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            if (CanMove)
            {
                positionCache = null;
            }
        }

        #region User Data

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetUserData<T>(T? userData) where T : class => userDataStorage.SetUserData<T>(userData);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetUserData<T>([MaybeNullWhen(false)] out T userData) where T : class => userDataStorage.TryGetUserData<T>(out userData);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetUserData<T>() where T : class => userDataStorage.GetUserData<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasUserData<T>() where T : class => userDataStorage.HasUserData<T>();

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RoomPosition FetchRoomPosition()
        {
            RoomPosition roomPosition = default;
            unsafe
            {
                ScreepsDotNet_Native.FetchObjectRoomPosition(ProxyObject.JSHandle, &roomPosition);
            }
            return roomPosition;
        }

        private Effect[] FetchEffects()
        {
            var effectsArr = Native_GetEffects(ProxyObject, Names.Effects);
            var result = new Effect[effectsArr.Length];
            try
            {
                for (int i = 0; i < effectsArr.Length; ++i)
                {
                    var obj = effectsArr[i];
                    result[i] = new(
                        effectType: (EffectType)obj.GetPropertyAsInt32(Names.Effect),
                        level: obj.TryGetPropertyAsInt32(Names.Level),
                        ticksRemaining: obj.GetPropertyAsInt32(Names.TicksRemaining)
                    );
                }
                return result;
            }
            finally
            {
                effectsArr.DisposeAll();
            }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal abstract partial class NativeRoomObjectWithId : NativeRoomObject, IWithId, IEquatable<NativeRoomObjectWithId?>
    {
        private ObjectId? idCache;

        public ObjectId Id => CacheLifetime(ref idCache) ??= FetchId(ProxyObject);

        protected NativeRoomObjectWithId(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        {
        }

        public override JSObject? ReacquireProxyObject()
        {
            if (idCache == null) { throw new InvalidOperationException($"Failed to reacquire proxy object (id was never cached)"); }
            return nativeRoot.GetProxyObjectById(idCache.Value);
        }

        protected override void OnLoseProxyObject(JSObject proxyObject)
        {
            base.OnLoseProxyObject(proxyObject);
            // Here we are losing the proxy object, e.g. we lost visibility of the object
            // If we haven't already cached the id, do so now so that we can recover the object via getObjectById if we regain visibility of it later
            if (idCache == null) { idCache = FetchId(proxyObject); }
        }

        private static ObjectId FetchId(JSObject from)
        {
            ScreepsDotNet_Native.RawObjectId rawObjectId = default;
            unsafe
            {
                if (ScreepsDotNet_Native.GetObjectId(from.JSHandle, &rawObjectId) == 0)
                {
                    throw new InvalidOperationException($"Failed to fetch object id for {from} (native call returned null)");
                }
            }
            return new(rawObjectId.AsSpan);
        }

        public override bool Equals(object? obj) => Equals(obj as NativeRoomObjectWithId);

        public bool Equals(NativeRoomObjectWithId? other) => other is not null && Id.Equals(other.Id);

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(NativeRoomObjectWithId? left, NativeRoomObjectWithId? right) => EqualityComparer<NativeRoomObjectWithId>.Default.Equals(left, right);

        public static bool operator !=(NativeRoomObjectWithId? left, NativeRoomObjectWithId? right) => !(left == right);
    }

    internal enum FindConstant
    {
        ExitTop = 1,
        ExitRight = 3,
        ExitBottom = 5,
        ExitLeft = 7,
        Exit = 10,
        Creeps = 101,
        MyCreeps = 102,
        HostileCreeps = 103,
        SourcesActive = 104,
        Sources = 105,
        DroppedResources = 106,
        Structures = 107,
        MyStructures = 108,
        HostileStructures = 109,
        Flags = 110,
        ConstructionSites = 111,
        MySpawns = 112,
        HostileSpawns = 113,
        MyConstructionSites = 114,
        HostileConstructionSites = 115,
        Minerals = 116,
        Nukes = 117,
        Tombstones = 118,
        PowerCreeps = 119,
        MyPowerCreeps = 120,
        HostilePowerCreeps = 121,
        Deposits = 122,
        Ruins = 123,
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class NativeRoomObjectPrototypes<T> where T : IRoomObject
    {
        public static int TypeId;
        public static JSObject? ConstructorObj;
        public static FindConstant? FindConstant;
        public static FindConstant? MyFindConstant;
        public static FindConstant? HostileFindConstant;
        public static string? LookConstant;
        public static string? StructureConstant;

        static NativeRoomObjectPrototypes()
        {
            NativeRoomObjectUtils.RegisterTypeMappingsIfNeeded();
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class NativeRoomObjectUtils
    {
        private const string TypeIdKey = "__dotnet_typeId";

        private static JSObject? prototypesObject;
        private static readonly List<Type> prototypeTypeMappings = new();
        private static readonly Dictionary<Type, string> prototypeNameMappings = new();
        private static readonly Dictionary<string, Type> structureConstantInterfaceMap = new();
        private static readonly Dictionary<Type, string> interfaceStructureConstantMap = new();

        #region Imports

        [JSImport("getPrototypes", "game")]
        internal static partial JSObject GetPrototypesObject();

        [JSImport("createRoomPosition", "game")]
        internal static partial JSObject CreateRoomPosition(int encodedInt);

        [JSImport("interpretDateTime", "object")]
        internal static partial double InterpretDateTime(JSObject obj);

        #endregion

        internal static void RegisterPrototypeTypeMapping<TInterface, TConcrete>(string prototypeName, FindConstant? findConstant = null, FindConstant? myFindConstant = null, FindConstant? hostileFindConstant = null, string? lookConstant = null, string? structureConstant = null)
            where TInterface : IRoomObject
            where TConcrete : NativeRoomObject
        {
            var constructor = prototypesObject!.GetPropertyAsJSObject(prototypeName);
            if (constructor == null)
            {
                Console.WriteLine($"Failed to retrieve constructor for '{prototypeName}'");
                return;
            }
            if (constructor.HasProperty(TypeIdKey))
            {
                Console.WriteLine($"Constructor for '{prototypeName}' was already bound to {prototypeTypeMappings[constructor.GetPropertyAsInt32(TypeIdKey) - 1]}");
                return;
            }
            int typeId = prototypeTypeMappings.Count;
            constructor.SetProperty(TypeIdKey, typeId + 1);
            prototypeTypeMappings.Add(typeof(TConcrete));
            NativeRoomObjectPrototypes<TInterface>.TypeId = typeId;
            NativeRoomObjectPrototypes<TInterface>.ConstructorObj = constructor;
            NativeRoomObjectPrototypes<TInterface>.FindConstant = findConstant;
            NativeRoomObjectPrototypes<TInterface>.MyFindConstant = myFindConstant;
            NativeRoomObjectPrototypes<TInterface>.HostileFindConstant = hostileFindConstant;
            NativeRoomObjectPrototypes<TInterface>.LookConstant = lookConstant;
            NativeRoomObjectPrototypes<TInterface>.StructureConstant = structureConstant;
            NativeRoomObjectPrototypes<TConcrete>.TypeId = typeId;
            NativeRoomObjectPrototypes<TConcrete>.ConstructorObj = constructor;
            NativeRoomObjectPrototypes<TConcrete>.FindConstant = findConstant;
            NativeRoomObjectPrototypes<TConcrete>.MyFindConstant = myFindConstant;
            NativeRoomObjectPrototypes<TConcrete>.HostileFindConstant = hostileFindConstant;
            NativeRoomObjectPrototypes<TConcrete>.LookConstant = lookConstant;
            NativeRoomObjectPrototypes<TConcrete>.StructureConstant = structureConstant;
            prototypeNameMappings.Add(typeof(TInterface), prototypeName);
            prototypeNameMappings.Add(typeof(TConcrete), prototypeName);
            if (!string.IsNullOrEmpty(structureConstant))
            {
                structureConstantInterfaceMap.Add(structureConstant, typeof(TInterface));
                interfaceStructureConstantMap.Add(typeof(TInterface), structureConstant);
            }
        }

        internal static Type? GetWrapperTypeForConstructor(JSObject constructor)
        {
            int? typeId = constructor.TryGetPropertyAsInt32(TypeIdKey) - 1;
            if (typeId == null || typeId < 0)
            {
                Console.WriteLine($"Failed to retrieve wrapper type for {constructor} - typeId not found");
                return null;
            }
            return prototypeTypeMappings[typeId.Value];
        }

        internal static Type? GetWrapperTypeForObject(JSObject jsObject)
            => GetWrapperTypeForConstructor(JSUtils.GetConstructorOf(jsObject));

        internal static NativeRoomObject? CreateWrapperForRoomObject(INativeRoot nativeRoot, JSObject proxyObject, Type expectedType)
        {
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType == null) { return null; }
            if (!wrapperType.IsAssignableTo(expectedType)) { return null; }
            return NativeRoomObjectFactory.Create(wrapperType, nativeRoot, proxyObject) as NativeRoomObject;
        }

        internal static T? CreateWrapperForRoomObject<T>(INativeRoot nativeRoot, JSObject proxyObject) where T : class, IRoomObject
        {
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType == null) { return null; }
            if (!wrapperType.IsAssignableTo(typeof(T))) { return null; }
            return NativeRoomObjectFactory.Create<T>(nativeRoot, proxyObject);
        }

        internal static NativeRoomObject? CreateWrapperForRoomObject(INativeRoot nativeRoot, JSObject proxyObject, in RoomObjectMetadata metadata, Type expectedType)
        {
            int typeId = metadata.TypeId - 1;
            if (typeId < 0) { return null; }
            Type wrapperType = prototypeTypeMappings[typeId];
            if (!wrapperType.IsAssignableTo(expectedType)) { return null; }
            return NativeRoomObjectFactory.Create(wrapperType, nativeRoot, proxyObject) as NativeRoomObject;
        }

        internal static T? CreateWrapperForRoomObject<T>(INativeRoot nativeRoot, JSObject proxyObject, in RoomObjectMetadata metadata) where T : class, IRoomObject
        {
            int typeId = metadata.TypeId - 1;
            if (typeId < 0) { return null; }
            Type wrapperType = prototypeTypeMappings[typeId];
            if (!wrapperType.IsAssignableTo(typeof(T))) { return null; }
            return NativeRoomObjectFactory.Create<T>(nativeRoot, proxyObject);
        }

        internal static string GetPrototypeName(Type type)
            => prototypeNameMappings[type];

        internal static Type? GetInterfaceTypeForStructureConstant(string structureConstant)
            => structureConstantInterfaceMap.TryGetValue(structureConstant, out var result) ? result : null;

        internal static string? GetStructureConstantForInterfaceType(Type interfaceType)
            => interfaceStructureConstantMap.TryGetValue(interfaceType, out var result) ? result : null;

        internal static void RegisterTypeMappingsIfNeeded()
        {
            if (prototypeTypeMappings.Count > 0) { return; }
            RegisterTypeMappings();
        }

        internal static void RegisterTypeMappings()
        {
            prototypesObject = GetPrototypesObject();
            try
            {
                // NOTE - order matters! We must do derived classes first and base classes last
                // This is to avoid a nasty bug where the runtime gets objects and their prototype objects mixed up due to the tracking id being set as a property on the object
                RegisterPrototypeTypeMapping<IStructureSpawn, NativeStructureSpawn>("StructureSpawn", FindConstant.Structures, FindConstant.MySpawns, FindConstant.HostileSpawns, "structure", "spawn");
                RegisterPrototypeTypeMapping<IStructureContainer, NativeStructureContainer>("StructureContainer", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "container");
                RegisterPrototypeTypeMapping<IStructureController, NativeStructureController>("StructureController", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "controller");
                RegisterPrototypeTypeMapping<IStructureExtension, NativeStructureExtension>("StructureExtension", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "extension");
                RegisterPrototypeTypeMapping<IStructureStorage, NativeStructureStorage>("StructureStorage", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "storage");
                RegisterPrototypeTypeMapping<IStructureRampart, NativeStructureRampart>("StructureRampart", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "rampart");
                RegisterPrototypeTypeMapping<IStructureTower, NativeStructureTower>("StructureTower", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "tower");
                RegisterPrototypeTypeMapping<IStructureLink, NativeStructureLink>("StructureLink", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "link");
                RegisterPrototypeTypeMapping<IStructureTerminal, NativeStructureTerminal>("StructureTerminal", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "terminal");
                RegisterPrototypeTypeMapping<IStructureExtractor, NativeStructureExtractor>("StructureExtractor", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "extractor");
                RegisterPrototypeTypeMapping<IStructureFactory, NativeStructureFactory>("StructureFactory", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "factory");
                RegisterPrototypeTypeMapping<IStructureInvaderCore, NativeStructureInvaderCore>("StructureInvaderCore", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "invaderCore");
                RegisterPrototypeTypeMapping<IStructureKeeperLair, NativeStructureKeeperLair>("StructureKeeperLair", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "keeperLair");
                RegisterPrototypeTypeMapping<IStructureLab, NativeStructureLab>("StructureLab", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "lab");
                RegisterPrototypeTypeMapping<IStructureNuker, NativeStructureNuker>("StructureNuker", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "nuker");
                RegisterPrototypeTypeMapping<IStructureObserver, NativeStructureObserver>("StructureObserver", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "observer");
                RegisterPrototypeTypeMapping<IStructurePowerBank, NativeStructurePowerBank>("StructurePowerBank", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "powerBank");
                RegisterPrototypeTypeMapping<IStructurePowerSpawn, NativeStructurePowerSpawn>("StructurePowerSpawn", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "powerSpawn");
                RegisterPrototypeTypeMapping<IStructurePortal, NativeStructurePortal>("StructurePortal", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "portal");
                RegisterPrototypeTypeMapping<IOwnedStructure, NativeOwnedStructure>("OwnedStructure", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure");
                RegisterPrototypeTypeMapping<IStructureRoad, NativeStructureRoad>("StructureRoad", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "road");
                RegisterPrototypeTypeMapping<IStructureWall, NativeStructureWall>("StructureWall", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "constructedWall");
                RegisterPrototypeTypeMapping<IStructure, NativeStructure>("Structure", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure");
                RegisterPrototypeTypeMapping<ISource, NativeSource>("Source", FindConstant.Sources, null, null, "source", "source");
                RegisterPrototypeTypeMapping<IMineral, NativeMineral>("Mineral", FindConstant.Minerals, null, null, "mineral", "mineral");
                RegisterPrototypeTypeMapping<IDeposit, NativeDeposit>("Deposit", FindConstant.Deposits, null, null, "deposit", "deposit");
                RegisterPrototypeTypeMapping<INuke, NativeNuke>("Nuke", FindConstant.Nukes, null, null, "nuke");
                RegisterPrototypeTypeMapping<ICreep, NativeCreep>("Creep", FindConstant.Creeps, FindConstant.MyCreeps, FindConstant.HostileCreeps, "powerCreep", "powerCreep");
                RegisterPrototypeTypeMapping<IPowerCreep, NativePowerCreep>("PowerCreep", FindConstant.PowerCreeps, FindConstant.MyPowerCreeps, FindConstant.HostilePowerCreeps, "creep", "creep");
                RegisterPrototypeTypeMapping<IFlag, NativeFlag>("Flag", FindConstant.Flags, null, null, "flag");
                RegisterPrototypeTypeMapping<IResource, NativeResource>("Resource", FindConstant.DroppedResources, null, null, "resource");
                RegisterPrototypeTypeMapping<IConstructionSite, NativeConstructionSite>("ConstructionSite", FindConstant.ConstructionSites, FindConstant.MyConstructionSites, FindConstant.HostileConstructionSites, "constructionSite");
                RegisterPrototypeTypeMapping<ITombstone, NativeTombstone>("Tombstone", FindConstant.Tombstones, null, null, "tombstone");
                RegisterPrototypeTypeMapping<IRuin, NativeRuin>("Ruin", FindConstant.Ruins, null, null, "ruin");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class NativeRoomObjectFactory
    {
        private static Dictionary<Type, Func<INativeRoot, JSObject, IRoomObject>> factoryFuncMap = [];

        internal static class FactoryFuncDb<T> where T : IRoomObject
        {
            public static Func<INativeRoot, JSObject, T>? FactoryFunc { get; set; }
        }

        private static void RegisterFactoryFunc<T>(Func<INativeRoot, JSObject, T> factoryFunc) where T : class, IRoomObject
        {
            FactoryFuncDb<T>.FactoryFunc = factoryFunc;
            factoryFuncMap.Add(typeof(T), factoryFunc);
        }

        static NativeRoomObjectFactory()
        {
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureSpawn(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureContainer(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureController(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureExtension(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureStorage(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureRampart(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureTower(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureLink(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureTerminal(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureExtractor(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureFactory(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureInvaderCore(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureKeeperLair(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureLab(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureNuker(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureObserver(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructurePowerBank(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructurePowerSpawn(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructurePortal(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeOwnedStructure(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureRoad(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructureWall(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeStructure(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeSource(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeMineral(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeDeposit(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeNuke(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeCreep(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativePowerCreep(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeFlag(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeResource(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeConstructionSite(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeTombstone(nativeRoot, proxyObject));
            RegisterFactoryFunc(static (nativeRoot, proxyObject) => new NativeRuin(nativeRoot, proxyObject));
        }

        public static T Create<T>(INativeRoot nativeRoot, JSObject proxyObject) where T : IRoomObject
        {
            var createFunc = FactoryFuncDb<T>.FactoryFunc ?? throw new InvalidOperationException($"Factory function for '{typeof(T)}' not found");
            return createFunc(nativeRoot, proxyObject);
        }

        public static IRoomObject Create(Type roomObjectType, INativeRoot nativeRoot, JSObject proxyObject)
        {
            if (!factoryFuncMap.TryGetValue(roomObjectType, out var createFunc)) { throw new InvalidOperationException($"Factory function for '{roomObjectType}' not found"); }
            return createFunc(nativeRoot, proxyObject);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class NativeRoomObjectExtensions
    {
        public static JSObject ToJS(this IRoomObject roomObject)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            => (roomObject as NativeRoomObject).ProxyObject;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        public static DateTime ToDateTime(this JSObject obj)
            => DateTime.UnixEpoch + TimeSpan.FromSeconds(NativeRoomObjectUtils.InterpretDateTime(obj));
    }
}
