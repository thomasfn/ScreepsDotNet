#include "common.h"

__attribute__((import_module("screeps:screepsdotnet/system-bindings"), import_name("get-wall-time")))
extern void sys_get_wall_time(uint64_t* seconds_ptr, uint32_t* nanoseconds_ptr);

typedef struct {
    uint64_t seconds;
    uint32_t nanoseconds;
} DateTime_t;

// (import "wasi:clocks/wall-clock@0.2.0" "now" (func $__wasm_import_wall_clock_now (param i32)))
__attribute__((used))
__attribute__((export_name("now")))
void now(DateTime_t* result)
{
    sys_get_wall_time(&result->seconds, &result->nanoseconds);
}
