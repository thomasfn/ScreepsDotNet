using System.Collections.Generic;
using System.IO;

namespace ScreepsDotNet.Bundler.Wasm.ComponentSections
{
    public enum ComponentSectionID
    {
        Custom = 0,
        CoreModule = 1,
        CoreInstances = 2,
        CoreTypes = 3,
        Component = 4,
        Instances = 5,
        Aliases = 6,
        Types = 7,
        Canons = 8,
        Start = 9,
        Imports = 10,
        Exports = 11,
        Values = 12
    }

    public abstract class ComponentSection
    {
        public uint BinaryOffset { get; set; }

        public uint BinaryLength { get; set; }

        public ComponentSectionID ID { get; protected set; }

        public abstract void Write(BinaryWriter wtr, IEnumerable<ComponentSection> sections);
    }

    public class UnknownComponentSection : ComponentSection
    {
        public byte[] RawData { get; set; }

        public UnknownComponentSection(BinaryReader rdr, ComponentSectionID id, uint size)
        {
            ID = id;
            RawData = rdr.ReadBytes((int)size);
        }

        public override void Write(BinaryWriter wtr, IEnumerable<ComponentSection> sections)
        {
            wtr.Write(RawData);
        }
    }
}
