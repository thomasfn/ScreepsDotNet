#include "common.h"

__attribute__((import_module("screeps:screepsdotnet/system-bindings"), import_name("write-stdout")))
extern void sys_write_stdout(uint8_t* ptr, int len);

__attribute__((import_module("screeps:screepsdotnet/system-bindings"), import_name("write-stderr")))
extern void sys_write_stderr(uint8_t* ptr, int len);

//  (import "wasi:io/streams@0.2.0" "[resource-drop]input-stream" (func $fimport$50 (param i32)))
__attribute__((used))
__attribute__((export_name("[resource-drop]input-stream")))
void input_stream_resource_drop(int stream)
{
    (void)stream;
}

//  (import "wasi:io/streams@0.2.0" "[resource-drop]output-stream" (func $fimport$51 (param i32)))
__attribute__((used))
__attribute__((export_name("[resource-drop]output-stream")))
void output_stream_resource_drop(int stream)
{
    (void)stream;
}

//  (import "wasi:io/streams@0.2.0" "[method]input-stream.subscribe" (func $fimport$57 (param i32) (result i32)))
__attribute__((used))
__attribute__((export_name("[method]input-stream.subscribe")))
int input_stream_subscribe(int stream)
{
    (void)stream;
    return 0;
}

//  (import "wasi:io/streams@0.2.0" "[method]output-stream.subscribe" (func $fimport$58 (param i32) (result i32)))
__attribute__((used))
__attribute__((export_name("[method]output-stream.subscribe")))
int output_stream_subscribe(int stream)
{
    (void)stream;
    return 0;
}

// (import "wasi:io/streams@0.2.0" "[method]output-stream.check-write" (func $__wasm_import_streams_method_output_stream_check_write (param i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]output-stream.check-write")))
void output_stream_check_write(int stream, uint64_t* result)
{
    if (stream == FD_STDOUT || stream == FD_STDERR)
    {
        *result = (uint64_t)-1;
    }
    else
    {
        *result = (uint64_t)0;
    }
}

// (import "wasi:io/streams@0.2.0" "[method]output-stream.write" (func $__wasm_import_streams_method_output_stream_write (param i32 i32 i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]output-stream.write")))
void output_stream_write(int stream, uint8_t* ptr, int len, void* result_ptr)
{
    int written;
    if (ptr && len > 0)
    {
        if (stream == FD_STDOUT)
        {
            sys_write_stdout(ptr, len);
            written = len;
        }
        else if (stream == FD_STDERR)
        {
            sys_write_stderr(ptr, len);
            written = len;
        }
        else
        {
            written = 0;
        }
    }
    else
    {
        written = 0;
    }
    
    uint8_t *out = (uint8_t*)result_ptr;
    out[0] = written > 0 ? 0 : 1;

    uint32_t *payload = (uint32_t*)(result_ptr) + 1;
    *payload = (uint32_t)written;
}

// (import "wasi:io/streams@0.2.0" "[method]output-stream.blocking-flush" (func $__wasm_import_streams_method_output_stream_blocking_flush (param i32 i32)))
__attribute__((used))
__attribute__((export_name("[method]output-stream.blocking-flush")))
void output_stream_blocking_flush(int stream, uint8_t* result_ptr)
{
    (void)stream;
    result_ptr[0] = 0;
}
