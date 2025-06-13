using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_compression_program.Algorithms
{
    public class Shannon_Fano_Decoder
    {
   private Dictionary<string, byte> decodingTable;

    public byte[] Decompress(byte[] compressedData, Dictionary<byte, string> encodingTable, int originalLength)
    {
        // عكس جدول الترميز: من سلسلة بتات → رمز
        decodingTable = new Dictionary<string, byte>();
        foreach (var kvp in encodingTable)
        {
            decodingTable[kvp.Value] = kvp.Key;
        }

        string bitString = DecodeToBitString(compressedData);

        var output = new List<byte>();
        StringBuilder current = new StringBuilder();

        foreach (char bit in bitString)
        {
            current.Append(bit);
            if (decodingTable.ContainsKey(current.ToString()))
            {
                output.Add(decodingTable[current.ToString()]);
                current.Clear();

                if (output.Count == originalLength)
                    break; // توقف عند الوصول للحجم الأصلي
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
