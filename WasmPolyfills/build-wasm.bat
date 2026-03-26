@echo off

set EMCC="..\ScreepsDotNet.Bundler\build\wasm\emsdk\upstream\emscripten\emcc.bat"
set WASMDIS="..\ScreepsDotNet.Bundler\build\wasm\binaryen-version_123\bin\wasm-dis.exe"
set DEST="..\ScreepsDotNet.Bundler\build\polyfills"

mkdir .\wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_environment.c -o .\wasm\wasi_cli_environment.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_exit.c -o .\wasm\wasi_cli_exit.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_stderr.c -o .\wasm\wasi_cli_stderr.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_stdin.c -o .\wasm\wasi_cli_stdin.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_stdout.c -o .\wasm\wasi_cli_stdout.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_terminal_input.c -o .\wasm\wasi_cli_terminal_input.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_terminal_output.c -o .\wasm\wasi_cli_terminal_output.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_terminal_stderr.c -o .\wasm\wasi_cli_terminal_stderr.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_terminal_stdin.c -o .\wasm\wasi_cli_terminal_stdin.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_cli_terminal_stdout.c -o .\wasm\wasi_cli_terminal_stdout.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_clocks_monotonic_clock.c -o .\wasm\wasi_clocks_monotonic_clock.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_clocks_wall_clock.c -o .\wasm\wasi_clocks_wall_clock.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_filesystem_preopens.c -o .\wasm\wasi_filesystem_preopens.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_filesystem_types.c -o .\wasm\wasi_filesystem_types.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_io_error.c -o .\wasm\wasi_io_error.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_io_poll.c -o .\wasm\wasi_io_poll.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_io_streams.c -o .\wasm\wasi_io_streams.wasm
call %EMCC% --no-entry -Os -sMALLOC=none -sSIDE_MODULE=1 -sWASM_BIGINT=1 -mno-bulk-memory -mno-nontrapping-fptoint -mno-sign-ext .\src\wasi_random.c -o .\wasm\wasi_random.wasm

call %WASMDIS% .\wasm\wasi_cli_environment.wasm -o .\wasm\wasi_cli_environment.wat
call %WASMDIS% .\wasm\wasi_cli_exit.wasm -o .\wasm\wasi_cli_exit.wat
call %WASMDIS% .\wasm\wasi_cli_stderr.wasm -o .\wasm\wasi_cli_stderr.wat
call %WASMDIS% .\wasm\wasi_cli_stdin.wasm -o .\wasm\wasi_cli_stdin.wat
call %WASMDIS% .\wasm\wasi_cli_stdout.wasm -o .\wasm\wasi_cli_stdout.wat
call %WASMDIS% .\wasm\wasi_cli_terminal_input.wasm -o .\wasm\wasi_cli_terminal_input.wat
call %WASMDIS% .\wasm\wasi_cli_terminal_output.wasm -o .\wasm\wasi_cli_terminal_output.wat
call %WASMDIS% .\wasm\wasi_cli_terminal_stderr.wasm -o .\wasm\wasi_cli_terminal_stderr.wat
call %WASMDIS% .\wasm\wasi_cli_terminal_stdin.wasm -o .\wasm\wasi_cli_terminal_stdin.wat
call %WASMDIS% .\wasm\wasi_cli_terminal_stdout.wasm -o .\wasm\wasi_cli_terminal_stdout.wat
call %WASMDIS% .\wasm\wasi_clocks_monotonic_clock.wasm -o .\wasm\wasi_clocks_monotonic_clock.wat
call %WASMDIS% .\wasm\wasi_clocks_wall_clock.wasm -o .\wasm\wasi_clocks_wall_clock.wat
call %WASMDIS% .\wasm\wasi_filesystem_preopens.wasm -o .\wasm\wasi_filesystem_preopens.wat
call %WASMDIS% .\wasm\wasi_filesystem_types.wasm -o .\wasm\wasi_filesystem_types.wat
call %WASMDIS% .\wasm\wasi_io_error.wasm -o .\wasm\wasi_io_error.wat
call %WASMDIS% .\wasm\wasi_io_poll.wasm -o .\wasm\wasi_io_poll.wat
call %WASMDIS% .\wasm\wasi_io_streams.wasm -o .\wasm\wasi_io_streams.wat
call %WASMDIS% .\wasm\wasi_random.wasm -o .\wasm\wasi_random.wat

copy .\wasm\wasi_cli_environment.wasm %DEST%\wasi_cli_environment.wasm
copy .\wasm\wasi_cli_exit.wasm %DEST%\wasi_cli_exit.wasm
copy .\wasm\wasi_cli_stderr.wasm %DEST%\wasi_cli_stderr.wasm
copy .\wasm\wasi_cli_stdin.wasm %DEST%\wasi_cli_stdin.wasm
copy .\wasm\wasi_cli_stdout.wasm %DEST%\wasi_cli_stdout.wasm
copy .\wasm\wasi_cli_terminal_input.wasm %DEST%\wasi_cli_terminal_input.wasm
copy .\wasm\wasi_cli_terminal_output.wasm %DEST%\wasi_cli_terminal_output.wasm
copy .\wasm\wasi_cli_terminal_stderr.wasm %DEST%\wasi_cli_terminal_stderr.wasm
copy .\wasm\wasi_cli_terminal_stdin.wasm %DEST%\wasi_cli_terminal_stdin.wasm
copy .\wasm\wasi_cli_terminal_stdout.wasm %DEST%\wasi_cli_terminal_stdout.wasm
copy .\wasm\wasi_clocks_monotonic_clock.wasm %DEST%\wasi_clocks_monotonic_clock.wasm
copy .\wasm\wasi_clocks_wall_clock.wasm %DEST%\wasi_clocks_wall_clock.wasm
copy .\wasm\wasi_filesystem_preopens.wasm %DEST%\wasi_filesystem_preopens.wasm
copy .\wasm\wasi_filesystem_types.wasm %DEST%\wasi_filesystem_types.wasm
copy .\wasm\wasi_io_error.wasm %DEST%\wasi_io_error.wasm
copy .\wasm\wasi_io_poll.wasm %DEST%\wasi_io_poll.wasm
copy .\wasm\wasi_io_streams.wasm %DEST%\wasi_io_streams.wasm
copy .\wasm\wasi_random.wasm %DEST%\wasi_random.wasm
