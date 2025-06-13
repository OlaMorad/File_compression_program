

namespace File_compression_program.Algorithms
{
    public class HuffmanNode
    {
        public byte? Symbol { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }

        public bool IsLeaf => Left == null && Right == null;
    }
}
