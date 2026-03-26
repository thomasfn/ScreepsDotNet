#include "common.h"

// (import "wasi:cli/stdout@0.2.0" "get-stdout" (func $__wasm_import_stdout_get_stdout (result i32)))
__attribute__((used))
__attribute__((export_name("get-stdout")))
int get_stdout()
{
    return FD_STDOUT;
}
