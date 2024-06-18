using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ScreepsDotNet.API.World;
using ScreepsDotNet.Interop;

namespace ScreepsDotNet.Native.World
{
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
    internal class NativeRoomObjectType
    (
        string prototypeName,
        FindConstant? findConstant,
        FindConstant? myFindConstant,
        FindConstant? hostileFindConstant,
        Name? lookConstant,
        Name? structureConstant,
        Func<INativeRoot, JSObject, IRoomObject>? factoryFunc,
        Type interfaceType,
        Type concreteType
    ) : IEquatable<NativeRoomObjectType>
    {
        public readonly string PrototypeName = prototypeName;
        public readonly FindConstant? FindConstant = findConstant;
        public readonly FindConstant? MyFindConstant = myFindConstant;
        public readonly FindConstant? HostileFindConstant = hostileFindConstant;
        public readonly Name? LookConstant = lookConstant;
        public readonly Name? StructureConstant = structureConstant;
        public readonly Func<INativeRoot, JSObject, IRoomObject>? FactoryFunc = factoryFunc;
        public readonly Type InterfaceType = interfaceType;
        public readonly Type ConcreteType = concreteType;

        private readonly int hashCode = prototypeName.GetHashCode();

        public NativeRoomObjectType? BaseType { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeRoomObject? CreateInstance(INativeRoot nativeRoot, JSObject proxyObject)
            => FactoryFunc?.Invoke(nativeRoot, proxyObject) as NativeRoomObject;

        public bool IsAssignableTo(NativeRoomObjectType otherType)
        {
            // X is assignable to Y if X : Y
            // e.g. an IStructureSpawn is assignable to IOwnedStructure because IStructureSpawn : IOwnedStructure
            var testType = this;
            do
            {
                if (testType == otherType) { return true; }
                testType = testType.BaseType;
            }
            while (testType is not null);
            return false;
        }

        public bool IsAssignableFrom(NativeRoomObjectType otherType) => otherType.IsAssignableTo(this);

        public override int GetHashCode() => hashCode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NativeRoomObjectType lhs, NativeRoomObjectType rhs) => lhs.hashCode == rhs.hashCode && lhs.PrototypeName == rhs.PrototypeName;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NativeRoomObjectType lhs, NativeRoomObjectType rhs) => !(lhs == rhs);

        public override string ToString() => PrototypeName;

        public override bool Equals(object? obj) => obj is NativeRoomObjectType other && this == other;

