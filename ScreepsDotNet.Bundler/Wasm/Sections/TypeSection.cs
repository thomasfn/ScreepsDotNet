using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScreepsDotNet.Bundler.Wasm.Sections
{
    public enum TypeTag : byte
    {
        Function = 0x60,
        I32 = 0x7f,
        I64 = 0x7e,
        F32 = 0x7d,
        F64 = 0x7c,
        V128 = 0x7b,
        FuncRef = 0x70,
        ExternRef = 0x6f
    }

    public abstract class BaseType
    {
        public virtual TypeTag Tag { get; }

        public abstract void Write(BinaryWriter wtr);
    }

    public class FunctionType : BaseType, System.IEquatable<FunctionType?>
    {
        public override TypeTag Tag { get => TypeTag.Function; }

        public List<ValueType> Params { get; } = new List<ValueType>();

        public List<ValueType> Results { get; } = new List<ValueType>();

        public FunctionType() { }

        public FunctionType(BinaryReader rdr)
        {
            rdr.ReadVector(Params, r => new ValueType((TypeTag)r.ReadByte()));
            rdr.ReadVector(Results, r => new ValueType((TypeTag)r.ReadByte()));
        }

        public override void Write(BinaryWriter wtr)
        {
            wtr.Write((byte)Tag);
            wtr.WriteVector(Params, (w, x) => x.Write(w));
            wtr.WriteVector(Results, (w, x) => x.Write(w));
        }

        public override string ToString()
            => $"(func{(Params.Count > 0 ? $" (param {string.Join(", ", Params)})" : "")}{(Results.Count > 0 ? $" (result {string.Join(", ", Results)})" : "")})";

        public override bool Equals(object? obj) => Equals(obj as FunctionType);

        public bool Equals(FunctionType? other)
        {
            if (other == null) { return false; }
            if (Params.Count != other.Params.Count) { return false; }
            for (int i = 0; i <Params.Count; ++i)
            {
                if (Params[i] != other.Params[i]) { return false; }
            }
            if (Results.Count != other.Results.Count) { return false; }
            for (int i = 0; i < Results.Count; ++i)
            {
                if (Results[i] != other.Results[i]) { return false; }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = 1155513406;
            foreach (var param in Params)
            {
                hashCode = hashCode * -1521134295 + param.GetHashCode();
            }
            foreach (var result in Results)
            {
                hashCode = hashCode * -1521134295 + result.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(FunctionType? left, FunctionType? right) => EqualityComparer<FunctionType>.Default.Equals(left, right);

        public static bool operator !=(FunctionType? left, FunctionType? right) => !(left == right);
    }

    public class ValueType : BaseType, System.IEquatable<ValueType?>
    {
        public override TypeTag Tag { get; }

        public ValueType(TypeTag tag)
        {
            Tag = tag;
        }

        public override void Write(BinaryWriter wtr)
        {
            wtr.Write((byte)Tag);
        }

        public override string ToString()
            => Tag switch { TypeTag.I32 => "i32", TypeTag.I64 => "i64", TypeTag.F32 => "f32", TypeTag.F64 => "f64", _ => "?" };

        public override bool Equals(object? obj) => Equals(obj as ValueType);

        public bool Equals(ValueType? other) => !(other is null) && Tag == other.Tag;

        public override int GetHashCode() => 1005349675 + Tag.GetHashCode();

        public static bool operator ==(ValueType? left, ValueType? right) => EqualityComparer<ValueType>.Default.Equals(left, right);

        public static bool operator !=(ValueType? left, ValueType? right) => !(left == right);
    }

    public class TypeSection : Section
    {
        public List<BaseType> Types { get; } = new List<BaseType>();

        public TypeSection()
        {
            ID = SectionID.Type;
        }

        public TypeSection(BinaryReader rdr)
            : this()
        {
            rdr.ReadVector(Types, ReadType);
        }

        private BaseType ReadType(BinaryReader rdr)
        {
            var tag = (TypeTag)rdr.ReadByte();
            switch (tag)
            {
                case TypeTag.Function:
                    return new FunctionType(rdr);
                case TypeTag.I32:
                case TypeTag.I64:
                case TypeTag.F32:
                case TypeTag.F64:
                    return new ValueType(tag);
                default:
                    throw new InvalidWasmException($"Unknown type tag '{tag}'");
            }
        }

        public override void Write(BinaryWriter wtr, IEnumerable<Section> sections)
        {
            wtr.WriteVector(Types, (w, x) => x.Write(w));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            int i = 0;
            foreach (var type in Types)
            {
                sb.Append("(type $");
                sb.Append(i);
                sb.Append(" ");
                sb.Append(type.ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
