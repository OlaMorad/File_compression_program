using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_compression_program.Algorithms
{
    public static class ShannonFanoTable
    {
        // نسلسل جدول الترميز إلى ملف: عدد العناصر (int), ثم لكل عنصر: البايت + طول الكود (byte) + الكود كبتس مضغوطة
        public static void Serialize(Dictionary<byte, string> table, BinaryWriter writer)
        {
            writer.Write(table.Count);

            foreach (var kvp in table)
            {
                writer.Write(kvp.Key);

                string code = kvp.Value;
                writer.Write((byte)code.Length);

                var bits = EncodeCodeToBitArray(code);
                byte[] bytes = BitArrayToBytes(bits);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }

        public static Dictionary<byte, string> Deserialize(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            var table = new Dictionary<byte, string>();

            for (int i = 0; i < count; i++)
            {
                byte symbol = reader.ReadByte();
                byte codeLength = reader.ReadByte();
                int bytesLength = reader.ReadInt32();
                byte[] bytes = reader.ReadBytes(bytesLength);

                BitArray bits = new BitArray(bytes);
                string code = DecodeBitArrayToCode(bits, codeLength);

                table[symbol] = code;
            }

            return table;
        }

        private static BitArray EncodeCodeToBitArray(string code)
        {
            var bits = new BitArray(code.Length);
            for (int i = 0; i < code.Length; i++)
            {
                bits[i] = code[i] == '1';
            }
            return bits;
        }

        private static byte[] BitArrayToBytes(BitArray bits)
        {
            int numBytes = (bits.Length + 7) / 8;
            byte[] bytes = new byte[numBytes];
            bits.CopyTo(bytes, 0);
            return bytes;
        }

        private static string DecodeBitArrayToCode(BitArray bits, int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = bits[i] ? '1' : '0';
            }
            return new string(chars);
        }
    }
}
