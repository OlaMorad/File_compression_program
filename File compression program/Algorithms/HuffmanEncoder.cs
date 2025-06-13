using System;
using System.Collections.Generic;
using System.Linq;

namespace File_compression_program.Algorithms
{
    public class HuffmanEncoder
    {
        private Dictionary<byte, string> _encodingTable;

        public (byte[] CompressedData, HuffmanNode Tree) Compress(byte[] input)
        {
            var frequencyTable = BuildFrequencyTable(input);
            var root = BuildHuffmanTree(frequencyTable);
            _encodingTable = BuildEncodingTable(root);

            string encodedBits = string.Concat(input.Select(b => _encodingTable[b]));
            byte[] encodedBytes = EncodeBitString(encodedBits);

            return (encodedBytes, root);
        }

        private Dictionary<byte, int> BuildFrequencyTable(byte[] data)
        {
            var freq = new Dictionary<byte, int>();
            foreach (var b in data)
            {
                if (!freq.ContainsKey(b)) freq[b] = 0;
                freq[b]++;
            }
            return freq;
        }

        private HuffmanNode BuildHuffmanTree(Dictionary<byte, int> frequencies)
        {
            var pq = new PriorityQueue<HuffmanNode, int>();
            foreach (var kvp in frequencies)
            {
                pq.Enqueue(new HuffmanNode { Symbol = kvp.Key, Frequency = kvp.Value }, kvp.Value);
            }

            while (pq.Count > 1)
            {
                var left = pq.Dequeue();
                var right = pq.Dequeue();
                var parent = new HuffmanNode
                {
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };
                pq.Enqueue(parent, parent.Frequency);
            }

            return pq.Dequeue();
        }

        private Dictionary<byte, string> BuildEncodingTable(HuffmanNode node)
        {
            var table = new Dictionary<byte, string>();
            BuildTableRecursive(node, "", table);
            return table;
        }

        private void BuildTableRecursive(HuffmanNode node, string path, Dictionary<byte, string> table)
        {
            if (node == null) return;

            if (node.IsLeaf && node.Symbol.HasValue)
                table[node.Symbol.Value] = path;

            BuildTableRecursive(node.Left, path + "0", table);
            BuildTableRecursive(node.Right, path + "1", table);
        }

        private byte[] EncodeBitString(string bitString)
        {
            int byteCount = (bitString.Length + 7) / 8;
            byte[] result = new byte[byteCount];

            for (int i = 0; i < bitString.Length; i++)
            {
                if (bitString[i] == '1')
                    result[i / 8] |= (byte)(1 << (7 - (i % 8)));
            }

            return result;
        }
    }
}
