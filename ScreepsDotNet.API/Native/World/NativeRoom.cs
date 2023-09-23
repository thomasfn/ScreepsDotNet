using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Linq;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal partial class NativeRoom : NativeObject, IRoom, IEquatable<NativeRoom?>
    {
        #region Imports

        [JSImport("Room.createConstructionSite", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_CreateConstructionSite([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y, [JSMarshalAs<JSType.String>] string structureType, [JSMarshalAs<JSType.String>] string? name);

        [JSImport("Room.createFlag", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_CreateFlag([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y, [JSMarshalAs<JSType.String>] string? name, [JSMarshalAs<JSType.Number>] int? color, [JSMarshalAs<JSType.Number>] int? secondaryColor);

        [JSImport("Room.find", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_Find([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int type);

        [JSImport("Room.findFast", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_FindFast([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int type);

        [JSImport("Room.findExitTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_FindExitTo([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string room);

        [JSImport("Room.findPath", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_FindPath([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Object>] JSObject fromPos, [JSMarshalAs<JSType.Object>] JSObject toPos, [JSMarshalAs<JSType.Object>] JSObject? opts);

        [JSImport("Room.findExitTo", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.String>]
        internal static partial string Native_GetEventLogRaw([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Boolean>] bool raw);

        [JSImport("Room.getTerrain", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Object>]
        internal static partial JSObject Native_GetTerrain([JSMarshalAs<JSType.Object>] JSObject proxyObject);

        [JSImport("Room.lookAt", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_LookAt([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y);

        [JSImport("Room.lookAtFast", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_LookAtFast([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y);

        [JSImport("Room.lookAtArea", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_LookAtArea([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int top, [JSMarshalAs<JSType.Number>] int left, [JSMarshalAs<JSType.Number>] int bottom, [JSMarshalAs<JSType.Number>] int right, [JSMarshalAs<JSType.Boolean>] bool asArray);

        [JSImport("Room.lookAtAreaFast", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_LookAtAreaFast([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int top, [JSMarshalAs<JSType.Number>] int left, [JSMarshalAs<JSType.Number>] int bottom, [JSMarshalAs<JSType.Number>] int right);

        [JSImport("Room.lookForAt", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_LookForAt([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string type, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y);

        [JSImport("Room.lookForAtFast", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_LookForAtFast([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string type, [JSMarshalAs<JSType.Number>] int x, [JSMarshalAs<JSType.Number>] int y);

        [JSImport("Room.lookForAtArea", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_LookForAtArea([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string type, [JSMarshalAs<JSType.Number>] int top, [JSMarshalAs<JSType.Number>] int left, [JSMarshalAs<JSType.Number>] int bottom, [JSMarshalAs<JSType.Number>] int right, [JSMarshalAs<JSType.Boolean>] bool asArray);

        [JSImport("Room.lookForAtAreaFast", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Number>]
        internal static partial int Native_LookForAtAreaFast([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.String>] string type, [JSMarshalAs<JSType.Number>] int top, [JSMarshalAs<JSType.Number>] int left, [JSMarshalAs<JSType.Number>] int bottom, [JSMarshalAs<JSType.Number>] int right);

        #endregion

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

        public IStructureController? Controller => CachePerTick(ref controllerCache) ??= nativeRoot.GetOrCreateWrapperObject<IStructureController>(ProxyObject.GetPropertyAsJSObject("controller"));

        public int EnergyAvailable => CachePerTick(ref energyAvailableCache) ??= ProxyObject.GetPropertyAsInt32("energyAvailable");

        public int EnergyCapacityAvailable => CachePerTick(ref energyCapacityAvailableCache) ??= ProxyObject.GetPropertyAsInt32("energyCapacityAvailable");

        public IMemoryObject Memory => CachePerTick(ref memoryCache) ??= new NativeMemoryObject(ProxyObject.GetPropertyAsJSObject("memory")!);

        public IStructureStorage? Storage => CachePerTick(ref storageCache) ??= nativeRoot.GetOrCreateWrapperObject<IStructureStorage>(ProxyObject.GetPropertyAsJSObject("storage"));

        public IStructureTerminal? Terminal => CachePerTick(ref terminalCache) ??= nativeRoot.GetOrCreateWrapperObject<IStructureTerminal>(ProxyObject.GetPropertyAsJSObject("terminal"));

        public IRoomVisual Visual => CachePerTick(ref visualCache) ??= new NativeRoomVisual(ProxyObject.GetPropertyAsJSObject("visual")!);

        public NativeRoom(INativeRoot nativeRoot, JSObject proxyObject, string? knownName)
            : base(nativeRoot, proxyObject)
        {
            Name = knownName ?? proxyObject.GetPropertyAsString("name")!;
            Coord = new(Name);
        }

        public NativeRoom(INativeRoot nativeRoot, JSObject proxyObject)
            : this(nativeRoot, proxyObject, proxyObject.GetPropertyAsString("name"))
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

        public RoomCreateConstructionSiteResult CreateConstructionSite<T>(Position position, string? name = null) where T : class, IStructure
        {
            var structureConstant = NativeRoomObjectPrototypes<T>.StructureConstant;
            if (structureConstant == null) { throw new ArgumentException("Must be a valid structure type", nameof(T)); }
            return (RoomCreateConstructionSiteResult)Native_CreateConstructionSite(ProxyObject, position.X, position.Y, structureConstant, name);
        }

        public RoomCreateConstructionSiteResult CreateConstructionSite(Position position, Type structureType, string? name = null)
        {
            var structureConstant = NativeRoomObjectUtils.GetStructureConstantForInterfaceType(structureType);
            if (structureConstant == null) { throw new ArgumentException("Must be a valid structure type", nameof(structureType)); }
            return (RoomCreateConstructionSiteResult)Native_CreateConstructionSite(ProxyObject, position.X, position.Y, structureConstant, name);
        }

        public RoomCreateFlagResult CreateFlag(Position position, out string newFlagName, string? name = null, FlagColor? color = null, FlagColor? secondaryColor = null)
        {
            using var resultJs = Native_CreateFlag(ProxyObject, position.X, position.Y, name, (int?)color, (int?)secondaryColor);
            newFlagName = resultJs.GetPropertyAsString("name") ?? string.Empty;
            return (RoomCreateFlagResult)resultJs.GetPropertyAsInt32("code");
        }

        public IEnumerable<T> Find<T>(bool? my = null) where T : class, IRoomObject
        {
            var findConstant = (my == true ? NativeRoomObjectPrototypes<T>.MyFindConstant : my == false ? NativeRoomObjectPrototypes<T>.HostileFindConstant : null) ?? NativeRoomObjectPrototypes<T>.FindConstant;
            if (findConstant == null) { return Enumerable.Empty<T>(); }
            if (typeof(T).IsAssignableTo(typeof(IWithId)))
            {
                int cnt = Native_FindFast(ProxyObject, (int)findConstant);
                return nativeRoot.GetWrapperObjectsFromCopyBuffer<T>(cnt);
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

        public IEnumerable<PathStep> FindPath(Position fromPos, Position toPos, FindPathOptions? opts = null)
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
            => nativeRoot.GetWrapperObjectsFromCopyBuffer<IRoomObject>(Native_LookAtFast(ProxyObject, position.X, position.Y));

        public IEnumerable<IRoomObject> LookAtArea(Position min, Position max)
            => nativeRoot.GetWrapperObjectsFromCopyBuffer<IRoomObject>(Native_LookAtAreaFast(ProxyObject, min.Y, min.X, max.Y, max.X));

        public IEnumerable<T> LookForAt<T>(Position position) where T : class, IRoomObject
        {
            var lookConstant = NativeRoomObjectPrototypes<T>.LookConstant;
            if (lookConstant == null) { return Enumerable.Empty<T>(); }
            return nativeRoot.GetWrapperObjectsFromCopyBuffer<T>(Native_LookForAtFast(ProxyObject, lookConstant, position.X, position.Y));
        }

        public IEnumerable<T> LookForAtArea<T>(Position min, Position max) where T : class, IRoomObject
        {
            var lookConstant = NativeRoomObjectPrototypes<T>.LookConstant;
            if (lookConstant == null) { return Enumerable.Empty<T>(); }
            return nativeRoot.GetWrapperObjectsFromCopyBuffer<T>(Native_LookForAtAreaFast(ProxyObject, lookConstant, min.Y, min.X, max.Y, max.X));
        }

        private JSObject? InterpretLookElement(JSObject lookElement)
        {
            var typeStr = lookElement.GetPropertyAsString("type")!;
            if (lookElement.GetTypeOfProperty(typeStr) != "object") { return null; }
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
