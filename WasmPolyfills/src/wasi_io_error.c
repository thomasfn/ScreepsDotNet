#include "common.h"

// (import "wasi:io/error@0.2.0" "[resource-drop]error" (func $__wasm_import_io_error_error_drop (param i32)))
__attribute__((used))
__attribute__((export_name("[resource-drop]error")))
void resource_drop_error(int handle)
{
    
}
