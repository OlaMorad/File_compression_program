using System;
using File_compression_program.Algorithms;

namespace File_compression_program.Logic
{
    public enum CompressionAlgorithm
    {
        Huffman,
        ShannonFano
    }

    public class CompressionManager
    {
        // تعديل دالة الضغط لقبول كلمة السر (افتراضي فارغ)
        public void Compress(string inputPath, string outputPath, CompressionAlgorithm algorithm, string password = "")
        {
            var data = FileManager.ReadFile(inputPath);
            switch (algorithm)
            {
                case CompressionAlgorithm.Huffman:
                    var huff = new HuffmanEncoder();
                    huff.CompressToFile(data, outputPath, System.IO.Path.GetExtension(inputPath), password);
                    break;

                case CompressionAlgorithm.ShannonFano:
                    var sf = new Shannon_Fano_Encoder();
                    sf.CompressToFile(data, outputPath, System.IO.Path.GetExtension(inputPath), password);
                    break;

                default:
                    throw new NotSupportedException("Algorithm not supported.");
            }
        }

        // فك الضغط مع تمرير كلمة السر
        public void Decompress(string inputPath, string outputPath, CompressionAlgorithm algorithm, string password = "")
        {
            byte[] result;
            string originalExtension;

            switch (algorithm)
            {
                case CompressionAlgorithm.Huffman:
                    var huff = new HuffmanDecoder();
                    result = huff.DecompressFromFile(inputPath, password, out originalExtension);
                    break;

                case CompressionAlgorithm.ShannonFano:
                    var sf = new Shannon_Fano_Decoder();
                    result = sf.DecompressFromFile(inputPath, password, out originalExtension);
                    break;

                default:
                    throw new NotSupportedException("Algorithm not supported.");
            }

            FileManager.WriteFile(outputPath + originalExtension, result);
        }
    }
}
