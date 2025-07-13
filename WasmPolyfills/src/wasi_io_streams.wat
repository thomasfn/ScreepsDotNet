(module
 (type $0 (func (param i32)))
 (type $1 (func (param i32) (result i32)))
 (export "[resource-drop]input-stream" (func $src/wasi_io_streams/input_stream_resource_drop))
 (export "[resource-drop]output-stream" (func $src/wasi_io_streams/input_stream_resource_drop))
 (export "[method]input-stream.subscribe" (func $src/wasi_io_streams/input_stream_subscribe))
 (export "[method]output-stream.subscribe" (func $src/wasi_io_streams/input_stream_subscribe))
 (func $src/wasi_io_streams/input_stream_resource_drop (param $0 i32)
 )
 (func $src/wasi_io_streams/input_stream_subscribe (param $0 i32) (result i32)
  i32.const -1
 )
)
