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

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/world-bindings", EntryPoint = "renew-object")]
    internal static unsafe extern int RenewObject(IntPtr jsHandle);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/world-bindings", EntryPoint = "batch-renew-objects")]
    internal static unsafe extern int BatchRenewObjects(IntPtr* jsHandleList, int count);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/world-bindings", EntryPoint = "fetch-object-room-position")]
    internal static unsafe extern int FetchObjectRoomPosition(IntPtr jsHandle);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/world-bindings", EntryPoint = "batch-fetch-object-room-positions")]
    internal static unsafe extern void BatchFetchObjectRoomPositions(IntPtr* jsHandleList, int count, RoomPosition* outRoomPosList);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/world-bindings", EntryPoint = "get-object-by-id")]
    internal static unsafe extern IntPtr GetObjectById(RawObjectId* objectId);

    [WasmImportLinkage, DllImport("screeps:screepsdotnet/world-bindings", EntryPoint = "get-object-id")]
    internal static unsafe extern int GetObjectId(IntPtr jsHandle, RawObjectId* outObjectId);
}
