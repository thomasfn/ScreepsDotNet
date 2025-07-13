(module
 (type $0 (func (param i32 i32) (result i32)))
 (type $1 (func (param i32 i32 i32 i32) (result i32)))
 (type $2 (func (param i32 i32)))
 (type $3 (func (param i32)))
 (type $4 (func (param i32 i64 i32) (result i32)))
 (type $5 (func (param i32 i64 i64 i32) (result i32)))
 (type $6 (func (param i32) (result i32)))
 (type $7 (func (param i32 i64) (result i32)))
 (type $8 (func (param i32 i32 i32 i64 i32) (result i32)))
 (type $9 (func (param i32 i32 i32) (result i32)))
 (type $10 (func (param i32 i64 i32 i32) (result i32)))
 (type $11 (func (param i32 i32 i32 i32 i32) (result i32)))
 (type $12 (func (param i32 i32 i32 i32 i32 i64 i64 i32 i32) (result i32)))
 (type $13 (func (param i32 i32 i32 i32 i32 i32) (result i32)))
 (type $14 (func (result i32)))
 (import "env" "memory" (memory $0 0))
 (import "screeps:screepsdotnet/system-bindings" "get-time" (func $src/wasi_snapshot_preview1/sys_get_time (param i32)))
 (import "screeps:screepsdotnet/system-bindings" "write-stdout" (func $src/wasi_snapshot_preview1/sys_write_stdout (param i32 i32)))
 (import "screeps:screepsdotnet/system-bindings" "write-stderr" (func $src/wasi_snapshot_preview1/sys_write_stderr (param i32 i32)))
 (import "screeps:screepsdotnet/system-bindings" "get-random" (func $src/wasi_snapshot_preview1/sys_get_random (param i32 i32)))
 (export "args_get" (func $src/wasi_snapshot_preview1/args_get))
 (export "args_sizes_get" (func $src/wasi_snapshot_preview1/args_sizes_get))
 (export "environ_get" (func $src/wasi_snapshot_preview1/args_get))
 (export "environ_sizes_get" (func $src/wasi_snapshot_preview1/args_sizes_get))
 (export "clock_res_get" (func $src/wasi_snapshot_preview1/clock_res_get))
 (export "clock_time_get" (func $src/wasi_snapshot_preview1/clock_time_get))
 (export "fd_advise" (func $src/wasi_snapshot_preview1/fd_advise))
 (export "fd_close" (func $src/wasi_snapshot_preview1/fd_close))
 (export "fd_fdstat_get" (func $src/wasi_snapshot_preview1/fd_fdstat_get))
 (export "fd_fdstat_set_flags" (func $src/wasi_snapshot_preview1/fd_fdstat_get))
 (export "fd_filestat_get" (func $src/wasi_snapshot_preview1/fd_fdstat_get))
 (export "fd_filestat_set_size" (func $src/wasi_snapshot_preview1/fd_filestat_set_size))
 (export "fd_pread" (func $src/wasi_snapshot_preview1/fd_pread))
 (export "fd_prestat_get" (func $src/wasi_snapshot_preview1/fd_fdstat_get))
 (export "fd_prestat_dir_name" (func $src/wasi_snapshot_preview1/fd_prestat_dir_name))
 (export "fd_read" (func $src/wasi_snapshot_preview1/fd_read))
 (export "fd_readdir" (func $src/wasi_snapshot_preview1/fd_pread))
 (export "fd_seek" (func $src/wasi_snapshot_preview1/fd_seek))
 (export "fd_tell" (func $src/wasi_snapshot_preview1/fd_fdstat_get))
 (export "fd_write" (func $src/wasi_snapshot_preview1/fd_write))
 (export "path_filestat_get" (func $src/wasi_snapshot_preview1/path_filestat_get))
 (export "path_open" (func $src/wasi_snapshot_preview1/path_open))
 (export "path_readlink" (func $src/wasi_snapshot_preview1/path_readlink))
 (export "path_unlink_file" (func $src/wasi_snapshot_preview1/fd_prestat_dir_name))
 (export "poll_oneoff" (func $src/wasi_snapshot_preview1/poll_oneoff))
 (export "proc_exit" (func $src/wasi_snapshot_preview1/proc_exit))
 (export "sched_yield" (func $src/wasi_snapshot_preview1/sched_yield))
 (export "random_get" (func $src/wasi_snapshot_preview1/random_get))
 (export "adapter_close_badfd" (func $src/wasi_snapshot_preview1/fd_close))
 (func $src/wasi_snapshot_preview1/args_get (param $0 i32) (param $1 i32) (result i32)
  i32.const 0
 )
 (func $src/wasi_snapshot_preview1/args_sizes_get (param $0 i32) (param $1 i32) (result i32)
  local.get $0
  i32.const 0
  i32.store
  local.get $1
  i32.const 0
  i32.store
  i32.const 0
 )
 (func $src/wasi_snapshot_preview1/clock_res_get (param $0 i32) (param $1 i32) (result i32)
  local.get $0
  i32.eqz
  if
   local.get $1
   i64.const 1
   i64.store
   i32.const 0
   return
  end
  i32.const 28
 )
 (func $src/wasi_snapshot_preview1/clock_time_get (param $0 i32) (param $1 i64) (param $2 i32) (result i32)
  local.get $0
  i32.eqz
  if
   local.get $2
   call $src/wasi_snapshot_preview1/sys_get_time
   i32.const 0
   return
  end
  i32.const 28
 )
 (func $src/wasi_snapshot_preview1/fd_advise (param $0 i32) (param $1 i64) (param $2 i64) (param $3 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/fd_close (param $0 i32) (result i32)
  i32.const 0
 )
 (func $src/wasi_snapshot_preview1/fd_fdstat_get (param $0 i32) (param $1 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/fd_filestat_set_size (param $0 i32) (param $1 i64) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/fd_pread (param $0 i32) (param $1 i32) (param $2 i32) (param $3 i64) (param $4 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/fd_prestat_dir_name (param $0 i32) (param $1 i32) (param $2 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/fd_read (param $0 i32) (param $1 i32) (param $2 i32) (param $3 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/fd_seek (param $0 i32) (param $1 i64) (param $2 i32) (param $3 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/fd_write (param $0 i32) (param $1 i32) (param $2 i32) (param $3 i32) (result i32)
  (local $4 i32)
  (local $5 i32)
  (local $6 i32)
  local.get $0
  i32.const 1
  i32.eq
  if
   loop $for-loop|0
    local.get $2
    local.get $4
    i32.gt_s
    if
     local.get $1
     local.get $4
     i32.const 3
     i32.shl
     i32.add
     local.tee $0
     i32.load offset=4
     local.set $6
     local.get $0
     i32.load
     local.get $6
     call $src/wasi_snapshot_preview1/sys_write_stdout
     local.get $5
     local.get $6
     i32.add
     local.set $5
     local.get $4
     i32.const 1
     i32.add
     local.set $4
     br $for-loop|0
    end
   end
   local.get $3
   local.get $5
   i32.store
   i32.const 0
   return
  else
   local.get $0
   i32.const 2
   i32.eq
   if
    loop $for-loop|1
     local.get $2
     local.get $4
     i32.gt_s
     if
      local.get $1
      local.get $4
      i32.const 3
      i32.shl
      i32.add
      local.tee $0
      i32.load offset=4
      local.set $6
      local.get $0
      i32.load
      local.get $6
      call $src/wasi_snapshot_preview1/sys_write_stderr
      local.get $5
      local.get $6
      i32.add
      local.set $5
      local.get $4
      i32.const 1
      i32.add
      local.set $4
      br $for-loop|1
     end
    end
    local.get $3
    local.get $5
    i32.store
    i32.const 0
    return
   end
  end
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/path_filestat_get (param $0 i32) (param $1 i32) (param $2 i32) (param $3 i32) (param $4 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/path_open (param $0 i32) (param $1 i32) (param $2 i32) (param $3 i32) (param $4 i32) (param $5 i64) (param $6 i64) (param $7 i32) (param $8 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/path_readlink (param $0 i32) (param $1 i32) (param $2 i32) (param $3 i32) (param $4 i32) (param $5 i32) (result i32)
  i32.const -1
 )
 (func $src/wasi_snapshot_preview1/poll_oneoff (param $0 i32) (param $1 i32) (param $2 i32) (param $3 i32) (result i32)
  i32.const 0
 )
 (func $src/wasi_snapshot_preview1/proc_exit (param $0 i32)
 )
 (func $src/wasi_snapshot_preview1/sched_yield (result i32)
  i32.const 0
 )
 (func $src/wasi_snapshot_preview1/random_get (param $0 i32) (param $1 i32) (result i32)
  local.get $0
  local.get $1
  call $src/wasi_snapshot_preview1/sys_get_random
  i32.const 0
 )
)
