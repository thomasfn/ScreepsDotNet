using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public enum ImportDescTag : byte
    {
        Func = 0,
        Table = 1,
        Mem = 2,
        Global = 3
    }

    public abstract class BaseImport
    {
        public string Module { get; set; }

        public string Name { get; set; }

        protected BaseImport(string module, string name)
        {
            Module = module;
            Name = name;
        }

        public virtual void Write(BinaryWriter wtr, TypeSection typeSection)
        {
            wtr.WriteVarString(Module);
            wtr.WriteVarString(Name);
        }
    }

    public class FunctionImport : BaseImport
    {
        public FunctionType Type { get; set; }

        public FunctionImport(string module, string name, FunctionType type)
            : base(module, name)
        {
            Type = type;
        }

        public FunctionImport(string module, string name, BinaryReader rdr, TypeSection typeSection)
            : base(module, name)
        {
            uint typeIndex = rdr.ReadVarU32();
            var type = typeSection.Types[(int)typeIndex] as FunctionType;
            if (type == null) { throw new InvalidWasmException($"Function import {module}.{name} references unknown type {typeIndex}"); }
            Type = type;
        }

        public override void Write(BinaryWriter wtr, TypeSection typeSection)
        {
            base.Write(wtr, typeSection);
            wtr.Write((byte)ImportDescTag.Func);
            int idx = typeSection.Types.IndexOf(Type);
            if (idx == -1) { throw new InvalidOperationException($"Type not present in type section"); }
            wtr.WriteVarU32((uint)idx);
        }

        public override string ToString()
            => $"(import \"{Module}\" \"{Name}\" {Type})";
    }

    public readonly struct Limits
    {
        public readonly uint Min;
        public readonly uint? Max;

        public Limits(uint min, uint? max)
        {
            Min = min;
            Max = max;
        }

        public Limits(BinaryReader rdr)
        {
            var flags = rdr.ReadByte();
            Min = rdr.ReadVarU32();
            Max = (flags & 1) == 1 ? (uint?)rdr.ReadVarU32() : null;
        }

        public void Write(BinaryWriter wtr)
        {
            wtr.WriteVarU32(Max != null ? 1u : 0u);
            wtr.WriteVarU32(Min);
            if (Max != null) { wtr.WriteVarU32(Max.Value); }
        }
    }

    public class MemoryImport : BaseImport
    {
        public Limits Limits { get; set; }

        public MemoryImport(string module, string name, Limits limits)
            : base(module, name)
        {
            Limits = limits;
        }

        public MemoryImport(string module, string name, BinaryReader rdr)
            : base(module, name)
        {
            Limits = new Limits(rdr);
        }

        public override void Write(BinaryWriter wtr, TypeSection typeSection)
        {
            base.Write(wtr, typeSection);
            wtr.Write((byte)ImportDescTag.Mem);
            Limits.Write(wtr);
        }

        public override string ToString()
            => $"(memory \"{Module}\" \"{Name}\")";
    }

    public class TableImport : BaseImport
    {
        public Limits Limits { get; set; }

        public BaseType RefType { get; set; }

        public TableImport(string module, string name, Limits limits, BaseType refType)
            : base(module, name)
        {
            Limits = limits;
            RefType = refType;
        }

        public TableImport(string module, string name, BinaryReader rdr)
            : base(module, name)
        {
            var tag = (TypeTag)rdr.ReadByte();
            switch (tag)
            {
                case TypeTag.FuncRef:
                    RefType = new FuncRefType();
                    break;
                case TypeTag.ExternRef:
                    RefType = new ExternRefType();
                    break;
                default:
                    throw new InvalidWasmException($"Unknown type tag '{tag}'");
            }
            Limits = new Limits(rdr);
        }

        public override void Write(BinaryWriter wtr, TypeSection typeSection)
        {
            base.Write(wtr, typeSection);
            wtr.Write((byte)ImportDescTag.Mem);
            Limits.Write(wtr);
            RefType.Write(wtr);
        }

        public override string ToString()
            => $"(table \"{Module}\" \"{Name}\" {RefType})";
    }

    public class ImportSection : Section
    {
        public List<BaseImport> Imports { get; } = new List<BaseImport>();

        public ImportSection()
        {
            ID = SectionID.Import;
        }

        public ImportSection(BinaryReader rdr, TypeSection typeSection)
            : this()
        {
            rdr.ReadVector(Imports, r => ReadImport(r, typeSection));
        }

        private BaseImport ReadImport(BinaryReader rdr, TypeSection typeSection)
        {
            string module = rdr.ReadVarString();
            string name = rdr.ReadVarString();
            var descTag = (ImportDescTag)rdr.ReadByte();
            switch (descTag)
            {
                case ImportDescTag.Func: return new FunctionImport(module, name, rdr, typeSection);
                case ImportDescTag.Mem: return new MemoryImport(module, name, rdr);
                case ImportDescTag.Table: return new TableImport(module, name, rdr);
                default: throw new InvalidWasmException($"Unknown import description tag {descTag}");
            }
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            var typeSection = sections.OfType<TypeSection>().Single();
            wtr.WriteVector(Imports, (w, x) => x.Write(wtr, typeSection));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var import in Imports)
            {
                sb.Append(import.ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
