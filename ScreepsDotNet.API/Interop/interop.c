#include <assert.h>
#include <driver.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>

__attribute__((__import_module__("js"), __import_name__("js_bind_import")))
extern int js_bind_import(const char* moduleNamePtr, const char* importNamePtr, const void* importSpecPtr);

__attribute__((__import_module__("js"), __import_name__("js_invoke_import")))
extern int js_invoke_import(int importIndex, void* paramsBufferPtr);

__attribute__((__import_module__("js"), __import_name__("js_release_object_reference")))
extern int js_release_object_reference(void* jsHandle);

void interop_attach_internal_calls()
{
	mono_add_internal_call("ScreepsDotNet_Interop::BindImport", js_bind_import);
	mono_add_internal_call("ScreepsDotNet_Interop::InvokeImport", js_invoke_import);
	mono_add_internal_call("ScreepsDotNet_Interop::ReleaseObjectReference", js_release_object_reference);
}

MonoMethod* method_Init;
MonoMethod* method_Loop;

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

__attribute__((export_name("screepsdotnet_loop")))
void screepsdotnet_loop()
{
	if (!method_Loop)
	{
		method_Loop = lookup_dotnet_method("ScreepsDotNet.dll", "ScreepsDotNet", "Program", "Loop", -1);
		assert(method_Loop);
	}

	MonoObject* exception;
	MonoObject* result = mono_runtime_invoke(method_Loop, NULL, NULL, &exception);
	assert(!exception);
}
