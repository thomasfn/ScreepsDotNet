#include "common.h"

// (import "wasi:cli/terminal-stdout@0.2.0" "get-terminal-stdout" (func $__wasm_import_terminal_stdout_get_terminal_stdout (param i32)))
__attribute__((used))
__attribute__((export_name("get-terminal-stdout")))
void get_terminal_stdout(OptionTag_t* result)
{
    result->tag = 0;
}
