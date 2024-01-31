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
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal abstract partial class NativeRoomObject : NativeObject, IRoomObject
    {
        #region Imports

        [JSImport("RoomObject.getEncodedRoomPosition", "game/prototypes/wrapped")]
        
        internal static partial void Native_GetEncodedRoomPosition(JSObject proxyObject, IntPtr outPtr);

        [JSImport("getProperty", "__object")]

        internal static partial JSObject[] Native_GetEffects(JSObject proxyObject, string key);

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
                var roomObj = ProxyObject.GetPropertyAsJSObject("room");
                if (roomObj == null) { return null; }
                return nativeRoot.GetRoomByProxyObject(roomObj);
            }
        }

        public NativeRoomObject(INativeRoot nativeRoot, JSObject? proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        public override void UpdateFromDataPacket(RoomObjectDataPacket dataPacket)
        {
            base.UpdateFromDataPacket(dataPacket);
            positionCache = dataPacket.RoomPos;
        }

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

        private RoomPosition FetchRoomPosition()
        {
            Span<byte> buffer = stackalloc byte[32];
            unsafe
            {
                fixed (byte* p = buffer)
                {
                    Native_GetEncodedRoomPosition(ProxyObject, (IntPtr)p);
                    ScreepsDotNet_Native.DecodeRoomPosition(p, p + 16);
                }
            }
            return MemoryMarshal.Cast<byte, RoomPosition>(buffer[16..])[0];
        }

        private Effect[] FetchEffects()
        {
            var effectsArr = Native_GetEffects(ProxyObject, "effects");
            var result = new Effect[effectsArr.Length];
            try
            {
                for (int i = 0; i < effectsArr.Length; ++i)
                {
                    var obj = effectsArr[i];
                    result[i] = new(
                        effectType: (EffectType)obj.GetPropertyAsInt32("effect"),
                        level: obj.TryGetPropertyAsInt32("level"),
                        ticksRemaining: obj.GetPropertyAsInt32("ticksRemaining")
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

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeRoomObjectUtils))]
        static NativeRoomObjectPrototypes()
        {
            RuntimeHelpers.RunClassConstructor(typeof(NativeRoomObjectUtils).TypeHandle);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class NativeCopyBuffer
    {
        #region Imports

        [JSImport("getMaxSize", "copybuffer")]
        
        internal static partial int Native_GetMaxSize();

        [JSImport("read", "copybuffer")]
        
        internal static partial int Native_Read(Span<byte> data);

        [JSImport("write", "copybuffer")]
        
        internal static partial int Native_Write(Span<byte> data);

        #endregion

        private static readonly byte[] copyBuffer;

        static NativeCopyBuffer()
        {
            copyBuffer = new byte[Native_GetMaxSize()];
        }

        public static ReadOnlySpan<byte> ReadFromJS()
        {
            int len = Native_Read(copyBuffer);
            return copyBuffer.AsSpan()[..len];
        }

        public static void WriteToJS(ReadOnlySpan<byte> data)
        {
            data.CopyTo(copyBuffer);
            Native_Write(copyBuffer);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0, Size = 48)]
    internal readonly struct RoomObjectDataPacket
    {
        public const int SizeInBytes = 48;

        static RoomObjectDataPacket()
        {
            if (Marshal.SizeOf<RoomObjectDataPacket>() != SizeInBytes)
            {
                throw new Exception($"Expected size of RoomObjectDataPacket to be {SizeInBytes}b, got {Marshal.SizeOf<RoomObjectDataPacket>()}b");
            }
        }

        public readonly ObjectId ObjectId;
        public readonly int TypeId;
        public readonly int Flags;
        public readonly int Hits;
        public readonly int HitsMax;
        public readonly RoomPosition RoomPos;

        public bool My => (Flags & 1) == 1;

        public override string ToString()
            => $"({ObjectId}, {TypeId}, {Flags}, {Hits}, {HitsMax}, {RoomPos})";
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class NativeRoomObjectUtils
    {
        private const string TypeIdKey = "__dotnet_typeId";

        private static readonly JSObject prototypesObject;
        private static readonly List<Type> prototypeTypeMappings = new();
        private static readonly Dictionary<Type, string> prototypeNameMappings = new();
        private static readonly Dictionary<string, Type> structureConstantInterfaceMap = new();
        private static readonly Dictionary<Type, string> interfaceStructureConstantMap = new();

        #region Imports

        [JSImport("getPrototypes", "game")]
        
        internal static partial JSObject GetPrototypesObject();

        [JSImport("createRoomPosition", "game")]
        
        internal static partial JSObject CreateRoomPosition(int x, int y, string roomName);

        [JSImport("interpretDateTime", "object")]
        
        internal static partial double InterpretDateTime(JSObject obj);

        #endregion

        internal static void RegisterPrototypeTypeMapping<TInterface, TConcrete>(string prototypeName, FindConstant? findConstant = null, FindConstant? myFindConstant = null, FindConstant? hostileFindConstant = null, string? lookConstant = null, string? structureConstant = null)
            where TInterface : IRoomObject
            where TConcrete : NativeRoomObject
        {
            var constructor = prototypesObject.GetPropertyAsJSObject(prototypeName);
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

        internal static NativeRoomObject? CreateWrapperForRoomObject(INativeRoot nativeRoot, JSObject? proxyObject, Type expectedType)
        {
            if (proxyObject == null) { return null; }
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType == null) { return null; }
            if (!wrapperType.IsAssignableTo(expectedType)) { return null; }
            if (wrapperType.IsAssignableTo(typeof(IWithId)))
            {
                var id = proxyObject.GetPropertyAsString("id");
                if (string.IsNullOrEmpty(id)) { return null; }
                return (Activator.CreateInstance(wrapperType, new object[] { nativeRoot, proxyObject, new ObjectId(id) }) as NativeRoomObject)!;
            }
            else
            {
                return (Activator.CreateInstance(wrapperType, new object[] { nativeRoot, proxyObject }) as NativeRoomObject)!;
            }
        }

        internal static NativeRoomObject? CreateWrapperForRoomObject(INativeRoot nativeRoot, JSObject? proxyObject, Type expectedType, ObjectId id)
        {
            if (proxyObject == null) { return null; }
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType == null) { return null; }
            if (!wrapperType.IsAssignableTo(expectedType)) { return null; }
            return (Activator.CreateInstance(wrapperType, new object[] { nativeRoot, proxyObject, id }) as NativeRoomObject)!;
        }

        internal static T? CreateWrapperForRoomObject<T>(INativeRoot nativeRoot, in RoomObjectDataPacket dataPacket) where T : class, IRoomObject
            => CreateWrapperForRoomObject(nativeRoot, dataPacket, typeof(T)) as T;

        internal static NativeRoomObject? CreateWrapperForRoomObject(INativeRoot nativeRoot, in RoomObjectDataPacket dataPacket, Type expectedType)
        {
            if (!dataPacket.ObjectId.IsValid) { return null; }
            int typeId = dataPacket.TypeId - 1;
            if (typeId < 0) { return null; }
            Type wrapperType = prototypeTypeMappings[typeId];
            if (!wrapperType.IsAssignableTo(expectedType)) { return null; }
            if (Activator.CreateInstance(wrapperType, new object?[] { nativeRoot, null, dataPacket.ObjectId }) is not NativeRoomObject obj) { return null; }
            obj.UpdateFromDataPacket(dataPacket);
            return obj;
        }

        internal static string GetPrototypeName(Type type)
            => prototypeNameMappings[type];

        internal static Type? GetInterfaceTypeForStructureConstant(string structureConstant)
            => structureConstantInterfaceMap.TryGetValue(structureConstant, out var result) ? result : null;

        internal static string? GetStructureConstantForInterfaceType(Type interfaceType)
            => interfaceStructureConstantMap.TryGetValue(interfaceType, out var result) ? result : null;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureSpawn))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureContainer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureController))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureExtension))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureStorage))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureRampart))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureTower))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureLink))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureTerminal))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureExtractor))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureFactory))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureInvaderCore))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureKeeperLair))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureLab))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureNuker))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureObserver))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructurePowerBank))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructurePowerSpawn))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructurePortal))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeOwnedStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureRoad))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureWall))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeSource))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeMineral))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeDeposit))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeNuke))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeCreep))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeFlag))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeResource))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeConstructionSite))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeTombstone))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeRuin))]
        static NativeRoomObjectUtils()
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
                RegisterPrototypeTypeMapping<ICreep, NativeCreep>("Creep", FindConstant.Creeps, FindConstant.MyCreeps, FindConstant.HostileCreeps, "creep", "creep");
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
