using System;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static class PositionExtensions
    {
        public static RoomPosition ToRoomPosition(this JSObject obj)
            => new(obj.ToPosition(), obj.GetPropertyAsString(Names.RoomName)!);

        public static RoomPosition? ToRoomPositionNullable(this JSObject? obj)
            => obj != null ? new RoomPosition?(obj.ToRoomPosition()) : null;

        public static JSObject ToJS(this RoomPosition pos)
            => NativeRoomObjectUtils.CreateRoomPosition(pos.ToEncodedInt());
    }
}
