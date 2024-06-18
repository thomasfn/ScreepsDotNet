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

    [WasmImportLinkage, DllImport("bindings", EntryPoint = "js_renew_object")]
    internal static unsafe extern int RenewObject(IntPtr jsHandle);

    [WasmImportLinkage, DllImport("bindings", EntryPoint = "js_batch_renew_objects")]
    internal static unsafe extern int BatchRenewObjects(IntPtr* jsHandleList, int count);

    [WasmImportLinkage, DllImport("bindings", EntryPoint = "js_fetch_object_room_position")]
    internal static unsafe extern int FetchObjectRoomPosition(IntPtr jsHandle);

    [WasmImportLinkage, DllImport("bindings", EntryPoint = "js_batch_fetch_object_room_positions")]
    internal static unsafe extern void BatchFetchObjectRoomPositions(IntPtr* jsHandleList, int count, RoomPosition* outRoomPosList);

    [WasmImportLinkage, DllImport("bindings", EntryPoint = "js_get_object_by_id")]
    internal static unsafe extern IntPtr GetObjectById(RawObjectId* objectId);

    [WasmImportLinkage, DllImport("bindings", EntryPoint = "js_get_object_id")]
    internal static unsafe extern int GetObjectId(IntPtr jsHandle, RawObjectId* outObjectId);
}
