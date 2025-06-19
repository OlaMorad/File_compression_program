using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_compression_program.Algorithms
{
    public static class HuffmanTree
    {
      // تسلسل الشجرة بطريقة preorder:
      // اكتب 1 ثم البايت إذا كانت ورقة
      // اكتب 0 إذا كانت فرع
        public static void Serialize(HuffmanNode node, BinaryWriter writer)
        {
            if (node.IsLeaf)
            {
                writer.Write((byte)1);       // علامة ورقة
                writer.Write(node.Symbol.Value);
            }
            else
            {
                writer.Write((byte)0);       // علامة فرع
                Serialize(node.Left, writer);
                Serialize(node.Right, writer);
            }
        }

    public static HuffmanNode Deserialize(BinaryReader reader)
    {
        byte flag = reader.ReadByte();
        if (flag == 1) // ورقة
        {
            byte symbol = reader.ReadByte();
            return new HuffmanNode { Symbol = symbol };
        }
        else // فرع
        {
            var left = Deserialize(reader);
            var right = Deserialize(reader);
            return new HuffmanNode { Left = left, Right = right };
        }
    }
}
}
