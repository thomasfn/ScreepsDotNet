#include "common.h"

// (import "wasi:cli/stderr@0.2.0" "get-stderr" (func $__wasm_import_stderr_get_stderr (result i32)))
__attribute__((used))
__attribute__((export_name("get-stderr")))
int get_stderr()
{
    return FD_STDERR;
}
