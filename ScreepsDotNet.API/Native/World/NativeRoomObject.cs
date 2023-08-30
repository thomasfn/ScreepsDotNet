using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal abstract partial class NativeRoomObject : NativeObject, IRoomObject
    {
        #region Imports

        [JSImport("RoomObject.getEncodedRoomPosition", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetEncodedRoomPosition([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] IntPtr outPtr);

        #endregion

        protected RoomPosition? positionCache;

        protected virtual bool CanMove { get => false; }

        public IEnumerable<Effect> Effects => throw new NotImplementedException();

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
                return new NativeRoom(nativeRoot, roomObj);
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

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class NativeRoomObjectPrototypes<T> where T : IRoomObject
    {
        public static int TypeId;
        public static JSObject? ConstructorObj;
        public static FindConstant? FindConstant;
        public static string? LookConstant;
        public static string? StructureConstant;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeRoomObjectUtils))]
        static NativeRoomObjectPrototypes()
        {
            RuntimeHelpers.RunClassConstructor(typeof(NativeRoomObjectUtils).TypeHandle);
        }
    }

    internal static partial class NativeCopyBuffer
    {
        #region Imports

        [JSImport("getMaxSize", "copybuffer")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_GetMaxSize();

        [JSImport("read", "copybuffer")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Read([JSMarshalAs<JSType.MemoryView>] Span<byte> data);

        [JSImport("write", "copybuffer")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_Write([JSMarshalAs<JSType.MemoryView>] Span<byte> data);

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

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static partial class NativeRoomObjectUtils
    {
        private const string TypeIdKey = "__dotnet_typeId";

        private static readonly JSObject prototypesObject;
        private static readonly IList<Type> prototypeTypeMappings = new List<Type>();
        private static readonly IDictionary<Type, string> prototypeNameMappings = new Dictionary<Type, string>();
        private static readonly IDictionary<string, Type> structureConstantInterfaceMap = new Dictionary<string, Type>();
        private static readonly IDictionary<Type, string> interfaceStructureConstantMap = new Dictionary<Type, string>();

        #region Imports

        [JSImport("getPrototypes", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetPrototypesObject();

        [JSImport("createRoomPosition", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject CreateRoomPosition([JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y, [JSMarshalAs<JSType.String>] string roomName);

        [JSImport("interpretDateTime", "object")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial double InterpretDateTime([JSMarshalAs<JSType.Object>] JSObject obj);

        #endregion

        internal static void RegisterPrototypeTypeMapping<TInterface, TConcrete>(string prototypeName, FindConstant? findConstant = null, string? lookConstant = null, string? structureConstant = null)
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
            NativeRoomObjectPrototypes<TInterface>.LookConstant = lookConstant;
            NativeRoomObjectPrototypes<TInterface>.StructureConstant = structureConstant;
            NativeRoomObjectPrototypes<TConcrete>.TypeId = typeId;
            NativeRoomObjectPrototypes<TConcrete>.ConstructorObj = constructor;
            NativeRoomObjectPrototypes<TConcrete>.FindConstant = findConstant;
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
            int typeId = constructor.GetPropertyAsInt32(TypeIdKey) - 1;
            if (typeId < 0)
            {
                Console.WriteLine($"Failed to retrieve wrapper type for {constructor} - typeId not found");
                return null;
            }
            return prototypeTypeMappings[typeId];
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
            var obj = Activator.CreateInstance(wrapperType, new object?[] { nativeRoot, null, dataPacket.ObjectId }) as NativeRoomObject;
            if (obj == null) { return null; }
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
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeOwnedStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureRoad))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureWall))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeSource))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeMineral))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeCreep))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeFlag))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeResource))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeConstructionSite))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeTombstone))]
        static NativeRoomObjectUtils()
        {
            prototypesObject = GetPrototypesObject();
            try
            {
                // NOTE - order matters! We must do derived classes first and base classes last
                // This is to avoid a nasty bug where the runtime gets objects and their prototype objects mixed up due to the tracking id being set as a property on the object
                RegisterPrototypeTypeMapping<IStructureSpawn, NativeStructureSpawn>("StructureSpawn", FindConstant.Structures, "structure", "spawn");
                RegisterPrototypeTypeMapping<IStructureContainer, NativeStructureContainer>("StructureContainer", FindConstant.Structures, "structure", "container");
                RegisterPrototypeTypeMapping<IStructureController, NativeStructureController>("StructureController", FindConstant.Structures, "structure", "controller");
                RegisterPrototypeTypeMapping<IStructureExtension, NativeStructureExtension>("StructureExtension", FindConstant.Structures, "structure", "extension");
                RegisterPrototypeTypeMapping<IStructureStorage, NativeStructureStorage>("StructureStorage", FindConstant.Structures, "structure", "storage");
                RegisterPrototypeTypeMapping<IStructureRampart, NativeStructureRampart>("StructureRampart", FindConstant.Structures, "structure", "rampart");
                RegisterPrototypeTypeMapping<IStructureTower, NativeStructureTower>("StructureTower", FindConstant.Structures, "structure", "tower");
                RegisterPrototypeTypeMapping<IStructureLink, NativeStructureLink>("StructureLink", FindConstant.Structures, "structure", "link");
                RegisterPrototypeTypeMapping<IStructureTerminal, NativeStructureTerminal>("StructureTerminal", FindConstant.Structures, "structure", "terminal");
                RegisterPrototypeTypeMapping<IStructureExtractor, NativeStructureExtractor>("StructureExtractor", FindConstant.Structures, "structure", "extractor");
                RegisterPrototypeTypeMapping<IOwnedStructure, NativeOwnedStructure>("OwnedStructure", FindConstant.Structures, "structure");
                RegisterPrototypeTypeMapping<IStructureRoad, NativeStructureRoad>("StructureRoad", FindConstant.Structures, "structure", "road");
                RegisterPrototypeTypeMapping<IStructureWall, NativeStructureWall>("StructureWall", FindConstant.Structures, "structure", "constructedWall");
                RegisterPrototypeTypeMapping<IStructure, NativeStructure>("Structure", FindConstant.Structures, "structure");
                RegisterPrototypeTypeMapping<ISource, NativeSource>("Source", FindConstant.Sources, "source");
                RegisterPrototypeTypeMapping<IMineral, NativeMineral>("Mineral", FindConstant.Minerals, "mineral");
                RegisterPrototypeTypeMapping<ICreep, NativeCreep>("Creep", FindConstant.Creeps, "creep");
                RegisterPrototypeTypeMapping<IFlag, NativeFlag>("Flag", FindConstant.Flags, "flag");
                RegisterPrototypeTypeMapping<IResource, NativeResource>("Resource", FindConstant.DroppedResources, "resource");
                RegisterPrototypeTypeMapping<IConstructionSite, NativeConstructionSite>("ConstructionSite", FindConstant.ConstructionSites, "constructionSite");
                RegisterPrototypeTypeMapping<ITombstone, NativeTombstone>("Tombstone", FindConstant.Tombstones, "tombstone");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
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
