#include "common.h"

// (import "wasi:filesystem/preopens@0.2.0" "get-directories" (func $__wasm_import_filesystem_preopens_get_directories (param i32)))
__attribute__((used))
__attribute__((export_name("get-directories")))
void get_directories(List_t* outList)
{
    outList->ptr = 0;
    outList->len = 0;
}
