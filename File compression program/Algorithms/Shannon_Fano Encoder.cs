using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_compression_program.Algorithms
{
    public class Shannon_Fano_Encoder
    {
        private const int ParallelThreshold = 1024 * 1024; // 1MB

        public void CompressToFile(byte[] input, string outputFilePath, string originalExtension, string password = "")
        {
            // بناء جدول الترددات
            var freqTable = input.Length > ParallelThreshold
                ? BuildFrequencyTableParallel(input)
                : BuildFrequencyTableSequential(input);

            var encodingTable = ShannonFanoTreeBuilder.BuildEncodingTable(freqTable);

            // ضغط البيانات
            byte[] compressedBytes = input.Length > ParallelThreshold
                ? CompressDataParallel(input, encodingTable)
                : CompressDataSequential(input, encodingTable);

            // كتابة الملف الناتج
            WriteOutputFile(outputFilePath, originalExtension, password, encodingTable, input.Length, compressedBytes);
        }

        private Dictionary<byte, int> BuildFrequencyTableSequential(byte[] data)
        {
            var freq = new Dictionary<byte, int>();
            foreach (var b in data)
                freq[b] = freq.GetValueOrDefault(b) + 1;
            return freq;
        }

        private Dictionary<byte, int> BuildFrequencyTableParallel(byte[] data)
        {
            var freq = new ConcurrentDictionary<byte, int>();
            var rangePartitioner = Partitioner.Create(0, data.Length);

            Parallel.ForEach(rangePartitioner, range =>
            {
                var localFreq = new Dictionary<byte, int>();
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    byte b = data[i];
                    localFreq[b] = localFreq.GetValueOrDefault(b) + 1;
                }

                foreach (var pair in localFreq)
                    freq.AddOrUpdate(pair.Key, pair.Value, (k, v) => v + pair.Value);
            });

            return new Dictionary<byte, int>(freq);
        }

        private byte[] CompressDataSequential(byte[] input, Dictionary<byte, string> encodingTable)
        {
            int bitLength = GetTotalBits(input, encodingTable);
            var bits = new BitArray(bitLength);
            int bitIndex = 0;

            foreach (byte b in input)
            {
                string code = encodingTable[b];
                foreach (char c in code)
                    bits[bitIndex++] = (c == '1');
            }

            return BitArrayToBytes(bits);
        }

        private byte[] CompressDataParallel(byte[] input, Dictionary<byte, string> encodingTable)
        {
            int numParts = Math.Min(Environment.ProcessorCount, input.Length / (512 * 1024));
            numParts = Math.Max(1, numParts);
            int partSize = input.Length / numParts;

            var compressedParts = new BitArray[numParts];
            Parallel.For(0, numParts, partNum =>
            {
                int start = partNum * partSize;
                int end = (partNum == numParts - 1) ? input.Length : start + partSize;
                int length = end - start;

                var part = new byte[length];
                Array.Copy(input, start, part, 0, length);

                int bitLength = 0;
                foreach (byte b in part)
                    bitLength += encodingTable[b].Length;

                var bits = new BitArray(bitLength);
                int bitIndex = 0;

                foreach (byte b in part)
                {
                    string code = encodingTable[b];
                    foreach (char c in code)
                        bits[bitIndex++] = (c == '1');
                }

                compressedParts[partNum] = bits;
            });
            // دمج الأجزاء
            int totalBits = compressedParts.Sum(b => b?.Length ?? 0);
            var mergedBits = new BitArray(totalBits);
            int position = 0;

            foreach (var bits in compressedParts)
            {
                if (bits == null) continue;

                for (int i = 0; i < bits.Length; i++)
                    mergedBits[position++] = bits[i];
            }

            return BitArrayToBytes(mergedBits);
        }

        private void WriteOutputFile(string outputFilePath, string originalExtension, string password,
                                    Dictionary<byte, string> encodingTable, int originalLength,
                                    byte[] compressedBytes)
        {
            if (originalExtension.StartsWith("."))
                originalExtension = originalExtension.Substring(1);

            byte[] extBytes = Encoding.UTF8.GetBytes(originalExtension);
            byte extLength = (byte)extBytes.Length;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte passwordLength = (byte)passwordBytes.Length;

            using (var fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(passwordLength);
                if (passwordLength > 0)
                    writer.Write(passwordBytes);

                writer.Write(extLength);
                writer.Write(extBytes);

                ShannonFanoTable.Serialize(encodingTable, writer);
                writer.Write(originalLength);
                writer.Write(compressedBytes);
                writer.Write((byte)(GetTotalBits(originalLength, encodingTable) % 8));
            }
        }

        private byte[] BitArrayToBytes(BitArray bits)
        {
            int byteCount = (bits.Length + 7) / 8;
            byte[] bytes = new byte[byteCount];
            bits.CopyTo(bytes, 0);
            return bytes;
        }

        private int GetTotalBits(int dataLength, Dictionary<byte, string> encodingTable)
        {
            return encodingTable.Values.Sum(code => code.Length) * dataLength / encodingTable.Count;
        }

        private int GetTotalBits(byte[] data, Dictionary<byte, string> encodingTable)
        {
            return data.Sum(b => encodingTable[b].Length);
        }
    }
}