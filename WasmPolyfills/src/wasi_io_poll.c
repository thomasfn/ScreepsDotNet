#include "common.h"

// (import "wasi:io/poll@0.2.0" "[resource-drop]pollable" (func $__wasm_import_poll_pollable_drop (param i32)))
__attribute__((used))
__attribute__((export_name("[resource-drop]pollable")))
void resource_drop_pollable(int handle)
{
    
}

// (import "wasi:io/poll@0.2.0" "poll" (func $__wasm_import_poll_poll (param i32 i32 i32)))
__attribute__((used))
__attribute__((export_name("poll")))
void poll(int handle, List_t* pollables, List_t* outList)
{
    outList->ptr = 0;
    outList->len = 0;
}


// (import "wasi:io/poll@0.2.0" "[method]pollable.block" (func $__wasm_import_poll_method_pollable_block (param i32)))
__attribute__((used))
__attribute__((export_name("[method]pollable.block")))
void pollable_block(int handle)
{

}
