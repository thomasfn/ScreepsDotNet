#include "common.h"

// (import "wasi:cli/stdin@0.2.0" "get-stdin" (func $__wasm_import_stdin_get_stdin (result i32)))
__attribute__((used))
__attribute__((export_name("get-stdin")))
int get_stdin()
{
    return FD_STDIN;
}
