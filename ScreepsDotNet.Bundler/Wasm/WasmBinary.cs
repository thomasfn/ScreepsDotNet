using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ScreepsDotNet.Bundler.Wasm.Sections;
using ScreepsDotNet.Bundler.Wasm.ComponentSections;

namespace ScreepsDotNet.Bundler.Wasm
{

    [Serializable]
    public class InvalidWasmException : Exception
    {
        public InvalidWasmException() { }
        public InvalidWasmException(string message) : base(message) { }
        public InvalidWasmException(string message, Exception inner) : base(message, inner) { }
        protected InvalidWasmException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class WasmBinary
    {
        public WasmModule? Module { get; private set; }

        public WasiComponent? Component { get; private set; }

        public WasmBinary(string filePath)
            : this(File.OpenRead(filePath))
        { }

        public WasmBinary(Stream stream)
        {
            using var rdr = new BinaryReader(stream, Encoding.UTF8, true);
            uint magic = rdr.ReadUInt32();
            if (magic != 0x6d736100) { throw new InvalidWasmException("Invalid magic number"); }
            int ver = rdr.ReadInt32();
            if (ver == 0x0001000d)
            {
                Component = new WasiComponent(stream);
            }
            else if (ver == 0x1)
            {
                Module = new WasmModule(stream);
            }
            else
            {
                throw new InvalidWasmException($"Unknown version");
            }
        }

        public WasmBinary(WasmModule module)
        {
            Module = module;
        }

        public WasmBinary(WasiComponent component)
        {
            Component = component;
        }

        public void Write(Stream stream)
        {
            using var wtr = new BinaryWriter(stream);
            wtr.Write(0x6d736100u);
            if (Module != null)
            {
                wtr.Write(1u);
                wtr.Flush();
                Module.Write(stream);
                return;
            }
            if (Component != null)
            {
                wtr.Write(0x0001000d);
                wtr.Flush();
                Component.Write(stream);
                return;
            }
        }

        public void Write(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            Write(stream);
        }
    }

    public class WasmModule
    {
        private readonly List<Section> sections;

        public bool WasiComponent { get; private set; }

        public TypeSection? TypeSection { get; private set; }

        public ImportSection? ImportSection { get; private set; }

        public FunctionSection? FunctionSection { get; private set; }

        public ExportSection? ExportSection { get; private set; }

        public CodeSection? CodeSection { get; private set; }

        public IEnumerable<Section> Sections => sections;

        public int SectionCount => sections.Count;

        internal WasmModule(Stream stream)
        {
            using var rdr = new BinaryReader(stream);
            sections = new List<Section>();
            while (stream.Position < stream.Length)
            {
                sections.Add(ReadSection(rdr));
            }
            long finalPos = stream.Position;
        }

        internal WasmModule(BinaryReader rdr, uint size)
        {
            sections = new List<Section>();
            long endOffset = rdr.BaseStream.Position + size;
            while (rdr.BaseStream.Position < endOffset)
            {
                sections.Add(ReadSection(rdr));
            }
        }

        private Section ReadSection(BinaryReader rdr)
        {
            long offset = rdr.BaseStream.Position;
            var id = (SectionID)rdr.ReadByte();
            uint size = rdr.ReadVarU32();
            long endOffset = rdr.BaseStream.Position + size;
            Section section;
            switch (id)
            {
                case SectionID.Type:
                    if (TypeSection != null) { throw new InvalidWasmException("Duplicate type section"); }
                    section = TypeSection = new TypeSection(rdr);
                    break;
                case SectionID.Import:
                    if (ImportSection != null) { throw new InvalidWasmException("Duplicate import section"); }
                    if (TypeSection == null) { throw new InvalidWasmException("Import section came before the type section"); }
                    section = ImportSection = new ImportSection(rdr, TypeSection);
                    break;
                case SectionID.Func:
                    if (FunctionSection != null) { throw new InvalidWasmException("Duplicate function section"); }
                    if (TypeSection == null) { throw new InvalidWasmException("Import section came before the function section"); }
                    section = FunctionSection = new FunctionSection(rdr, TypeSection);
                    break;
                case SectionID.Export:
                    if (ExportSection != null) { throw new InvalidWasmException("Duplicate export section"); }
                    section = ExportSection = new ExportSection(rdr);
                    break;
                case SectionID.Code:
                    if (CodeSection != null) { throw new InvalidWasmException("Duplicate code section"); }
                    section = CodeSection = new CodeSection(rdr);
                    break;
                case SectionID.Custom:
                    {
                        var name = rdr.ReadVarString();
                        uint dataLength = (uint)(endOffset - rdr.BaseStream.Position);
                        switch (name)
                        {
                            case "name": section = new NameSection(rdr, dataLength); break;
                            case "target_features": section = new TargetFeaturesSection(rdr, dataLength); break;
                            default:
                                {
                                    var data = rdr.ReadBytes((int)dataLength);
                                    section = new CustomSection(name, data);
                                    break;
                                }
                        }
                    }
                    break;
                default: section = new UnknownSection(rdr, id, size); break;
            }
            section.BinaryOffset = (uint)offset;
            section.BinaryLength = size;
            if (rdr.BaseStream.Position != endOffset) { throw new InvalidWasmException($"Unexpected offset after reading {id}"); }
            return section;
        }

        public void Write(Stream stream)
        {
            using var wtr = new BinaryWriter(stream);
            foreach (var section in sections)
            {
                byte[] sectionData;
                {
                    using var sectionStream = new MemoryStream();
                    using var sectionWtr = new BinaryWriter(sectionStream);
                    section.Write(sectionWtr, sections);
                    sectionWtr.Flush();
                    sectionData = sectionStream.ToArray();
                }
                wtr.Write((byte)section.ID);
                wtr.WriteVarU32((uint)sectionData.Length);
                wtr.Write(sectionData);
            }
        }
    }


    public class WasiComponent
    {
        private readonly List<ComponentSection> sections;

        public IEnumerable<ComponentSection> Sections => sections;

        internal WasiComponent(Stream stream)
        {
            using var rdr = new BinaryReader(stream);
            sections = new List<ComponentSection>();
            while (stream.Position < stream.Length)
            {
                sections.Add(ReadSection(rdr));
            }
            long finalPos = stream.Position;
        }

        private ComponentSection ReadSection(BinaryReader rdr)
        {
            long offset = rdr.BaseStream.Position;
            var id = (ComponentSectionID)rdr.ReadByte();
            uint size = rdr.ReadVarU32();
            long endOffset = rdr.BaseStream.Position + size;
            ComponentSection section;
            switch (id)
            {
                case ComponentSectionID.CoreModule: section = new CoreModuleSection(rdr, size); break;
                default: section = new UnknownComponentSection(rdr, id, size); break;
            }
            section.BinaryOffset = (uint)offset;
            section.BinaryLength = size;
            if (rdr.BaseStream.Position != endOffset) { throw new InvalidWasmException($"Unexpected offset after reading {id}"); }
            return section;
        }

        public void Write(Stream stream)
        {
            using var wtr = new BinaryWriter(stream);
            foreach (var section in sections)
            {
                byte[] sectionData;
                {
                    using var sectionStream = new MemoryStream();
                    using var sectionWtr = new BinaryWriter(sectionStream);
                    section.Write(sectionWtr, sections);
                    sectionWtr.Flush();
                    sectionData = sectionStream.ToArray();
                }
                wtr.Write((byte)section.ID);
                wtr.WriteVarU32((uint)sectionData.Length);
                wtr.Write(sectionData);
            }
        }
    }
}
