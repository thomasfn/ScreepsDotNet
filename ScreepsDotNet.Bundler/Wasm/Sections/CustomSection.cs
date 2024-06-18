using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public class CustomSection : Section
    {
        public string Name { get; set; }

        public byte[] Data { get; set; }

        public CustomSection(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        public CustomSection(BinaryReader rdr, uint size)
        {
            ID = SectionID.Custom;
            long endOffset = rdr.BaseStream.Position + size;
            Name = rdr.ReadVarString();
            int dataLength = (int)(endOffset - rdr.BaseStream.Position);
            Data = rdr.ReadBytes(dataLength);
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            wtr.WriteVarString(Name);
            wtr.Write(Data);
        }

        public override string ToString() => $"(custom \"{Name}\" {Data.Length})";
    }
}
