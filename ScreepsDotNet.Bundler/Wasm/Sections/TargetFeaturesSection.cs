using System.Collections.Generic;
using System.IO;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public class TargetFeaturesSection : Section
    {
        public List<KeyValuePair<string, char>> TargetFeatureEntries { get; } = new List<KeyValuePair<string, char>>();

        public TargetFeaturesSection()
        {
            ID = SectionID.Custom;
        }

        public TargetFeaturesSection(BinaryReader rdr, uint size)
            : this()
        {
            rdr.ReadVector(TargetFeatureEntries, r => ReadEntry(r));
        }

        private KeyValuePair<string, char> ReadEntry(BinaryReader rdr)
        {
            char prefix = (char)rdr.ReadByte();
            string feature = rdr.ReadVarString();
            return new KeyValuePair<string, char>(feature, prefix);
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            wtr.WriteVarString("target_features");
            wtr.WriteVector(TargetFeatureEntries, (w, x) => { w.Write((byte)x.Value); w.WriteVarString(x.Key); });
        }
    }
}