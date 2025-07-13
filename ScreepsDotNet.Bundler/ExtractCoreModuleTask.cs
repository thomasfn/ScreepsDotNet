using System.Linq;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using ScreepsDotNet.Bundler.Wasm;
using ScreepsDotNet.Bundler.Wasm.ComponentSections;

namespace ScreepsDotNet.Bundler
{
    public class ExtractCoreModuleTask : Task
    {
        [Required]
        public string InWasmFileName { get; set; } = null!;

        [Required]
        public string OutWasmFileName { get; set; } = null!;

        public override bool Execute()
        {
            WasmBinary inWasmBinary = new WasmBinary(InWasmFileName);
            WasmModule coreModule;
            if (inWasmBinary.Component != null)
            {
                coreModule = inWasmBinary.Component.Sections.OfType<CoreModuleSection>().First().Module!;
            }
            else
            {
                coreModule = inWasmBinary.Module!;
            }
            WasmBinary outWasmBinary = new WasmBinary(coreModule);
            outWasmBinary.Write(OutWasmFileName);
            return !Log.HasLoggedErrors;
        }
    }
}
