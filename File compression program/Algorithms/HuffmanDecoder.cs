using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace File_compression_program.Algorithms
{
    public class HuffmanDecoder
    {
        public byte[] DecompressFromFile(string inputFilePath, string password, out string originalExtension)
        {
            using (var fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                // 1. قراءة طول كلمة السر وكلمة السر نفسها
                byte passwordLength = reader.ReadByte();
                string filePassword = "";
                if (passwordLength > 0)
                {
                    byte[] pwdBytes = reader.ReadBytes(passwordLength);
                    filePassword = System.Text.Encoding.UTF8.GetString(pwdBytes);
                }

                if (filePassword != password)
                    throw new UnauthorizedAccessException("Incorrect password.");

                // 2. قراءة طول اللاحقة
                byte extLength = reader.ReadByte();

                // 3. قراءة اللاحقة نفسها
                byte[] extBytes = reader.ReadBytes(extLength);
                originalExtension = System.Text.Encoding.UTF8.GetString(extBytes);

                // 4. فك باقي البيانات كالمعتاد
                var root = HuffmanTree.Deserialize(reader);
                int originalLength = reader.ReadInt32();

                long bytesLength = fs.Length - fs.Position - 1; // نطرح البادينغ
                byte[] compressedData = reader.ReadBytes((int)bytesLength);

                byte paddingBits = reader.ReadByte();
                BitArray bits = new BitArray(compressedData);
                int bitCount = bits.Length - paddingBits;

                return Decompress(bits, bitCount, root, originalLength);
            }
        }

        private byte[] Decompress(BitArray bits, int bitCount, HuffmanNode root, int originalLength)
        {
            var output = new List<byte>();
            HuffmanNode current = root;

            for (int i = 0; i < bitCount; i++)
            {
                current = bits[i] ? current.Right : current.Left;

                if (current.IsLeaf)
                {
                    output.Add(current.Symbol.Value);
                    current = root;
                    if (output.Count == originalLength)
                        break;
                }
            }

            return output.ToArray();
        }
    }
}
