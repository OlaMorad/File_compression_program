// algorithms/HuffmanDecoder.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace File_compression_program.Algorithms
{
    public class HuffmanDecoder
    {
        public byte[] Decompress(byte[] compressedData, HuffmanNode root, int originalLength)
        {
            List<byte> output = new List<byte>();
            HuffmanNode current = root;

            string bitString = DecodeToBitString(compressedData);

            foreach (char bit in bitString)
            {
                current = (bit == '0') ? current.Left : current.Right;

                if (current.IsLeaf)
                {
                    if (current.Symbol.HasValue)
                        output.Add(current.Symbol.Value);
                    current = root;

                    if (output.Count == originalLength)
                        break;
                }
            }

            return output.ToArray();
        }

        private string DecodeToBitString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }
    }
}
