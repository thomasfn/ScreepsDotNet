using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public class FunctionSection : Section
    {
        public List<FunctionType> Types { get; } = new List<FunctionType>();

        public FunctionSection()
        {
            ID = SectionID.Func;
        }

        public FunctionSection(BinaryReader rdr, TypeSection typeSection)
            : this()
        {
            rdr.ReadVector(Types, r =>
            {
                uint typeIndex = r.ReadVarU32();
                if (!(typeSection.Types[(int)typeIndex] is FunctionType functionType)) { throw new InvalidWasmException($"Function referred to invalid type"); }
                return functionType;
            });
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            var typeSection = sections.OfType<TypeSection>().Single();
            wtr.WriteVector(Types, (w, t) =>
            {
                int idx = typeSection.Types.IndexOf(t);
                w.WriteVarU32((uint)idx);
            });
        }
    }
}
