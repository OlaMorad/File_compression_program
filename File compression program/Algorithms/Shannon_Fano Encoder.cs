using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_compression_program.Algorithms
{
    public class Shannon_Fano_Encoder
    {
        private Dictionary<byte, string> encodingTable;

        public byte[] Compress(byte[] input, out Dictionary<byte, string> encodingTableOut)
        {
            var freqTable = BuildFrequencyTable(input);
            encodingTable = new Dictionary<byte, string>();

            var symbols = freqTable.OrderByDescending(x => x.Value).ToList();
            BuildEncoding(symbols, "");

            encodingTableOut = encodingTable;

            var encodedBitString = string.Concat(input.Select(b => encodingTable[b]));
            return EncodeBitString(encodedBitString);
        }

        private Dictionary<byte, int> BuildFrequencyTable(byte[] data)
        {
            var freq = new Dictionary<byte, int>();
            foreach (byte b in data)
            {
                if (!freq.ContainsKey(b)) freq[b] = 0;
                freq[b]++;
            }
            return freq;
        }

        private void BuildEncoding(List<KeyValuePair<byte, int>> symbols, string prefix)
        {
            if (symbols.Count == 1)
            {
                encodingTable[symbols[0].Key] = prefix.Length == 0 ? "0" : prefix;
                return;
            }

            int total = symbols.Sum(s => s.Value);
            int half = total / 2;
            int sum = 0;
            int splitIndex = 0;

            for (int i = 0; i < symbols.Count; i++)
            {
                sum += symbols[i].Value;
                if (sum >= half)
                {
                    splitIndex = i + 1;
                    break;
                }
            }

            var left = symbols.Take(splitIndex).ToList();
            var right = symbols.Skip(splitIndex).ToList();

            BuildEncoding(left, prefix + "0");
            BuildEncoding(right, prefix + "1");
        }

        private byte[] EncodeBitString(string bits)
        {
            int byteCount = (bits.Length + 7) / 8;
            byte[] result = new byte[byteCount];

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == '1')
                    result[i / 8] |= (byte)(1 << (7 - (i % 8)));
            }

            return result;
        }
    }
}
