using System.Collections.Generic;
using System.IO;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public enum ExportTag : byte
    {
        Func = 0,
        Table = 1,
        Mem = 2,
        Global = 3
    }

    public class Export
    {
        public string Name { get; set; }

        public ExportTag Tag { get; set; }

        public uint Index { get; set; }

        public Export(string name, ExportTag tag, uint index)
        {
            Name = name;
            Tag = tag;
            Index = index;
        }

        public Export(BinaryReader rdr)
        {
            Name = rdr.ReadVarString();
            Tag = (ExportTag)rdr.ReadByte();
            Index = rdr.ReadVarU32();
        }

        public virtual void Write(BinaryWriter wtr)
        {
            wtr.WriteVarString(Name);
            wtr.Write((byte)Tag);
            wtr.WriteVarU32(Index);
        }
    }

    public class ExportSection : Section
    {
        public List<Export> Exports { get; } = new List<Export>();

        public ExportSection()
        {
            ID = SectionID.Export;
        }

        public ExportSection(BinaryReader rdr)
            : this()
        {
            rdr.ReadVector(Exports, r => new Export(r));
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            wtr.WriteVector(Exports, (w, x) => x.Write(wtr));
        }
    }
}
