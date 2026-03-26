#include "common.h"

// (import "wasi:cli/exit@0.2.0" "exit" (func $__wasm_import_exit_exit (param i32)))
__attribute__((used))
__attribute__((export_name("exit")))
void exit(int status)
{

}
