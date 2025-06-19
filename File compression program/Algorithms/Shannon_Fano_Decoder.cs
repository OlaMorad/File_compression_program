using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace File_compression_program.Algorithms
{
    public class Shannon_Fano_Decoder
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
                    filePassword = Encoding.UTF8.GetString(pwdBytes);
                }

                if (filePassword != password)
                    throw new UnauthorizedAccessException("Incorrect password.");

                // 2. قراءة لاحقة الملف الأصلية
                byte extLength = reader.ReadByte();
                byte[] extBytes = reader.ReadBytes(extLength);
                originalExtension = Encoding.UTF8.GetString(extBytes);

                // 3. جدول الترميز
                var encodingTable = ShannonFanoTable.Deserialize(reader);

                // 4. طول البيانات الأصلية
                int originalLength = reader.ReadInt32();

                // 5. البيانات المضغوطة
                long bytesLength = fs.Length - fs.Position - 1;
                byte[] compressedData = reader.ReadBytes((int)bytesLength);

                // 6. عدد البتات الفارغة
                byte paddingBits = reader.ReadByte();

                BitArray bits = new BitArray(compressedData);
                int bitCount = bits.Length - paddingBits;

                return Decompress(bits, bitCount, encodingTable, originalLength);
            }
        }

        private byte[] Decompress(BitArray bits, int bitCount, Dictionary<byte, string> encodingTable, int originalLength)
        {
            var decodingTable = new Dictionary<string, byte>();
            foreach (var kvp in encodingTable)
                decodingTable[kvp.Value] = kvp.Key;

            var output = new List<byte>();
            StringBuilder currentCode = new StringBuilder();

            for (int i = 0; i < bitCount; i++)
            {
                currentCode.Append(bits[i] ? '1' : '0');

                if (decodingTable.TryGetValue(currentCode.ToString(), out byte symbol))
                {
                    output.Add(symbol);
                    currentCode.Clear();

                    if (output.Count == originalLength)
                        break;
                }
            }

            return output.ToArray();
        }
    }
}
