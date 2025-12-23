using System;

namespace ScreepsDotNet.Interop
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class JSImportAttribute(string importName, string moduleName) : Attribute
    {
        public readonly string ImportName = importName;
        public readonly string ModuleName = moduleName;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class JSMarshalAsDataViewAttribute() : Attribute
    {

    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    [System.AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class JSStructAttribute() : Attribute
    {

    }

    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    [System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class JSStructFieldAttribute(string fieldName) : Attribute
    {
        public readonly string FieldName = fieldName;
    }
}
