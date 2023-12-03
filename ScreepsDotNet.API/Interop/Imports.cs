using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreepsDotNet.Interop
{
    [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
    internal static partial class Imports
    {
        [JSImport("object", "create")]
        internal static partial JSObject CreateObject(JSObject? prototypeObj);
    }
}
