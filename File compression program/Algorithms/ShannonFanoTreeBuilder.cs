using System.Collections.Generic;
using System.Linq;

namespace File_compression_program.Algorithms
{
    public static class ShannonFanoTreeBuilder
    {
        // يبني جدول الترميز من جدول الترددات
        public static Dictionary<byte, string> BuildEncodingTable(Dictionary<byte, int> freqTable)
        {
            var symbols = freqTable.OrderByDescending(kvp => kvp.Value).ToList();
            var table = new Dictionary<byte, string>();
            BuildEncodingRecursive(symbols, "", table);
            return table;
        }

        private static void BuildEncodingRecursive(List<KeyValuePair<byte, int>> symbols, string prefix, Dictionary<byte, string> table)
        {
            if (symbols.Count == 1)
            {
                table[symbols[0].Key] = prefix.Length == 0 ? "0" : prefix;
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

            BuildEncodingRecursive(left, prefix + "0", table);
            BuildEncodingRecursive(right, prefix + "1", table);
        }
    }
}
