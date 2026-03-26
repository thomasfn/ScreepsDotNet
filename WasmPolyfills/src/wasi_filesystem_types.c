#include "common.h"

static inline void write_result_ok_u32(void* out_ptr, uint32_t value)
{
    uint8_t *out = (uint8_t*)out_ptr;
    out[0] = 0; // ok
    *(uint32_t*)(out + 4) = value;
}

static inline void write_result_err(void* out_ptr, uint32_t err)
{
    uint8_t *out = (uint8_t*)out_ptr;
    out[0] = 1; // err
    *(uint32_t*)(out + 4) = err;
}

// (import "wasi:filesystem/types@0.2.0" "[resource-drop]descriptor" (func $__wasm_import_filesystem_descriptor_drop (param i32)))
__attribute__((used))
__attribute__((export_name("[resource-drop]descriptor")))
void descriptor_drop(int handle)
{
    (void)handle;
}

// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.read-via-stream" (func $__wasm_import_filesystem_method_descriptor_read_via_stream (param i32 i64 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.read-via-stream")))
void descriptor_read_via_stream(int self, long long offset, void* out_ptr)
{
    (void)self;
    (void)offset;

    write_result_err(out_ptr, 1);
}


// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.write-via-stream" (func $__wasm_import_filesystem_method_descriptor_write_via_stream (param i32 i64 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.write-via-stream")))
void descriptor_write_via_stream(int self, long long offset, void* out_ptr)
{
    (void)self;
    (void)offset;

    write_result_err(out_ptr, 1);
}

// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.append-via-stream" (func $__wasm_import_filesystem_method_descriptor_append_via_stream (param i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.append-via-stream")))
void descriptor_append_via_stream(int self, void* out_ptr)
{
    (void)self;

    write_result_err(out_ptr, 1);
}


// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.get-flags" (func $__wasm_import_filesystem_method_descriptor_get_flags (param i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.get-flags")))
void descriptor_get_flags(int self, void* out_ptr)
{
    (void)self;

    write_result_ok_u32(out_ptr, 0);
}


// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.stat" (func $__wasm_import_filesystem_method_descriptor_stat (param i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.stat")))
void descriptor_stat(int self, void* out_ptr)
{
    (void)self;

    write_result_err(out_ptr, 1);
}


// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.stat-at" (func $__wasm_import_filesystem_method_descriptor_stat_at (param i32 i32 i32 i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.stat-at")))
void descriptor_stat_at(int self, int path_ptr, int path_len, int flags, void* out_ptr)
{
    (void)self;
    (void)path_ptr;
    (void)path_len;
    (void)flags;

    write_result_err(out_ptr, 1);
}


// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.unlink-file-at" (func $__wasm_import_filesystem_method_descriptor_unlink_file_at (param i32 i32 i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.unlink-file-at")))
void descriptor_unlink_file_at(int self, int path_ptr, int path_len, int flags)
{
    (void)self;
    (void)path_ptr;
    (void)path_len;
    (void)flags;

}


// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.metadata-hash" (func $__wasm_import_filesystem_method_descriptor_metadata_hash (param i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.metadata-hash")))
void descriptor_metadata_hash(int self, void* out_ptr)
{
    (void)self;

    write_result_err(out_ptr, 1);
}


// (import "wasi:filesystem/types@0.2.0" "[method]descriptor.metadata-hash-at" (func $__wasm_import_filesystem_method_descriptor_metadata_hash_at (param i32 i32 i32 i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]descriptor.metadata-hash-at")))
void descriptor_metadata_hash_at(int self, int path_ptr, int path_len, int flags, void* out_ptr)
{
    (void)self;
    (void)path_ptr;
    (void)path_len;
    (void)flags;

    write_result_err(out_ptr, 1);
}
