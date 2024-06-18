using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Linq;

using ScreepsDotNet.Interop;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal partial class NativeRoom : NativeObject, IRoom, IEquatable<NativeRoom?>
    {
        #region Imports

        [JSImport("Room.createConstructionSite", "game/prototypes/wrapped")]
        internal static partial int Native_CreateConstructionSite(JSObject proxyObject, int x, int y, Name structureType, string? name);

        [JSImport("Room.createFlag", "game/prototypes/wrapped")]
        internal static partial JSObject Native_CreateFlag(JSObject proxyObject, int x, int y, string? name, int? color, int? secondaryColor);

        [JSImport("Room.find", "game/prototypes/wrapped")]
        internal static partial JSObject[] Native_Find(JSObject proxyObject, int type);

        [JSImport("Room.findFast", "game/prototypes/wrapped")]
        internal static partial int Native_FindFast(JSObject proxyObject, int type, IntPtr outRoomObjectMetadataPtr, int maxObjectCount);

        [JSImport("Room.findExitTo", "game/prototypes/wrapped")]
        internal static partial int Native_FindExitTo(JSObject proxyObject, string room);

        [JSImport("Room.findPath", "game/prototypes/wrapped")]
        internal static partial JSObject[] Native_FindPath(JSObject proxyObject, JSObject fromPos, JSObject toPos, JSObject? opts);

        [JSImport("Room.findExitTo", "game/prototypes/wrapped")]
        internal static partial string Native_GetEventLogRaw(JSObject proxyObject, bool raw);

        [JSImport("Room.getTerrain", "game/prototypes/wrapped")]
        internal static partial JSObject Native_GetTerrain(JSObject proxyObject);

        [JSImport("Room.lookAt", "game/prototypes/wrapped")]
        internal static partial JSObject[] Native_LookAt(JSObject proxyObject, int x, int y);

        [JSImport("Room.lookAtFast", "game/prototypes/wrapped")]
        internal static partial int Native_LookAtFast(JSObject proxyObject, int x, int y, IntPtr outRoomObjectMetadataPtr, int maxObjectCount);

        [JSImport("Room.lookAtArea", "game/prototypes/wrapped")]
        internal static partial JSObject[] Native_LookAtArea(JSObject proxyObject, int top, int left, int bottom, int right, bool asArray);

        [JSImport("Room.lookAtAreaFast", "game/prototypes/wrapped")]
        internal static partial int Native_LookAtAreaFast(JSObject proxyObject, int top, int left, int bottom, int right, IntPtr outRoomObjectMetadataPtr, int maxObjectCount);

        [JSImport("Room.lookForAt", "game/prototypes/wrapped")]
        internal static partial JSObject[] Native_LookForAt(JSObject proxyObject, Name type, int x, int y);

        [JSImport("Room.lookForAtFast", "game/prototypes/wrapped")]
        internal static partial int Native_LookForAtFast(JSObject proxyObject, Name type, int x, int y, IntPtr outRoomObjectMetadataPtr, int maxObjectCount);

        [JSImport("Room.lookForAtArea", "game/prototypes/wrapped")]
        internal static partial JSObject[] Native_LookForAtArea(JSObject proxyObject, Name type, int top, int left, int bottom, int right, bool asArray);

        [JSImport("Room.lookForAtAreaFast", "game/prototypes/wrapped")]
        internal static partial int Native_LookForAtAreaFast(JSObject proxyObject, Name type, int top, int left, int bottom, int right, IntPtr outRoomObjectMetadataPtr, int maxObjectCount);

        #endregion

        private const int objectCountBufferSize = 4096; // Assumption: room.Find, room.LookFor etc won't return more than this many objects at once
        private static readonly RoomObjectMetadata[] roomObjectMetadataBuffer = new RoomObjectMetadata[objectCountBufferSize];

        private UserDataStorage userDataStorage;

        private IStructureController? controllerCache;
        private int? energyAvailableCache;
        private int? energyCapacityAvailableCache;
        private NativeMemoryObject? memoryCache;
        private IStructureStorage? storageCache;
        private IStructureTerminal? terminalCache;
        private NativeRoomVisual? visualCache;

        private NativeRoomTerrain? roomTerrainCache;

        public string Name { get; private set; }

        public RoomCoord Coord { get; }

        public IStructureController? Controller => CachePerTick(ref controllerCache) ??= nativeRoot.GetOrCreateWrapperObject<IStructureController>(ProxyObject.GetPropertyAsJSObject(Names.Controller));

        public int EnergyAvailable => CachePerTick(ref energyAvailableCache) ??= ProxyObject.GetPropertyAsInt32(Names.EnergyAvailable);

        public int EnergyCapacityAvailable => CachePerTick(ref energyCapacityAvailableCache) ??= ProxyObject.GetPropertyAsInt32(Names.EnergyCapacityAvailable);

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject(Names.Memory)!);

        public IStructureStorage? Storage => CachePerTick(ref storageCache) ??= nativeRoot.GetOrCreateWrapperObject<IStructureStorage>(ProxyObject.GetPropertyAsJSObject(Names.Storage));

        public IStructureTerminal? Terminal => CachePerTick(ref terminalCache) ??= nativeRoot.GetOrCreateWrapperObject<IStructureTerminal>(ProxyObject.GetPropertyAsJSObject(Names.Terminal));

        public IRoomVisual Visual => CachePerTick(ref visualCache) ??= new NativeRoomVisual(ProxyObject.GetPropertyAsJSObject(Names.Visual)!);

        public NativeRoom(INativeRoot nativeRoot, JSObject proxyObject, string? knownName)
            : base(nativeRoot, proxyObject)
        {
            Name = knownName ?? proxyObject.GetPropertyAsString(Names.Name)!;
            Coord = new(Name);
        }

        public NativeRoom(INativeRoot nativeRoot, JSObject proxyObject)
            : this(nativeRoot, proxyObject, proxyObject.GetPropertyAsString(Names.Name))
        { }

        public override JSObject? ReacquireProxyObject()
            => nativeRoot.RoomsObj.GetPropertyAsJSObject(Name);

        protected override void ClearNativeCache()
        {
            base.ClearNativeCache();
            controllerCache = null;
            energyAvailableCache = null;
            energyCapacityAvailableCache = null;
            memoryCache = null;
            storageCache = null;
            terminalCache = null;
            visualCache = null;
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

        public RoomCreateConstructionSiteResult CreateConstructionSite<T>(Position position, string? name = null) where T : class, IStructure
        {
            var structureConstant = NativeRoomObjectTypes.TypeOf<T>().StructureConstant;
            return structureConstant == null
                ? throw new ArgumentException("Must be a valid structure type", nameof(T))
                : (RoomCreateConstructionSiteResult)Native_CreateConstructionSite(ProxyObject, position.X, position.Y, structureConstant.Value, name);
        }

        public RoomCreateConstructionSiteResult CreateConstructionSite(Position position, Type structureType, string? name = null)
        {
            var structureConstant = NativeRoomObjectTypes.GetTypeForInterfaceType(structureType)?.StructureConstant;
            return structureConstant == null
                ? throw new ArgumentException("Must be a valid structure type", nameof(structureType))
                : (RoomCreateConstructionSiteResult)Native_CreateConstructionSite(ProxyObject, position.X, position.Y, structureConstant.Value, name);
        }

        public RoomCreateFlagResult CreateFlag(Position position, out string newFlagName, string? name = null, FlagColor? color = null, FlagColor? secondaryColor = null)
        {
            using var resultJs = Native_CreateFlag(ProxyObject, position.X, position.Y, name, (int?)color, (int?)secondaryColor);
            newFlagName = resultJs.GetPropertyAsString(Names.Name) ?? string.Empty;
            return (RoomCreateFlagResult)resultJs.GetPropertyAsInt32(Names.Code);
        }

        public IEnumerable<T> Find<T>(bool? my = null) where T : class, IRoomObject
        {
            var type = NativeRoomObjectTypes.TypeOf<T>();
            var findConstant = (my == true ? type.MyFindConstant : my == false ? type.HostileFindConstant : null) ?? type.FindConstant;
            if (findConstant == null) { return []; }
            if (typeof(T).IsAssignableTo(typeof(IWithId)))
            {
                int cnt;
                unsafe
                {
                    fixed (RoomObjectMetadata* roomObjectMetadataBufferPtr = roomObjectMetadataBuffer)
                    {
                        cnt = Native_FindFast(ProxyObject, (int)findConstant, (IntPtr)roomObjectMetadataBufferPtr, objectCountBufferSize);
                    }
                }
                if (cnt >= objectCountBufferSize)
                {
                    Console.WriteLine($"WARNING: IRoom.Find object buffer potential overflow (got {cnt} from native call, objectCountBufferSize={objectCountBufferSize}) - may need to increase buffer size");
                }
                return nativeRoot.GetWrapperObjectsFromBuffer<T>(roomObjectMetadataBuffer.AsSpan()[..cnt]);
            }
            return Native_Find(ProxyObject, (int)findConstant)
                .Select(nativeRoot.GetOrCreateWrapperObject<T>)
                .Where(x => x != null)
                .OfType<T>()
                .ToArray();
        }

        public IEnumerable<Position> FindExits(ExitDirection? exitFilter = null)
        {
            FindConstant findConstant = FindConstant.Exit;
            if (exitFilter != null)
            {
                switch (exitFilter.Value)
                {
                    case ExitDirection.Top: findConstant = FindConstant.ExitTop; break;
                    case ExitDirection.Right: findConstant = FindConstant.ExitRight; break;
                    case ExitDirection.Bottom: findConstant = FindConstant.ExitBottom; break;
                    case ExitDirection.Left: findConstant = FindConstant.ExitLeft; break;
                }
            }
            return Native_Find(ProxyObject, (int)findConstant)
                .Select(x => x.ToPosition())
                .ToArray();
        }

        public RoomFindExitResult FindExitTo(IRoom room)
            => FindExitTo(room.Name);

        public RoomFindExitResult FindExitTo(string roomName)
            => (RoomFindExitResult)Native_FindExitTo(ProxyObject, roomName);

        public IEnumerable<PathStep> FindPath(RoomPosition fromPos, RoomPosition toPos, FindPathOptions? opts = null)
        {
            using var fromPosJs = fromPos.ToJS();
            using var toPosJs = toPos.ToJS();
            using var optsJs = opts?.ToJS();
            var path = Native_FindPath(ProxyObject, fromPosJs, toPosJs, optsJs);
            try
            {
                return path
                    .Select(x => x.ToPathStep())
                    .ToArray();
            }
            finally
            {
                foreach (var pathStepJs in path)
                {
                    pathStepJs.Dispose();
                }
            }
        }

        public string GetRawEventLog()
            => Native_GetEventLogRaw(ProxyObject, true);

        public RoomPosition GetPositionAt(Position position)
            => new(position, Name);

        public IRoomTerrain GetTerrain()
            => roomTerrainCache ??= new NativeRoomTerrain(Native_GetTerrain(ProxyObject));

        public IEnumerable<IRoomObject> LookAt(Position position)
        {
            int cnt;
            unsafe
            {
                fixed (RoomObjectMetadata* roomObjectMetadataBufferPtr = roomObjectMetadataBuffer)
                {
                    cnt = Native_LookAtFast(ProxyObject, position.X, position.Y, (IntPtr)roomObjectMetadataBufferPtr, objectCountBufferSize);
                }
            }
            if (cnt >= objectCountBufferSize)
            {
                Console.WriteLine($"WARNING: IRoom.LookAt object buffer potential overflow (got {cnt} from native call, objectCountBufferSize={objectCountBufferSize}) - may need to increase buffer size");
            }
            return nativeRoot.GetWrapperObjectsFromBuffer<IRoomObject>(roomObjectMetadataBuffer.AsSpan()[..cnt]);
        }

        public IEnumerable<IRoomObject> LookAtArea(Position min, Position max)
        {
            int cnt;
            unsafe
            {
                fixed (RoomObjectMetadata* roomObjectMetadataBufferPtr = roomObjectMetadataBuffer)
                {
                    cnt = Native_LookAtAreaFast(ProxyObject, min.Y, min.X, max.Y, max.X, (IntPtr)roomObjectMetadataBufferPtr, objectCountBufferSize);
                }
            }
            if (cnt >= objectCountBufferSize)
            {
                Console.WriteLine($"WARNING: IRoom.LookAt object buffer potential overflow (got {cnt} from native call, objectCountBufferSize={objectCountBufferSize}) - may need to increase buffer size");
            }
            return nativeRoot.GetWrapperObjectsFromBuffer<IRoomObject>(roomObjectMetadataBuffer.AsSpan()[..cnt]);
        }

        public IEnumerable<T> LookForAt<T>(Position position) where T : class, IRoomObject
        {
            var lookConstant = NativeRoomObjectTypes.TypeOf<T>().LookConstant;
            if (lookConstant == null) { return []; }
            int cnt;
            unsafe
            {
                fixed (RoomObjectMetadata* roomObjectMetadataBufferPtr = roomObjectMetadataBuffer)
                {
                    cnt = Native_LookForAtFast(ProxyObject, lookConstant.Value, position.X, position.Y, (IntPtr)roomObjectMetadataBufferPtr, objectCountBufferSize);
                }
            }
            return nativeRoot.GetWrapperObjectsFromBuffer<T>(roomObjectMetadataBuffer.AsSpan()[..cnt]);
        }

        public IEnumerable<T> LookForAtArea<T>(Position min, Position max) where T : class, IRoomObject
        {
            var lookConstant = NativeRoomObjectTypes.TypeOf<T>().LookConstant;
            if (lookConstant == null) { return []; }
            int cnt;
            unsafe
            {
                fixed (RoomObjectMetadata* roomObjectMetadataBufferPtr = roomObjectMetadataBuffer)
                {
                    cnt = Native_LookForAtAreaFast(ProxyObject, lookConstant.Value, min.Y, min.X, max.Y, max.X, (IntPtr)roomObjectMetadataBufferPtr, objectCountBufferSize);
                }
            }
            return nativeRoot.GetWrapperObjectsFromBuffer<T>(roomObjectMetadataBuffer.AsSpan()[..cnt]);
        }

        private JSObject? InterpretLookElement(JSObject lookElement)
        {
            var typeStr = lookElement.GetPropertyAsString(Names.Type)!;
            if (lookElement.GetTypeOfProperty(typeStr) != JSPropertyType.Object) { return null; }
            return lookElement.GetPropertyAsJSObject(typeStr);
        }

        public override string ToString()
            => $"Room['{Name}']";

        public override bool Equals(object? obj) => Equals(obj as NativeRoom);

        public bool Equals(NativeRoom? other) => other is not null && Coord == other.Coord;

        public override int GetHashCode() => Coord.GetHashCode();

        public static bool operator ==(NativeRoom? left, NativeRoom? right) => EqualityComparer<NativeRoom>.Default.Equals(left, right);

        public static bool operator !=(NativeRoom? left, NativeRoom? right) => !(left == right);
    }
}
