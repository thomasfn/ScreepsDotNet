using System;

namespace ScreepsDotNet.Interop
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class JSImportAttribute : Attribute
    {
        public readonly string ImportName;
        public readonly string ModuleName;

        public JSImportAttribute(string importName, string moduleName)
        {
            ImportName = importName;
            ModuleName = moduleName;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class JSMarshalAsDataViewAttribute : Attribute
    {
        public JSMarshalAsDataViewAttribute()
        {
            
        }
    }
}
