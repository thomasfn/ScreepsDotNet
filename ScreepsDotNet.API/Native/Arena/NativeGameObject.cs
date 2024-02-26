using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Immutable;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet.Native.Arena
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeGameObject : IGameObject, IEquatable<NativeGameObject?>
    {
        #region Imports

        [JSImport("GameObject.findClosestByPath", "game/prototypes/wrapped")]
        internal static partial JSObject? Native_FindClosestByPath_NoOpts(JSObject proxyObject, JSObject[] positions);

        [JSImport("GameObject.findClosestByPath", "game/prototypes/wrapped")]
        internal static partial JSObject? Native_FindClosestByPath(JSObject proxyObject, JSObject[] positions, JSObject options);

        [JSImport("GameObject.findClosestByRange", "game/prototypes/wrapped")]
        internal static partial JSObject Native_FindClosestByRange(JSObject proxyObject, JSObject[] positions);

        [JSImport("GameObject.findInRange", "game/prototypes/wrapped")]
        internal static partial JSObject[] Native_FindInRange(JSObject proxyObject, JSObject[] positions, int range);

        [JSImport("GameObject.findPathTo", "game/prototypes/wrapped")]
        internal static partial int Native_FindPathTo(JSObject proxyObject, JSObject pos, JSObject? options, IntPtr outPtr);

        #endregion

        protected readonly INativeRoot nativeRoot;
        protected readonly JSObject proxyObject;

        private UserDataStorage userDataStorage;
        private int cacheValidAsOf;
        private bool canMove;

        private bool? existsCache;
        private string? idCache;
        private int? ticksToDecayCache;
        private Position? positionCache;

        public bool Exists => CachePerTick(ref existsCache) ??= proxyObject.GetPropertyAsBoolean(Names.Exists);

        public string Id => CacheLifetime(ref idCache) ??= proxyObject.GetPropertyAsString(Names.Id) ?? throw new NotSpawnedYetException();

        public int? TicksToDecay => CachePerTick(ref ticksToDecayCache) ??= proxyObject.TryGetPropertyAsInt32(Names.TicksToDecay);

        public int X => Position.X;

        public int Y => Position.Y;

        public Position Position => (canMove ? ref CachePerTick(ref positionCache) : ref CacheLifetime(ref positionCache)) ??= FetchPosition();

        internal JSObject ProxyObject => proxyObject;

        public NativeGameObject(INativeRoot nativeRoot, JSObject proxyObject, bool canMove)
        {
            this.nativeRoot = nativeRoot;
            this.proxyObject = proxyObject;
            this.canMove = canMove;
            cacheValidAsOf = nativeRoot.TickIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ref T CacheLifetime<T>(ref T cacheValue) => ref cacheValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ref T CachePerTick<T>(ref T cacheValue)
        {
            int tickIndex = nativeRoot.TickIndex;
            if (tickIndex > cacheValidAsOf)
            {
                ClearNativeCache();
                cacheValidAsOf = tickIndex;
            }
            return ref cacheValue;
        }

        protected virtual void ClearNativeCache()
        {
            existsCache = null;
            ticksToDecayCache = null;
            if (canMove) { positionCache = null; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Position FetchPosition() => Exists ? (proxyObject.GetPropertyAsInt32(Names.X), proxyObject.GetPropertyAsInt32(Names.Y)) : throw new NotSpawnedYetException();

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

        public T? FindClosestByPath<T>(IEnumerable<T> positions, FindPathOptions? options) where T : class, IPosition
            => (options != null
                ? Native_FindClosestByPath(proxyObject, positions.Select(x => x.ToJS()).ToArray(), options.Value.ToJS())
                : Native_FindClosestByPath_NoOpts(proxyObject, positions.Select(x => x.ToJS()).ToArray())
            ).ToGameObject<IGameObject>(nativeRoot) as T;

        public Position? FindClosestByPath(IEnumerable<Position> positions, FindPathOptions? options)
            => (options != null
                ? Native_FindClosestByPath(proxyObject, positions.Select(x => x.ToJS()).ToArray(), options.Value.ToJS())
                : Native_FindClosestByPath_NoOpts(proxyObject, positions.Select(x => x.ToJS()).ToArray())
            ).ToPositionNullable();

        public T? FindClosestByRange<T>(IEnumerable<T> positions) where T : class, IPosition
            => Native_FindClosestByRange(proxyObject, positions.Select(x => x.ToJS()).ToArray()).ToGameObject<IGameObject>(nativeRoot) as T;

        public Position? FindClosestByRange(IEnumerable<Position> positions)
            => Native_FindClosestByRange(proxyObject, positions.Select(x => x.ToJS()).ToArray()).ToPosition();

        public IEnumerable<T> FindInRange<T>(IEnumerable<T> positions, int range) where T : class, IPosition
            => Native_FindInRange(proxyObject, positions.Select(x => x.ToJS()).ToArray(), range)
                .Select(nativeRoot.GetOrCreateWrapperForObject)
                .OfType<T>();

        public IEnumerable<Position> FindInRange(IEnumerable<Position> positions, int range)
            => Native_FindInRange(proxyObject, positions.Select(x => x.ToJS()).ToArray(), range)
                .Select(x => x.ToPosition());

        public ImmutableArray<Position> FindPathTo(IPosition pos, FindPathOptions? options)
        {
            using var optionsJs = options?.ToJS();
            int pathLength;
            unsafe
            {
                fixed (Position* pathPositionBufferPtr = NativePathFinder.pathPositionBuffer)
                {
                    pathLength = Native_FindPathTo(proxyObject, pos.ToJS(), optionsJs, (IntPtr)pathPositionBufferPtr);
                }
            }
            if (pathLength == 0) { return []; }
            return NativePathFinder.pathPositionBuffer.AsSpan()[..pathLength].ToImmutableArray();
        }

        public ImmutableArray<Position> FindPathTo(Position pos, FindPathOptions? options)
        {
            using var optionsJs = options?.ToJS();
            int pathLength;
            unsafe
            {
                fixed (Position* pathPositionBufferPtr = NativePathFinder.pathPositionBuffer)
                {
                    pathLength = Native_FindPathTo(proxyObject, pos.ToJS(), optionsJs, (IntPtr)pathPositionBufferPtr);
                }
            }
            if (pathLength == 0) { return []; }
            return NativePathFinder.pathPositionBuffer.AsSpan()[..pathLength].ToImmutableArray();
        }

        public int GetRangeTo(IPosition pos)
            => Position.LinearDistanceTo(pos.Position);

        public int GetRangeTo(Position pos)
            => Position.LinearDistanceTo(pos);

        public override string ToString()
            => Exists ? $"GameObject({Id}, {Position})" : idCache != null ? $"GameObject({idCache})" : $"GameObject({ProxyObject})";

        public override bool Equals(object? obj) => Equals(obj as NativeGameObject);

        public bool Equals(NativeGameObject? other) => other is not null && Id == other.Id;

        public override int GetHashCode() => HashCode.Combine(Id);

        public static bool operator ==(NativeGameObject? left, NativeGameObject? right) => EqualityComparer<NativeGameObject>.Default.Equals(left, right);

        public static bool operator !=(NativeGameObject? left, NativeGameObject? right) => !(left == right);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class NativeGameObjectPrototypes<T> where T : IGameObject
    {
        public static JSObject? ConstructorObj;

        static NativeGameObjectPrototypes()
        {
            RuntimeHelpers.RunClassConstructor(typeof(NativeGameObjectUtils).TypeHandle);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class NativeGameObjectUtils
    {
        private static readonly Name TypeIdKey = Name.CreateNew("__dotnet_typeId");

        private static readonly JSObject prototypesObject;
        private static readonly List<Type> prototypeTypeMappings = [];
        private static readonly Dictionary<Type, string> prototypeNameMappings = [];

        [JSImport("getUtils", "game")]
        internal static partial JSObject GetUtilsObject();

        [JSImport("getPrototypes", "game")]
        internal static partial JSObject GetPrototypesObject();

        internal static void RegisterPrototypeTypeMapping<TInterface, TConcrete>(string prototypeName)
            where TInterface : IGameObject
            where TConcrete : NativeGameObject
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
            NativeGameObjectPrototypes<TInterface>.ConstructorObj = constructor;
            NativeGameObjectPrototypes<TConcrete>.ConstructorObj = constructor;
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
            => GetWrapperTypeForConstructor(JSUtils.GetConstructorOf(jsObject));

        internal static NativeGameObject? CreateWrapperForObjectNullSafe(INativeRoot nativeRoot, JSObject? proxyObject)
            => proxyObject == null ? null : CreateWrapperForObject(nativeRoot, proxyObject);

        internal static NativeGameObject CreateWrapperForObject(INativeRoot nativeRoot, JSObject proxyObject)
        {
            var wrapperType = GetWrapperTypeForObject(proxyObject);
            if (wrapperType == null) { return new NativeGameObject(nativeRoot, proxyObject, true); }
            return (Activator.CreateInstance(wrapperType, [nativeRoot, proxyObject]) as NativeGameObject)!;
        }

        internal static string GetPrototypeName(Type type)
            => prototypeNameMappings[type];

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeCreep))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeOwnedStructure))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureTower))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureContainer))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureExtension))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureRampart))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureRoad))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureSpawn))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeStructureWall))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeFlag))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeSource))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(NativeConstructionSite))]
        static NativeGameObjectUtils()
        {
            prototypesObject = GetPrototypesObject();
            try
            {
                // NOTE - order matters! We must do derived classes first and base classes last
                // This is to avoid a nasty bug where the runtime gets objects and their prototype objects mixed up due to the tracking id being set as a property on the object
                RegisterPrototypeTypeMapping<IStructureTower, NativeStructureTower>("StructureTower");
                RegisterPrototypeTypeMapping<IStructureContainer, NativeStructureContainer>("StructureContainer");
                RegisterPrototypeTypeMapping<IStructureExtension, NativeStructureExtension>("StructureExtension");
                RegisterPrototypeTypeMapping<IStructureRampart, NativeStructureRampart>("StructureRampart");
                RegisterPrototypeTypeMapping<IStructureRoad, NativeStructureRoad>("StructureRoad");
                RegisterPrototypeTypeMapping<IStructureSpawn, NativeStructureSpawn>("StructureSpawn");
                RegisterPrototypeTypeMapping<IOwnedStructure, NativeOwnedStructure>("OwnedStructure");
                RegisterPrototypeTypeMapping<IStructureWall, NativeStructureWall>("StructureWall");
                RegisterPrototypeTypeMapping<IStructure, NativeStructure>("Structure");
                RegisterPrototypeTypeMapping<IFlag, NativeFlag>("Flag");
                RegisterPrototypeTypeMapping<IResource, NativeResource>("Resource");
                RegisterPrototypeTypeMapping<ISource, NativeSource>("Source");
                RegisterPrototypeTypeMapping<IConstructionSite, NativeConstructionSite>("ConstructionSite");
                RegisterPrototypeTypeMapping<ICreep, NativeCreep>("Creep");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class NativeGameObjectExtensions
    {
        public static JSObject ToJS(this IPosition position)
        {
            if (position is NativeGameObject nativeGameObject) { return nativeGameObject.ProxyObject; }
            var obj = JSObject.Create();
            obj.SetProperty(Names.X, position.X);
            obj.SetProperty(Names.Y, position.Y);
            return obj;
        }

        public static T? ToGameObject<T>(this JSObject? proxyObject, INativeRoot nativeRoot) where T : class, IGameObject
            => proxyObject != null ? nativeRoot.GetOrCreateWrapperForObject(proxyObject) as T : null;
    }
}
