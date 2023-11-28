using System.Runtime.CompilerServices;

internal static class ScreepsDotNet_Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern void DecodeRoomPosition(void* encodedRoomPositionPtr, void* outPtr);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern void DecodeRoomPositions(void* encodedRoomPositionPtr, int encodedRoomPositionStride, void* outPtr, int outStride, int count);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern void DecodeObjectId(void* encodedIdPtr, void* outPtr);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern void DecodeObjectIds(void* encodedIdPtr, int encodedIdStride, void* outPtr, int outStride, int count);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static unsafe extern void DecodeRoomObjectListFromCopyBuffer(void* outPtr, int count);
}
