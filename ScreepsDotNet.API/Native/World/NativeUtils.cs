using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    internal static class PositionExtensions
    {
        public static Position ToPosition(this JSObject obj)
            => (obj.GetPropertyAsInt32("x"), obj.GetPropertyAsInt32("y"));

        public static Position? ToPositionNullable(this JSObject? obj)
            => obj != null ? new Position?(obj.ToPosition()) : null;

        public static RoomPosition ToRoomPosition(this JSObject obj)
            => new(obj.ToPosition(), obj.GetPropertyAsString("roomName")!);

        public static RoomPosition? ToRoomPositionNullable(this JSObject? obj)
            => obj != null ? new RoomPosition?(obj.ToRoomPosition()) : null;

        public static JSObject ToJS(this Position pos)
        {
            var obj = NativeRoomObjectUtils.CreateObject(null);
            obj.SetProperty("x", pos.X);
            obj.SetProperty("y", pos.Y);
            return obj;
        }

        public static JSObject ToJS(this RoomPosition pos)
            => NativeRoomObjectUtils.CreateRoomPosition(pos.Position.X, pos.Position.Y, pos.RoomName);

        public static JSObject ToJS(this FractionalPosition pos)
        {
            var obj = NativeRoomObjectUtils.CreateObject(null);
            obj.SetProperty("x", pos.X);
            obj.SetProperty("y", pos.Y);
            return obj;
        }

        public static JSObject? ToJS(this Position? pos)
            => pos != null ? pos.Value.ToJS() : null;

        public static JSObject? ToJS(this FractionalPosition? pos)
            => pos != null ? pos.Value.ToJS() : null;
    }
}
