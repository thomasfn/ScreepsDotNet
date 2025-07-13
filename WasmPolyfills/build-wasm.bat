@echo off

set WASMAS="..\ScreepsDotNet.Bundler\build\wasm\binaryen-version_123\bin\wasm-as.exe"
set DEST="..\ScreepsDotNet.Bundler\build\polyfills"

call %WASMAS% .\src\wasi_snapshot_preview1.wat -o %DEST%\wasi_snapshot_preview1.wasm
call %WASMAS% .\src\wasi_io_poll.wat -o %DEST%\wasi_io_poll.wasm
call %WASMAS% .\src\wasi_io_streams.wat -o %DEST%\wasi_io_streams.wasm
call %WASMAS% .\src\wasi_sockets_udp.wat -o %DEST%\wasi_sockets_udp.wasm
call %WASMAS% .\src\wasi_sockets_tcp.wat -o %DEST%\wasi_sockets_tcp.wasm
call %WASMAS% .\src\wasi_clocks_monotonic_clock.wat -o %DEST%\wasi_clocks_monotonic_clock.wasm
