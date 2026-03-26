#include "common.h"

// (import "wasi:cli/terminal-stderr@0.2.0" "get-terminal-stderr" (func $__wasm_import_terminal_stderr_get_terminal_stderr (param i32)))
__attribute__((used))
__attribute__((export_name("get-terminal-stderr")))
void get_terminal_stderr(OptionTag_t* result)
{
    result->tag = 0;
}
