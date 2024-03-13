#include <assert.h>
#include <driver.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>

const char* dotnet_wasi_getentrypointassemblyname();

__attribute__((__import_module__("js"), __import_name__("js_bind_import")))
extern int js_bind_import(const char* moduleNamePtr, const char* importNamePtr, const void* importSpecPtr);

__attribute__((__import_module__("js"), __import_name__("js_invoke_import")))
extern int js_invoke_import(int importIndex, void* paramsBufferPtr);

__attribute__((__import_module__("js"), __import_name__("js_release_object_reference")))
extern int js_release_object_reference(void* jsHandle);

__attribute__((__import_module__("js"), __import_name__("js_set_name")))
extern void js_set_name(int nameIndex, const char* valuePtr);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_i")))
extern int js_invoke_i_i(int importIndex, int paramA);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_ii")))
extern int js_invoke_i_ii(int importIndex, int paramA, int paramB);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_iii")))
extern int js_invoke_i_iii(int importIndex, int paramA, int paramB, int paramC);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_o")))
extern int js_invoke_i_o(int importIndex, int paramA);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_oi")))
extern int js_invoke_i_oi(int importIndex, int paramA, int paramB);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_on")))
extern int js_invoke_i_on(int importIndex, int paramA, int paramB);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_oii")))
extern int js_invoke_i_oii(int importIndex, int paramA, int paramB, int paramC);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_oo")))
extern int js_invoke_i_oo(int importIndex, int paramA, int paramB);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_ooi")))
extern int js_invoke_i_ooi(int importIndex, int paramA, int paramB, int paramC);

__attribute__((__import_module__("js"), __import_name__("js_invoke_i_ooii")))
extern int js_invoke_i_ooii(int importIndex, int paramA, int paramB, int paramC, int paramD);

__attribute__((__import_module__("js"), __import_name__("js_invoke_d_v")))
extern double js_invoke_d_v(int importIndex);

void interop_attach_internal_calls()
{
	mono_add_internal_call("ScreepsDotNet_Interop::BindImport", js_bind_import);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport", js_invoke_import);
	mono_add_internal_call("ScreepsDotNet_Interop::ReleaseObjectReference", js_release_object_reference);
	mono_add_internal_call("ScreepsDotNet_Interop::SetName", js_set_name);

	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_i", js_invoke_i_i);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_ii", js_invoke_i_ii);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_iii", js_invoke_i_iii);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_o", js_invoke_i_o);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_oi", js_invoke_i_oi);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_on", js_invoke_i_on);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_oii", js_invoke_i_oii);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_oo", js_invoke_i_oo);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_ooi", js_invoke_i_ooi);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_i_ooii", js_invoke_i_ooii);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport_d_v", js_invoke_d_v);
}

MonoMethod* method_Init;
MonoMethod* method_Loop;

__attribute__((export_name("screepsdotnet_init")))
void screepsdotnet_init()
{
	interop_attach_internal_calls();

	if (!method_Init)
	{
		method_Init = lookup_dotnet_method(dotnet_wasi_getentrypointassemblyname(), "ScreepsDotNet", "Program", "Init", -1);
		assert(method_Init);
	}

	MonoObject* exception;
	MonoObject* result = mono_runtime_invoke(method_Init, NULL, NULL, &exception);
	assert(!exception);
}

__attribute__((export_name("screepsdotnet_loop")))
void screepsdotnet_loop()
{
	if (!method_Loop)
	{
		method_Loop = lookup_dotnet_method(dotnet_wasi_getentrypointassemblyname(), "ScreepsDotNet", "Program", "Loop", -1);
		assert(method_Loop);
	}

	MonoObject* exception;
	MonoObject* result = mono_runtime_invoke(method_Loop, NULL, NULL, &exception);
	assert(!exception);
}
