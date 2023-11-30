#include <assert.h>
#include <driver.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>

__attribute__((__import_module__("js"), __import_name__("js_bind_import")))
extern int js_bind_import(const char* moduleNamePtr, const char* importNamePtr, const void* importSpecPtr);

__attribute__((__import_module__("js"), __import_name__("js_invoke_import")))
extern int js_invoke_import(int importIndex, void* paramsBufferPtr);

void interop_attach_internal_calls()
{
	mono_add_internal_call("ScreepsDotNet_Interop::BindImport", js_bind_import);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport", js_invoke_import);
}

MonoMethod* method_Init;
MonoMethod* method_Tick;

__attribute__((export_name("screepsdotnet_init")))
void screepsdotnet_init()
{
	interop_attach_internal_calls();

	if (!method_Init)
	{
		method_Init = lookup_dotnet_method("ScreepsDotNet.dll", "ScreepsDotNet", "Program", "Init", -1);
		assert(method_Init);
	}

	MonoObject* exception;
	MonoObject* result = mono_runtime_invoke(method_Init, NULL, NULL, &exception);
	assert(!exception);
}

__attribute__((export_name("screepsdotnet_tick")))
void screepsdotnet_tick()
{
	if (!method_Tick)
	{
		method_Tick = lookup_dotnet_method("ScreepsDotNet.dll", "ScreepsDotNet", "Program", "Tick", -1);
		assert(method_Tick);
	}

	MonoObject* exception;
	MonoObject* result = mono_runtime_invoke(method_Tick, NULL, NULL, &exception);
	assert(!exception);
}
