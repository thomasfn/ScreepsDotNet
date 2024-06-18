using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScreepsDotNet.Bundler.Wasm
{
    internal static class IOExtensions
    {
        public static uint ReadVarU32(this BinaryReader reader)
        {
            uint result = 0;
            int shift = 0;
            byte b;
            do
            {
                b = reader.ReadByte();
                result |= (b & 0x7fu) << shift;
                shift += 7;
            }
            while ((b & 0x80u) != 0);
            return result;
        }

        public static string ReadVarString(this BinaryReader reader)
        {
            uint len = reader.ReadVarU32();
            byte[] rawData = reader.ReadBytes((int)len);
            return Encoding.UTF8.GetString(rawData);
        }

        public static void ReadVector<T>(this BinaryReader reader, IList<T> outList, Func<BinaryReader, T> elementReadFunc)
        {
            uint len = reader.ReadVarU32();
            for (uint i = 0; i < len; ++i)
            {
                outList.Add(elementReadFunc(reader));
            }
        }

        public static void WriteVarU32(this BinaryWriter writer, uint value)
        {
            do
            {
                byte b = (byte)(value & 0x7fu);
                value >>= 7;
                if (value != 0) { b |= 0x80; }
                writer.Write(b);
            } while (value != 0);
        }

        public static void WriteVarString(this BinaryWriter writer, string str)
        {
            var rawData = Encoding.UTF8.GetBytes(str);
            writer.WriteVarU32((uint)rawData.Length);
            writer.Write(rawData);
        }

        public static void WriteVector<T>(this BinaryWriter writer, IReadOnlyList<T> inList, Action<BinaryWriter, T> elementWriteFunc)
        {
            writer.WriteVarU32((uint)inList.Count);
            foreach (var item in inList)
            {
                elementWriteFunc(writer, item);
            }
        }
    }
}
