using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ScreepsDotNet.API.World;

internal static class ScreepsDotNet_Native
{
    [InlineArray(24)]
    public struct RawObjectId
    {
        private byte firstChar;

        public Span<byte> AsSpan => MemoryMarshal.CreateSpan(ref firstChar, 24);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern int RenewObject(IntPtr jsHandle);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern int BatchRenewObjects(IntPtr* jsHandleList, int count);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern void DecodeObjectIds(RawObjectId* rawObjectIdList, int count, ObjectId* outObjectIdList);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern void FetchObjectRoomPosition(IntPtr jsHandle, RoomPosition* outRoomPos);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern void BatchFetchObjectRoomPositions(IntPtr* jsHandleList, int count, RoomPosition* outRoomPosList);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern IntPtr GetObjectById(ObjectId* objectId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern int GetObjectId(IntPtr jsHandle, ObjectId* outObjectId);
}
