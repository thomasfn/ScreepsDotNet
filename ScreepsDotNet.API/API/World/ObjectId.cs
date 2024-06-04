using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ScreepsDotNet.API.World
{
    /// <summary>
    /// Represents the id of a room object.
    /// Faster than the string counterpart for equality, hashing etc.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    public readonly struct ObjectId : IEquatable<ObjectId>
    {
        private readonly int a, b, c, h;

        public bool IsValid => a != 0 || b != 0 || c != 0;

        internal ObjectId(int a, int b, int c, int h)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.h = h;
        }

        public ObjectId(ReadOnlySpan<char> idStr)
        {
            if (idStr.Length < 24)
            {
                Span<char> tmp = stackalloc char[24];
                idStr.CopyTo(tmp);
                tmp[idStr.Length..].Fill('0');
                a = int.Parse(tmp[0..8], System.Globalization.NumberStyles.AllowHexSpecifier);
                b = int.Parse(tmp[8..16], System.Globalization.NumberStyles.AllowHexSpecifier);
                c = int.Parse(tmp[16..24], System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            else
            {
                a = int.Parse(idStr[0..8], System.Globalization.NumberStyles.AllowHexSpecifier);
                b = int.Parse(idStr[8..16], System.Globalization.NumberStyles.AllowHexSpecifier);
                c = int.Parse(idStr[16..24], System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            h = ((17 * 31 + a) * 31 + b) * 31 + c;
            h = (h & ~31) | idStr.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DecodeHexDigit(int digit)
            => digit >= 97 ? 10 + (digit - 97) : digit - 48;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Decode4HexDigits(int digits)
            => (DecodeHexDigit(digits & 255) << 12) | (DecodeHexDigit((digits >> 8) & 255) << 8) | (DecodeHexDigit((digits >> 16) & 255) << 4) | DecodeHexDigit(digits >> 24);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int EncodeHexDigit(int nibble)
            => nibble >= 10 ? 97 + (nibble - 10) : 48 + nibble;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Encode4HexDigits(int value)
            => (EncodeHexDigit(value & 0xf) << 24) | (EncodeHexDigit((value >> 4) & 0xf) << 16) | (EncodeHexDigit((value >> 8) & 0xf) << 8) | EncodeHexDigit((value >> 12) & 0xf);

        public ObjectId(ReadOnlySpan<byte> idBytes)
        {
            int len;
            Span<byte> tmp = stackalloc byte[24];
            idBytes.CopyTo(tmp);
            for (len = 0; len < 24; ++len)
            {
                if (tmp[len] == 0) { break; }
            }
            for (int i = len; i < 24; ++i)
            {
                tmp[len] = (byte)'0';
            }
            var idInts = MemoryMarshal.Cast<byte, int>(tmp);
            a = (Decode4HexDigits(idInts[0]) << 16) | Decode4HexDigits(idInts[1]);
            b = (Decode4HexDigits(idInts[2]) << 16) | Decode4HexDigits(idInts[3]);
            c = (Decode4HexDigits(idInts[4]) << 16) | Decode4HexDigits(idInts[5]);
            h = ((17 * 31 + a) * 31 + b) * 31 + c;
            h = (h & ~31) | len;
        }

        public void ToBytes(Span<byte> idBytes)
        {
            Span<int> encodedId = MemoryMarshal.Cast<byte, int>(idBytes);
            encodedId[0] = Encode4HexDigits(a >> 16);
            encodedId[1] = Encode4HexDigits(a & 0xffff);
            encodedId[2] = Encode4HexDigits(b >> 16);
            encodedId[3] = Encode4HexDigits(b & 0xffff);
            encodedId[4] = Encode4HexDigits(c >> 16);
            encodedId[5] = Encode4HexDigits(c & 0xffff);
            int len = h & 31;
            if (len < 24) { idBytes[len] = 0; }
        }

        public override bool Equals(object? obj) => obj is ObjectId id && Equals(id);

        public bool Equals(ObjectId other) => h == other.h && a == other.a && b == other.b && c == other.c;

        public override int GetHashCode() => h;

        public static bool operator ==(ObjectId left, ObjectId right) => left.Equals(right);

        public static bool operator !=(ObjectId left, ObjectId right) => !(left == right);

        public static implicit operator string(in ObjectId objectId) => objectId.ToString();

        public int ToString(Span<char> outString)
        {
            Span<char> tmp = stackalloc char[24];
            a.TryFormat(tmp[0..8], out _, "x8");
            b.TryFormat(tmp[8..16], out _, "x8");
            c.TryFormat(tmp[16..24], out _, "x8");
            int len = h & 31;
            tmp[..len].CopyTo(outString);
            return len;
        }

        public override string ToString()
        {
            Span<char> str = stackalloc char[24];
            return str[0..ToString(str)].ToString();
        }
    }
}
