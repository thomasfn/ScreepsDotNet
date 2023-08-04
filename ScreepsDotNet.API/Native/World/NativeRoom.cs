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

        [JSImport("Room.find", "game/prototypes/wrapped")]
        [return: JSMarshalAsAttribute<JSType.Array<JSType.Object>>]
        internal static partial JSObject[] Native_Find([JSMarshalAs<JSType.Object>] JSObject proxyObject, [JSMarshalAs<JSType.Number>] int type);

        #endregion

        public string Name { get; private set; }

        public bool Exists => proxyObjectOrNull != null;

        public IStructureController? Controller => NativeRoomObjectUtils.CreateWrapperForRoomObject<IStructureController>(nativeRoot, ProxyObject.GetPropertyAsJSObject("controller"));

        public int EnergyAvailable => ProxyObject.GetPropertyAsInt32("energyAvailable");

        public int EnergyCapacityAvailable => ProxyObject.GetPropertyAsInt32("energyCapacityAvailable");

        public object Memory => throw new NotImplementedException();

        public object? Storage => throw new NotImplementedException();

        public object? Terminal => throw new NotImplementedException();

        public object Visual => throw new NotImplementedException();

        public NativeRoom(INativeRoot nativeRoot, JSObject proxyObject, string knownName)
            : base(nativeRoot, proxyObject)
        {
            Name = knownName ?? proxyObject.GetPropertyAsString("name")!;
        }

        public NativeRoom(INativeRoot nativeRoot, JSObject proxyObject)
            : this(nativeRoot, proxyObject, proxyObject.GetPropertyAsString("name")!)
        { }

        public override void InvalidateProxyObject()
        {
            proxyObjectOrNull = nativeRoot.RoomsObj.GetPropertyAsJSObject(Name);
            ClearNativeCache();
        }

        //RoomCreateConstructionSiteResult CreateConstructionSite<T>(Position position, string? name = null) where T : class, IStructure
        //{
        //    throw new NotImplementedException();
        //}

        public RoomCreateFlagResult CreateFlag(Position position, out string newFlagName, string? name = null, Color? color = null, Color? secondaryColor = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Find<T>() where T : class, IRoomObject
        {
            var findConstant = NativeRoomObjectPrototypes<T>.FindConstant;
            if (findConstant == null) { return Enumerable.Empty<T>(); }
            return Native_Find(ProxyObject, (int)findConstant)
                .Select(x => NativeRoomObjectUtils.CreateWrapperForRoomObject<T>(nativeRoot, x))
                .Where(x => x != null)
                .OfType<T>()
                .ToArray();
        }

        public IEnumerable<Position> FindExits(RoomExitDirection? exitFilter = null)
        {
            throw new NotImplementedException();
        }

        public RoomFindExitResult FindExitTo(IRoom room)
        {
            throw new NotImplementedException();
        }

        public RoomFindExitResult FindExitTo(string roomName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PathStep> FindPath(Position fromPos, Position toPos, FindPathOptions? opts = null)
        {
            throw new NotImplementedException();
        }

        public string GetRawEventLog()
        {
            throw new NotImplementedException();
        }

        public RoomPosition GetPositionAt(Position position)
            => new(position, Name);

        //public IRoomTerrain GetTerrain()
        //{
        //    throw new NotImplementedException();
        //}

        public IEnumerable<IRoomObject> LookAt(Position position)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IRoomObject> LookAtArea(Position min, Position max)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> LookForAt<T>(Position position) where T : class, IRoomObject
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> LookForAtArea<T>(Position min, Position max) where T : class, IRoomObject
        {
            throw new NotImplementedException();
        }

        public override string ToString()
            => $"Room['{Name}']";

        public override bool Equals(object? obj) => Equals(obj as NativeRoom);

        public bool Equals(NativeRoom? other) => other is not null && Name == other.Name;

        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator ==(NativeRoom? left, NativeRoom? right) => EqualityComparer<NativeRoom>.Default.Equals(left, right);

        public static bool operator !=(NativeRoom? left, NativeRoom? right) => !(left == right);
    }
}
