using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal abstract class NativeRoomObject : NativeObject, IRoomObject
    {
        private RoomPosition? positionCache;

        public bool Exists => proxyObjectOrNull != null;

        public IEnumerable<Effect> Effects => throw new NotImplementedException();

        public RoomPosition Position => positionCache ??= ProxyObject.GetPropertyAsJSObject("pos")!.ToRoomPosition();

        public IRoom? Room
        {
            get
            {
                var roomObj = ProxyObject.GetPropertyAsJSObject("room");
                if (roomObj == null) { return null; }
                return new NativeRoom(nativeRoot, roomObj);
            }
        }

        public NativeRoomObject(INativeRoot nativeRoot, JSObject proxyObject)
            : base(nativeRoot, proxyObject)
        { }

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            positionCache = null;
        }

        public override string ToString()
            => $"NativeRoomObject[{proxyObjectOrNull}]";
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
        public static JSObject? ConstructorObj;
        public static FindConstant? FindConstant;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeRoomObjectUtils))]
        static NativeRoomObjectPrototypes()
        {
            RuntimeHelpers.RunClassConstructor(typeof(NativeRoomObjectUtils).TypeHandle);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static partial class NativeRoomObjectUtils
    {
        private const string TypeIdKey = "__dotnet_typeId";

        private static readonly JSObject prototypesObject;
        private static readonly IList<Type> prototypeTypeMappings = new List<Type>();
        private static readonly IDictionary<Type, string> prototypeNameMappings = new Dictionary<Type, string>();

        [JSImport("getPrototypes", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetPrototypesObject();

        [JSImport("createRoomPosition", "game")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject CreateRoomPosition([JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y, [JSMarshalAs<JSType.String>] string roomName);

        [JSImport("getConstructorOf", "object")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject GetConstructorOf([JSMarshalAs<JSType.Object>] JSObject obj);

        [JSImport("create", "object")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject CreateObject([JSMarshalAs<JSType.Object>] JSObject? prototype);

        [JSImport("set", "object")]
        internal static partial void SetObjectArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key, [JSMarshalAs<JSType.Array<JSType.Object>>] JSObject[] val);

        [JSImport("get", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[]? GetObjectArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key);

        [JSImport("set", "object")]
        internal static partial void SetIntArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key, [JSMarshalAs<JSType.Array<JSType.Number>>] int[] val);

        [JSImport("get", "object")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Number>>]
        internal static partial int[]? GetIntArrayOnObject([JSMarshalAs<JSType.Object>] JSObject obj, [JSMarshalAs<JSType.String>] string key);

        [JSImport("interpretDateTime", "object")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial double InterpretDateTime([JSMarshalAs<JSType.Object>] JSObject obj);

        internal static void RegisterPrototypeTypeMapping<TInterface, TConcrete>(string prototypeName, FindConstant? findConstant)
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
            NativeRoomObjectPrototypes<TInterface>.ConstructorObj = constructor;
            NativeRoomObjectPrototypes<TInterface>.FindConstant = findConstant;
            NativeRoomObjectPrototypes<TConcrete>.ConstructorObj = constructor;
            NativeRoomObjectPrototypes<TConcrete>.FindConstant = findConstant;
            prototypeNameMappings.Add(typeof(TInterface), prototypeName);
            prototypeNameMappings.Add(typeof(TConcrete), prototypeName);
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
            => GetWrapperTypeForConstructor(GetConstructorOf(jsObject));

        internal static NativeRoomObject? CreateWrapperForRoomObject(INativeRoot nativeRoot, JSObject? proxyObject, Type expectedType)
        {
            if (proxyObject == null) { return null; }
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType == null) { return null; }
            if (!wrapperType.IsAssignableTo(expectedType)) { return null; }
            return (Activator.CreateInstance(wrapperType, new object[] { nativeRoot, proxyObject }) as NativeRoomObject)!;
        }

        internal static T? CreateWrapperForRoomObject<T>(INativeRoot nativeRoot, JSObject? proxyObject) where T : class, IRoomObject
            => CreateWrapperForRoomObject(nativeRoot, proxyObject, typeof(T)) as T;

        internal static string GetPrototypeName(Type type)
            => prototypeNameMappings[type];

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureSpawn))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureController))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeOwnedStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeSource))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeCreep))]
        static NativeRoomObjectUtils()
        {
            prototypesObject = GetPrototypesObject();
            try
            {
                // NOTE - order matters! We must do derived classes first and base classes last
                // This is to avoid a nasty bug where the runtime gets objects and their prototype objects mixed up due to the tracking id being set as a property on the object
                RegisterPrototypeTypeMapping<IStructureSpawn, NativeStructureSpawn>("StructureSpawn", FindConstant.Structures);
                RegisterPrototypeTypeMapping<IStructureController, NativeStructureController>("StructureController", FindConstant.Structures);
                RegisterPrototypeTypeMapping<IOwnedStructure, NativeOwnedStructure>("OwnedStructure", FindConstant.Structures);
                RegisterPrototypeTypeMapping<IStructure, NativeStructure>("Structure", FindConstant.Structures);
                RegisterPrototypeTypeMapping<ISource, NativeSource>("Source", FindConstant.Sources);
                RegisterPrototypeTypeMapping<ICreep, NativeCreep>("Creep", FindConstant.Creeps);
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
