using System;
using System.Collections.Generic;
using System.IO;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public enum NameSubsectionType : byte
    {
        ModuleName = 0,
        FunctionNames = 1,
        LocalNames = 2
    }

    public abstract class NameSubsection
    {
        public virtual NameSubsectionType Type { get; }

        public void Write(BinaryWriter wtr)
        {
            wtr.Write((byte)Type);
            byte[] subsectionData;
            using (var innerStrm = new MemoryStream())
            {
                using var innerWtr = new BinaryWriter(innerStrm);
                WriteSubsection(innerWtr);
                innerWtr.Flush();
                subsectionData = innerStrm.ToArray();
            }
            wtr.WriteVarU32((uint)subsectionData.Length);
            wtr.Write(subsectionData);
        }

        protected abstract void WriteSubsection(BinaryWriter wtr);
    }

    public class ModuleNameSubsection : NameSubsection
    {
        public override NameSubsectionType Type { get => NameSubsectionType.ModuleName; }

        public string ModuleName { get; set; }

        public ModuleNameSubsection(string moduleName)
        {
            ModuleName = moduleName;
        }

        public ModuleNameSubsection(BinaryReader rdr)
        {
            _ = rdr.ReadVarU32();
            ModuleName = rdr.ReadVarString();
        }

        protected override void WriteSubsection(BinaryWriter wtr)
        {
            wtr.WriteVarString(ModuleName);
        }
    }

    public class NameMap
    {
        
    }

    public class FunctionNameSubsection : NameSubsection
    {
        public override NameSubsectionType Type { get => NameSubsectionType.FunctionNames; }

        public List<KeyValuePair<uint, string>> NameMap { get; } = new List<KeyValuePair<uint, string>>();

        public FunctionNameSubsection()
        {
            
        }

        public FunctionNameSubsection(BinaryReader rdr)
        {
            _ = rdr.ReadVarU32();
            rdr.ReadVector(NameMap, r => new KeyValuePair<uint, string>(r.ReadVarU32(), r.ReadVarString()));
        }

        protected override void WriteSubsection(BinaryWriter wtr)
        {
            wtr.WriteVector(NameMap, (w, x) => { w.WriteVarU32(x.Key); w.WriteVarString(x.Value); });
        }
    }

    public class UnknownNameSubsection : NameSubsection
    {
        private NameSubsectionType type;

        public override NameSubsectionType Type { get => type; }

        public byte[] Data { get; set; }

        public UnknownNameSubsection(NameSubsectionType type, byte[] data)
        {
            this.type = type;
            Data = data;
        }

        public UnknownNameSubsection(NameSubsectionType type, BinaryReader rdr)
        {
            this.type = type;
            uint size = rdr.ReadVarU32();
            Data = rdr.ReadBytes((int)size);
        }

        protected override void WriteSubsection(BinaryWriter wtr)
        {
            wtr.Write(Data);
        }
    }

    public class NameSection : Section
    {
        public List<NameSubsection> Subsections { get; } = new List<NameSubsection>();

        public NameSection(BinaryReader rdr, uint size)
        {
            long expectedEndOffset = rdr.BaseStream.Position + size;
            while (rdr.BaseStream.Position < expectedEndOffset)
            {
                Subsections.Add(ReadSubsection(rdr));
            }
        }

        private NameSubsection ReadSubsection(BinaryReader rdr)
        {
            var subsectionType = (NameSubsectionType)rdr.ReadByte();
            switch (subsectionType)
            {
                case NameSubsectionType.ModuleName: return new ModuleNameSubsection(rdr);
                case NameSubsectionType.FunctionNames: return new FunctionNameSubsection(rdr);
                default: return new UnknownNameSubsection(subsectionType, rdr);
            }
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            wtr.WriteVarString("name");
            foreach (var subsection in Subsections)
            {
                subsection.Write(wtr);
            }
        }
    }
}
