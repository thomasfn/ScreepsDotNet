#include "common.h"

__attribute__((import_module("screeps:screepsdotnet/system-bindings"), import_name("get-random-bytes")))
extern void sys_get_random_bytes(uint8_t* ptr, int len);

// (import "wasi:random/random@0.2.0" "get-random-bytes" (func $__wasm_import_random_get_random_bytes (param i64 i32)))
__attribute__((used))
__attribute__((export_name("get-random-bytes")))
void get_random_bytes(int64_t length, List_t* outBytes)
{
    if (length <= 0)
    {
        outBytes->ptr = 0;
        outBytes->len = 0;
        return;
    }

    if ((uint64_t)length > UINT32_MAX)
    {
        length = UINT32_MAX;
    }

    uint32_t len32 = (uint32_t)length;

    uint8_t* buf = (uint8_t*)_malloc(len32);
    if (!buf)
    {
        outBytes->ptr = 0;
        outBytes->len = 0;
        return;
    }

    sys_get_random_bytes(buf, len32);

    outBytes->ptr = (uint32_t)buf;
    outBytes->len = len32;
}