        public bool Equals(NativeRoomObjectType? other) => other is not null && this == other;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class NativeRoomObjectTypes
    {
        #region Imports

        [JSImport("getPrototypes", "game")]
        internal static partial JSObject GetPrototypesObject();

        #endregion

        private static class TypeContainer<T> where T : IRoomObject
        {
            public static int TypeId;
            public static JSObject? ConstructorObj;
            public static NativeRoomObjectType? Type;
            public static Func<INativeRoot, JSObject, T>? FactoryFunc;
        }

        private static readonly Name TypeIdKey = Name.Create("__dotnet_typeId");

        private static JSObject? prototypesObject;
        private static readonly List<NativeRoomObjectType> typeIdToType = [];
        private static readonly Dictionary<string, NativeRoomObjectType> structureConstantToType = [];
        private static readonly Dictionary<Type, NativeRoomObjectType> interfaceTypeToType = [];

        private static bool didRegisterTypes;

        private static NativeRoomObjectType RegisterType<TInterface, TConcrete>
        (
            string prototypeName,
            FindConstant? findConstant,
            FindConstant? myFindConstant,
            FindConstant? hostileFindConstant,
            string? lookConstant,
            string? structureConstant,
            Func<INativeRoot, JSObject, TConcrete>? factoryFunc
        ) where TInterface : IRoomObject where TConcrete : NativeRoomObject
        {
            var type = new NativeRoomObjectType(prototypeName, findConstant, myFindConstant, hostileFindConstant, Name.CreateNullSafe(lookConstant), Name.CreateNullSafe(structureConstant), factoryFunc, typeof(TInterface), typeof(TConcrete));
            var constructor = prototypesObject!.GetPropertyAsJSObject(type.PrototypeName);
            if (constructor == null)
            {
                Console.WriteLine($"Failed to retrieve constructor for '{type}'");
                return type;
            }
            var existingTypeId = constructor.TryGetPropertyAsInt32(TypeIdKey);
            if (existingTypeId != null)
            {
                Console.WriteLine($"Constructor for '{type}' was already bound to {typeIdToType[existingTypeId.Value]}");
                return type;
            }
            int typeId = typeIdToType.Count;
            constructor.SetProperty(TypeIdKey, typeId);
            typeIdToType.Add(type);
            TypeContainer<TInterface>.TypeId = typeId;
            TypeContainer<TInterface>.ConstructorObj = constructor;
            TypeContainer<TInterface>.Type = type;
            TypeContainer<TConcrete>.TypeId = typeId;
            TypeContainer<TConcrete>.ConstructorObj = constructor;
            TypeContainer<TConcrete>.Type = type;
            TypeContainer<TConcrete>.FactoryFunc = factoryFunc;
            interfaceTypeToType.Add(typeof(TInterface), type);
            if (!string.IsNullOrEmpty(structureConstant))
            {
                structureConstantToType.Add(structureConstant, type);
            }
            return type;
        }

        public static void RegisterTypesIfNeeded()
        {
            if (didRegisterTypes) { return; }
            try
            {
                RegisterTypes();
            }
            finally
            {
                didRegisterTypes = true;
            }
        }

        private static void RegisterTypes()
        {
            prototypesObject = GetPrototypesObject();
            try
            {
                // NOTE - order matters! We must do derived classes first and base classes last
                // This is to avoid a nasty bug where the runtime gets objects and their prototype objects mixed up due to the tracking id being set as a property on the object
                var structureSpawnType = RegisterType<IStructureSpawn, NativeStructureSpawn>("StructureSpawn", FindConstant.Structures, FindConstant.MySpawns, FindConstant.HostileSpawns, "structure", "spawn", static (x, y) => new NativeStructureSpawn(x, y));
                var structureContainerType = RegisterType<IStructureContainer, NativeStructureContainer>("StructureContainer", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "container", static (x, y) => new NativeStructureContainer(x, y));
                var structureControllerType = RegisterType<IStructureController, NativeStructureController>("StructureController", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "controller", static (x, y) => new NativeStructureController(x, y));
                var structureExtensionType = RegisterType<IStructureExtension, NativeStructureExtension>("StructureExtension", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "extension", static (x, y) => new NativeStructureExtension(x, y));
                var structureStorageType = RegisterType<IStructureStorage, NativeStructureStorage>("StructureStorage", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "storage", static (x, y) => new NativeStructureStorage(x, y));
                var structureRampartType = RegisterType<IStructureRampart, NativeStructureRampart>("StructureRampart", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "rampart", static (x, y) => new NativeStructureRampart(x, y));
                var structureTowerType = RegisterType<IStructureTower, NativeStructureTower>("StructureTower", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "tower", static (x, y) => new NativeStructureTower(x, y));
                var structureLinkType = RegisterType<IStructureLink, NativeStructureLink>("StructureLink", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "link", static (x, y) => new NativeStructureLink(x, y));
                var structureTerminalType = RegisterType<IStructureTerminal, NativeStructureTerminal>("StructureTerminal", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "terminal", static (x, y) => new NativeStructureTerminal(x, y));
                var structureExtractorType = RegisterType<IStructureExtractor, NativeStructureExtractor>("StructureExtractor", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "extractor", static (x, y) => new NativeStructureExtractor(x, y));
                var structureFactoryType = RegisterType<IStructureFactory, NativeStructureFactory>("StructureFactory", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "factory", static (x, y) => new NativeStructureFactory(x, y));
                var structureInvaderCoreType = RegisterType<IStructureInvaderCore, NativeStructureInvaderCore>("StructureInvaderCore", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "invaderCore", static (x, y) => new NativeStructureInvaderCore(x, y));
                var structureKeeperLairType = RegisterType<IStructureKeeperLair, NativeStructureKeeperLair>("StructureKeeperLair", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "keeperLair", static (x, y) => new NativeStructureKeeperLair(x, y));
                var structureLabType = RegisterType<IStructureLab, NativeStructureLab>("StructureLab", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "lab", static (x, y) => new NativeStructureLab(x, y));
                var structureNukerType = RegisterType<IStructureNuker, NativeStructureNuker>("StructureNuker", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "nuker", static (x, y) => new NativeStructureNuker(x, y));
                var structureObserverType = RegisterType<IStructureObserver, NativeStructureObserver>("StructureObserver", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "observer", static (x, y) => new NativeStructureObserver(x, y));
                var structurePowerBankType = RegisterType<IStructurePowerBank, NativeStructurePowerBank>("StructurePowerBank", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "powerBank", static (x, y) => new NativeStructurePowerBank(x, y));
                var structurePowerSpawnType = RegisterType<IStructurePowerSpawn, NativeStructurePowerSpawn>("StructurePowerSpawn", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "powerSpawn", static (x, y) => new NativeStructurePowerSpawn(x, y));
                var structurePortalType = RegisterType<IStructurePortal, NativeStructurePortal>("StructurePortal", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "portal", static (x, y) => new NativeStructurePortal(x, y));
                var ownedStructureType = RegisterType<IOwnedStructure, NativeOwnedStructure>("OwnedStructure", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", null, static (x, y) => new NativeOwnedStructure(x, y));
                var structureRoadType = RegisterType<IStructureRoad, NativeStructureRoad>("StructureRoad", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "road", static (x, y) => new NativeStructureRoad(x, y));
                var structureWallType = RegisterType<IStructureWall, NativeStructureWall>("StructureWall", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", "constructedWall", static (x, y) => new NativeStructureWall(x, y));
                var structureType = RegisterType<IStructure, NativeStructure>("Structure", FindConstant.Structures, FindConstant.MyStructures, FindConstant.HostileStructures, "structure", null, static (x, y) => new NativeStructure(x, y));
                var sourceType = RegisterType<ISource, NativeSource>("Source", FindConstant.Sources, null, null, "source", "source", static (x, y) => new NativeSource(x, y));
                var mineralType = RegisterType<IMineral, NativeMineral>("Mineral", FindConstant.Minerals, null, null, "mineral", "mineral", static (x, y) => new NativeMineral(x, y));
                var depositType = RegisterType<IDeposit, NativeDeposit>("Deposit", FindConstant.Deposits, null, null, "deposit", "deposit", static (x, y) => new NativeDeposit(x, y));
                var nukeType = RegisterType<INuke, NativeNuke>("Nuke", FindConstant.Nukes, null, null, "nuke", null, static (x, y) => new NativeNuke(x, y));
                var creepType = RegisterType<ICreep, NativeCreep>("Creep", FindConstant.Creeps, FindConstant.MyCreeps, FindConstant.HostileCreeps, "powerCreep", "powerCreep", static (x, y) => new NativeCreep(x, y));
                var powerCreepType = RegisterType<IPowerCreep, NativePowerCreep>("PowerCreep", FindConstant.PowerCreeps, FindConstant.MyPowerCreeps, FindConstant.HostilePowerCreeps, "creep", "creep", static (x, y) => new NativePowerCreep(x, y));
                var flagType = RegisterType<IFlag, NativeFlag>("Flag", FindConstant.Flags, null, null, "flag", null, static (x, y) => new NativeFlag(x, y));
                var resourceType = RegisterType<IResource, NativeResource>("Resource", FindConstant.DroppedResources, null, null, "resource", null, static (x, y) => new NativeResource(x, y));
                var constructionSiteType = RegisterType<IConstructionSite, NativeConstructionSite>("ConstructionSite", FindConstant.ConstructionSites, FindConstant.MyConstructionSites, FindConstant.HostileConstructionSites, "constructionSite", null, static (x, y) => new NativeConstructionSite(x, y));
                var tombstoneType = RegisterType<ITombstone, NativeTombstone>("Tombstone", FindConstant.Tombstones, null, null, "tombstone", null, static (x, y) => new NativeTombstone(x, y));
                var ruinType = RegisterType<IRuin, NativeRuin>("Ruin", FindConstant.Ruins, null, null, "ruin", null, static (x, y) => new NativeRuin(x, y));
                var roomObjectType = RegisterType<IRoomObject, NativeRoomObject>("RoomObject", null, null, null, null, null, null);

                structureSpawnType.BaseType = ownedStructureType;
                structureContainerType.BaseType = ownedStructureType;
                structureControllerType.BaseType = ownedStructureType;
                structureExtensionType.BaseType = ownedStructureType;
                structureStorageType.BaseType = ownedStructureType;
                structureRampartType.BaseType = ownedStructureType;
                structureTowerType.BaseType = ownedStructureType;
                structureLinkType.BaseType = ownedStructureType;
                structureTerminalType.BaseType = ownedStructureType;
                structureExtractorType.BaseType = ownedStructureType;
                structureFactoryType.BaseType = ownedStructureType;
                structureInvaderCoreType.BaseType = ownedStructureType;
                structureKeeperLairType.BaseType = ownedStructureType;
                structureLabType.BaseType = ownedStructureType;
                structureNukerType.BaseType = ownedStructureType;
                structureObserverType.BaseType = ownedStructureType;
                structurePowerBankType.BaseType = ownedStructureType;
                structurePowerSpawnType.BaseType = ownedStructureType;
                structurePortalType.BaseType = ownedStructureType;

                ownedStructureType.BaseType = structureType;
                structureRoadType.BaseType = structureType;
                structureWallType.BaseType = structureType;

                structureType.BaseType = roomObjectType;
                sourceType.BaseType = roomObjectType;
                mineralType.BaseType = roomObjectType;
                depositType.BaseType = roomObjectType;
                nukeType.BaseType = roomObjectType;
                creepType.BaseType = roomObjectType;
                powerCreepType.BaseType = roomObjectType;
                flagType.BaseType = roomObjectType;
                resourceType.BaseType = roomObjectType;
                constructionSiteType.BaseType = roomObjectType;
                tombstoneType.BaseType = roomObjectType;
                ruinType.BaseType = roomObjectType;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeRoomObjectType TypeOf<T>() where T : IRoomObject
            => TypeContainer<T>.Type is null ? throw new InvalidOperationException($"Failed to retrieve NativeRoomObjectType for {typeof(T)}") : TypeContainer<T>.Type;

        public static NativeRoomObjectType? GetWrapperTypeForConstructor(JSObject constructor)
        {
            int? typeId = constructor.TryGetPropertyAsInt32(TypeIdKey);
            if (typeId == null || typeId < 0 || typeId >= typeIdToType.Count)
            {
                Console.WriteLine($"Failed to retrieve wrapper type for {constructor} - typeId {typeId} not found");
                return null;
            }
            return typeIdToType[typeId.Value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeRoomObjectType? GetWrapperTypeForObject(JSObject proxyObject) => GetWrapperTypeForConstructor(JSUtils.GetConstructorOf(proxyObject));

        public static NativeRoomObject? CreateWrapperForRoomObject(INativeRoot nativeRoot, JSObject proxyObject, NativeRoomObjectType expectedType)
        {
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType is null) { return null; }
            if (!wrapperType.IsAssignableTo(expectedType)) { return null; }
            if (wrapperType.FactoryFunc == null) { return null; }
            return wrapperType.CreateInstance(nativeRoot, proxyObject);
        }

        public static T? CreateWrapperForRoomObject<T>(INativeRoot nativeRoot, JSObject proxyObject) where T : class, IRoomObject
        {
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType is null) { return null; }
            var type = TypeOf<T>();
            if (!wrapperType.IsAssignableTo(type)) { return null; }
            return wrapperType.FactoryFunc?.Invoke(nativeRoot, proxyObject) as T;
        }

        public static NativeRoomObject? CreateWrapperForRoomObject(INativeRoot nativeRoot, JSObject proxyObject, in RoomObjectMetadata metadata, NativeRoomObjectType expectedType)
        {
            var wrapperType = typeIdToType[metadata.TypeId];
            if (!wrapperType.IsAssignableTo(expectedType)) { return null; }
            return wrapperType.CreateInstance(nativeRoot, proxyObject);
        }

        public static T? CreateWrapperForRoomObject<T>(INativeRoot nativeRoot, JSObject proxyObject, in RoomObjectMetadata metadata) where T : class, IRoomObject
        {
            var wrapperType = typeIdToType[metadata.TypeId];
            if (!wrapperType.IsAssignableTo(TypeOf<T>())) { return null; }
            return wrapperType.CreateInstance(nativeRoot, proxyObject) as T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeRoomObjectType? GetTypeForStructureConstant(string structureConstant)
            => structureConstantToType.TryGetValue(structureConstant, out var result) ? result : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeRoomObjectType? GetTypeForInterfaceType(Type interfaceType)
            => interfaceTypeToType.TryGetValue(interfaceType, out var result) ? result : null;

        static NativeRoomObjectTypes()
        {
            RegisterTypesIfNeeded();
        }

    }
}
