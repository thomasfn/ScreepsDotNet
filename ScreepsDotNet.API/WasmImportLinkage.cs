namespace System.Runtime.InteropServices
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class WasmImportLinkageAttribute : Attribute
    {
        public WasmImportLinkageAttribute() { }
    }
}
