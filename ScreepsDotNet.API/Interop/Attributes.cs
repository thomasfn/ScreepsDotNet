using System;

namespace ScreepsDotNet.Interop
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class JSImportAttribute : Attribute
    {
        public readonly string ModuleName;
        public readonly string ImportName;

        public JSImportAttribute(string moduleName, string importName)
        {
            ModuleName = moduleName;
            ImportName = importName;
        }
    }
}
