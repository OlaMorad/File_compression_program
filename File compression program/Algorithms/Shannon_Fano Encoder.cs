using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace File_compression_program.Algorithms
{
    public class Shannon_Fano_Encoder
    {
        public void CompressToFile(byte[] input, string outputFilePath, string originalExtension, string password = "")
        {
            var freqTable = BuildFrequencyTable(input);
            var encodingTable = ShannonFanoTreeBuilder.BuildEncodingTable(freqTable);

            BitArray bitArray = EncodeToBitArray(input, encodingTable);
            byte[] compressedBytes = BitArrayToBytes(bitArray);

            if (originalExtension.StartsWith("."))
                originalExtension = originalExtension.Substring(1); // نحذف النقطة

            byte[] extBytes = Encoding.UTF8.GetBytes(originalExtension);
            byte extLength = (byte)extBytes.Length;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte passwordLength = (byte)passwordBytes.Length;

            using (var fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                // 1. كتابة طول كلمة السر وكلمة السر نفسها
                writer.Write(passwordLength);
                if (passwordLength > 0)
                    writer.Write(passwordBytes);

                // 2. اكتب طول اللاحقة ثم اللاحقة نفسها
                writer.Write(extLength);
                writer.Write(extBytes);

                // 3. نسلسل جدول الترميز
                ShannonFanoTable.Serialize(encodingTable, writer);

                // 4. طول البيانات الأصلية
                writer.Write(input.Length);

                // 5. البيانات المضغوطة
                writer.Write(compressedBytes);

                // 6. عدد البتات الفارغة
                writer.Write((byte)(bitArray.Length % 8));
            }
        }

        private Dictionary<byte, int> BuildFrequencyTable(byte[] data)
        {
            var freq = new Dictionary<byte, int>();
            foreach (var b in data)
                freq[b] = freq.GetValueOrDefault(b) + 1;
            return freq;
        }

        private BitArray EncodeToBitArray(byte[] input, Dictionary<byte, string> encodingTable)
        {
            int bitLength = input.Sum(b => encodingTable[b].Length);
            var bits = new BitArray(bitLength);
            int bitIndex = 0;

            foreach (byte b in input)
            {
                string code = encodingTable[b];
                foreach (char c in code)
                    bits[bitIndex++] = (c == '1');
            }

            return bits;
        }

        private byte[] BitArrayToBytes(BitArray bits)
        {
            int byteCount = (bits.Length + 7) / 8;
            byte[] bytes = new byte[byteCount];
            bits.CopyTo(bytes, 0);
            return bytes;
        }
    }
}
