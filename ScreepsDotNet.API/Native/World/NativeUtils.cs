using System;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class PositionExtensions
    {
        public static RoomPosition ToRoomPosition(this JSObject obj)
            => new(obj.ToPosition(), obj.GetPropertyAsString("roomName")!);

        public static RoomPosition? ToRoomPositionNullable(this JSObject? obj)
            => obj != null ? new RoomPosition?(obj.ToRoomPosition()) : null;

        public static JSObject ToJS(this RoomPosition pos)
        {
            if (string.IsNullOrEmpty(pos.RoomName)) { throw new ArgumentException($"RoomName must be a valid string", nameof(pos)); }
            return NativeRoomObjectUtils.CreateRoomPosition(pos.Position.X, pos.Position.Y, pos.RoomName);
        }
    }
}
