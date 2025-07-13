using System.Collections.Generic;
using System.IO;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public enum SectionID
    {
        Custom = 0,
        Type = 1,
        Import = 2,
        Func = 3,
        Table = 4,
        Mem = 5,
        Global = 6,
        Export = 7,
        Start = 8,
        Elem = 9,
        Code = 10,
        Data = 11,
    }

    public abstract class Section
    {
        public uint BinaryOffset { get; set; }

        public uint BinaryLength { get; set; }

        public SectionID ID { get; protected set; }

        public abstract void Write(BinaryWriter wtr, IEnumerable<Section> sections);
    }

    public class UnknownSection : Section
    {
        public byte[] RawData { get; set; }

        public UnknownSection(BinaryReader rdr, SectionID id, uint size)
        {
            ID = id;
            RawData = rdr.ReadBytes((int)size);
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            wtr.Write(RawData);
        }
    }
}
