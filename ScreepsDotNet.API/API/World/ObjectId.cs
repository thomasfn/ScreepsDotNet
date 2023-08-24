using System;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Represents the id of a room object.
    /// Faster than the string counterpart for equality, hashing etc.
    /// </summary>
    public readonly struct ObjectId : IEquatable<ObjectId>
    {
        private readonly int a, b, c, h;

        public bool IsValid => a != 0 || b != 0 || c != 0;

        internal ObjectId(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            h = HashCode.Combine(a, b, c);
        }

        public ObjectId(ReadOnlySpan<char> idStr)
        {
            if (idStr.Length != 24) { throw new ArgumentException("Span must be 24 long", nameof(idStr)); }
            a = int.Parse(idStr[0..8], System.Globalization.NumberStyles.AllowHexSpecifier);
            b = int.Parse(idStr[8..16], System.Globalization.NumberStyles.AllowHexSpecifier);
            c = int.Parse(idStr[16..24], System.Globalization.NumberStyles.AllowHexSpecifier);
            h = HashCode.Combine(a, b, c);
        }

        public ObjectId(ReadOnlySpan<byte> idBytes)
        {
            if (idBytes.Length != 24) { throw new ArgumentException("Span must be 24 long", nameof(idBytes)); }
            Span<int> idI32 = stackalloc int[3];
            for (int i = 0; i < 24; i += 2)
            {
                var left = idBytes[i];
                int leftNibble = left >= 97 ? 10 + (left - 97) : left - 48;
                var right = idBytes[i + 1];
                int rightNibble = right >= 97 ? 10 + (right - 97) : right - 48;
                idI32[i >> 3] = (idI32[i >> 3] << 8) | (leftNibble << 4) | rightNibble;
            }
            a = idI32[0];
            b = idI32[1];
            c = idI32[2];
            h = HashCode.Combine(a, b, c);
        }

        public override bool Equals(object? obj) => obj is ObjectId id && Equals(id);

        public bool Equals(ObjectId other) => a == other.a && b == other.b && c == other.c;

        public override int GetHashCode() => h;

        public static bool operator ==(ObjectId left, ObjectId right) => left.Equals(right);

        public static bool operator !=(ObjectId left, ObjectId right) => !(left == right);

        public static implicit operator string(in ObjectId objectId) => objectId.ToString();

        public void ToString(Span<char> outString)
        {
            if (outString.Length != 24) { throw new ArgumentException("Span must be 24 long", nameof(outString)); }
            a.TryFormat(outString[0..8], out _, "x8");
            b.TryFormat(outString[8..16], out _, "x8");
            c.TryFormat(outString[16..24], out _, "x8");
        }

        public override string ToString()
        {
            Span<char> str = stackalloc char[24];
            ToString(str);
            return str.ToString();
        }
    }
}
