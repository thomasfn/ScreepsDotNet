#include "common.h"

// (import "wasi:cli/environment@0.2.0" "get-environment" (func $__wasm_import_environment_get_environment (param i32)))
__attribute__((used))
__attribute__((export_name("get-environment")))
void get_environment(List_t* outList)
{
    outList->ptr = 0;
    outList->len = 0;
}
