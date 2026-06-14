#include "common.h"

__attribute__((import_module("screeps:screepsdotnet/system-bindings"), import_name("get-env-args")))
extern void sys_get_env_args(List_t* outList);

// (import "wasi:cli/environment@0.2.0" "get-environment" (func $__wasm_import_environment_get_environment (param i32)))
__attribute__((used))
__attribute__((export_name("get-environment")))
void get_environment(List_t* outList)
{
    sys_get_env_args(outList);
}
