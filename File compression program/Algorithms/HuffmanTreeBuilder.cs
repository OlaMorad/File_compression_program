using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_compression_program.Algorithms
{
    public static class HuffmanTreeBuilder
    {
        // يبني شجرة هوفمان من جدول الترددات
        public static HuffmanNode BuildTree(Dictionary<byte, int> frequencies)
        {
            var pq = new PriorityQueue<HuffmanNode, int>();

            // نضيف كل رمز مع تردده كعقدة ورقية في الطابور ذو الأولوية
            foreach (var kvp in frequencies)
            {
                pq.Enqueue(new HuffmanNode { Symbol = kvp.Key, Frequency = kvp.Value }, kvp.Value);
            }

            // ندمج أقل عقدتين حتى يتبقى جذر واحد للشجرة
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

        // يبني جدول الترميز: كل بايت يقابله سلسلة 0 و 1
        public static Dictionary<byte, string> BuildEncodingTable(HuffmanNode root)
        {
            var table = new Dictionary<byte, string>();
            BuildTableRecursive(root, "", table);
            return table;
        }

        private static void BuildTableRecursive(HuffmanNode node, string path, Dictionary<byte, string> table)
        {
            if (node == null)
                return;

            if (node.IsLeaf && node.Symbol.HasValue)
            {
                table[node.Symbol.Value] = path;
            }
            else
            {
                BuildTableRecursive(node.Left, path + "0", table);
                BuildTableRecursive(node.Right, path + "1", table);
            }
        }
    }
}

