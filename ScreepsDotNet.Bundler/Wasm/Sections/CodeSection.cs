using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public class Local
    {
        public uint N { get; set; }

        public ValueType Type { get; set; }

        public Local(uint n, ValueType type)
        {
            N = n;
            Type = type;
        }

        public Local(BinaryReader rdr)
        {
            N = rdr.ReadVarU32();
            Type = new ValueType((TypeTag)rdr.ReadByte());
        }

        public void Write(BinaryWriter wtr)
        {
            wtr.WriteVarU32(N);
            Type.Write(wtr);
        }
    }

    public enum Opcode : byte
    {

    }

    public class Code
    {
        public List<Local> Locals { get; } = new List<Local>();

        public byte[] ExpressionByteCode { get; set; }

        public Code(BinaryReader rdr)
        {
            uint size = rdr.ReadVarU32();
            long endOffset = rdr.BaseStream.Position + size;
            rdr.ReadVector(Locals, r => new Local(r));
            int bytecodeLen = (int)(endOffset - rdr.BaseStream.Position);
            ExpressionByteCode = rdr.ReadBytes(bytecodeLen);
            if (ExpressionByteCode[ExpressionByteCode.Length - 1] != 0x0b) { throw new InvalidWasmException($"Last instruction of expression was not"); }
        }

        public void Write(BinaryWriter wtr)
        {
            byte[] localsData;
            using (var innerStrm = new MemoryStream())
            {
                using var innerWtr = new BinaryWriter(innerStrm);
                innerWtr.WriteVector(Locals, (w, x) => x.Write(w));
                innerWtr.Flush();
                localsData = innerStrm.ToArray();
            }
            wtr.WriteVarU32((uint)(localsData.Length + ExpressionByteCode.Length));
            wtr.Write(localsData);
            wtr.Write(ExpressionByteCode);
        }
    }

    public class CodeSection : Section
    {
        public List<Code> Codes { get; } = new List<Code>();

        public CodeSection()
        {
            ID = SectionID.Code;
        }

        public CodeSection(BinaryReader rdr)
            : this()
        {
            rdr.ReadVector(Codes, r => new Code(r));
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            wtr.WriteVector(Codes, (w, x) => x.Write(wtr));
        }
    }
}
