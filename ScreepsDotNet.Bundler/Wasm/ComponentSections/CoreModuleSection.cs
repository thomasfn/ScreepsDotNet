using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScreepsDotNet.Bundler.Wasm.ComponentSections
{
    public class CoreModuleSection : ComponentSection
    {
        public WasmModule? Module { get; set; }

        public CoreModuleSection()
        {
            ID = ComponentSectionID.CoreModule;
        }

        public CoreModuleSection(BinaryReader rdr, uint size)
        {
            ID = ComponentSectionID.CoreModule;
            uint magic = rdr.ReadUInt32();
            if (magic != 0x6d736100) { throw new InvalidWasmException("Invalid magic number"); }
            int ver = rdr.ReadInt32();
            if (ver != 0x1) { throw new InvalidWasmException("Invalid version"); }
            Module = new WasmModule(rdr, size - 8);
        }

        public override void Write(BinaryWriter wtr, IEnumerable<ComponentSection> sections)
        {
            wtr.Flush();
            Module?.Write(wtr.BaseStream);
        }
    }
}
