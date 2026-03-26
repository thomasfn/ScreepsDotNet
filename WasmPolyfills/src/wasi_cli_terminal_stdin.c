#include "common.h"

// (import "wasi:cli/terminal-stdin@0.2.0" "get-terminal-stdin" (func $__wasm_import_terminal_stdin_get_terminal_stdin (param i32)))
__attribute__((used))
__attribute__((export_name("get-terminal-stdin")))
void get_terminal_stdin(OptionTag_t* result)
{
    result->tag = 0;
}
