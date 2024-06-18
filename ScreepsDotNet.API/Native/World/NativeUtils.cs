using System;
using ScreepsDotNet.Interop;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet.Native.World
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class PositionExtensions
    {
        #region Imports

        [JSImport("createRoomPosition", "game")]
        internal static partial JSObject CreateRoomPosition(int encodedInt);

        #endregion

        public static RoomPosition ToRoomPosition(this JSObject obj)
            => new(obj.ToPosition(), obj.GetPropertyAsString(Names.RoomName)!);

        public static RoomPosition? ToRoomPositionNullable(this JSObject? obj)
            => obj != null ? new RoomPosition?(obj.ToRoomPosition()) : null;

        public static JSObject ToJS(this RoomPosition pos)
            => CreateRoomPosition(pos.ToEncodedInt());
    }
}
