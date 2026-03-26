#include "common.h"

__attribute__((import_module("screeps:screepsdotnet/system-bindings"), import_name("get-mono-time")))
extern void sys_get_mono_time(int64_t* time_ptr);

// (import "wasi:clocks/monotonic-clock@0.2.0" "now" (func $__wasm_import_monotonic_clock_now (result i64)))
__attribute__((used))
__attribute__((export_name("now")))
int64_t now()
{
    int64_t result;
    sys_get_mono_time(&result);
    return result;
}

// (import "wasi:clocks/monotonic-clock@0.2.0" "subscribe-instant" (func $__wasm_import_monotonic_clock_subscribe_instant (param i64) (result i32)))
__attribute__((used))
__attribute__((export_name("subscribe-instant")))
uint32_t subscribe_instant(uint64_t when)
{
    return 0;
}


// (import "wasi:clocks/monotonic-clock@0.2.0" "subscribe-duration" (func $__wasm_import_monotonic_clock_subscribe_duration (param i64) (result i32)))
__attribute__((used))
__attribute__((export_name("subscribe-duration")))
uint32_t subscribe_duration(uint64_t when)
{
    return 0;
}
