#include "common.h"

// (import "wasi:random/random@0.2.0" "get-random-bytes" (func $__wasm_import_random_get_random_bytes (param i64 i32)))
__attribute__((used))
__attribute__((export_name("get-random-bytes")))
void get_random_bytes(int64_t length, List_t* outBytes)
{
    outBytes->ptr = 0;
    outBytes->len = 0;
}
