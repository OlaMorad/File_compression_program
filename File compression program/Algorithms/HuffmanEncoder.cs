using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace File_compression_program.Algorithms
{
    public class HuffmanEncoder
    {
        public void CompressToFile(byte[] input, string outputFilePath, string originalExtension, string password = "")
        {
            var freqTable = BuildFrequencyTable(input);
            var root = HuffmanTreeBuilder.BuildTree(freqTable);
            var encodingTable = HuffmanTreeBuilder.BuildEncodingTable(root);

            BitArray bitArray = EncodeToBitArray(input, encodingTable);
            byte[] compressedBytes = BitArrayToBytes(bitArray);

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte passwordLength = (byte)passwordBytes.Length;

            using (var fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs, Encoding.UTF8))
            {
                // 1. كتابة طول كلمة السر وكلمة السر نفسها
                writer.Write(passwordLength);
                if (passwordLength > 0)
                    writer.Write(passwordBytes);

                // 2. كتابة طول اللاحقة
                writer.Write((byte)originalExtension.Length);

                // 3. كتابة اللاحقة نفسها (مثل "pdf")
                writer.Write(Encoding.UTF8.GetBytes(originalExtension));

                // 4. كتابة شجرة هوفمان
                HuffmanTree.Serialize(root, writer);

                // 5. كتابة طول البيانات الأصلية
                writer.Write(input.Length);

                // 6. كتابة البيانات المضغوطة
                writer.Write(compressedBytes);

                // 7. كتابة عدد البتات غير المستخدمة في آخر بايت (padding)
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
                {
                    bits[bitIndex++] = (c == '1');
                }
            }

            return bits;
        }

        private byte[] BitArrayToBytes(BitArray bits)
        {
            int numBytes = (bits.Length + 7) / 8;
            byte[] bytes = new byte[numBytes];
            bits.CopyTo(bytes, 0);
            return bytes;
        }
    }
}
