using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_compression_program.Logic
{
    public class CompressionStats
    {
        public long OriginalSize { get; set; }
        public long CompressedSize { get; set; }


        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public double GetCompressionRatio()
        {
            if (OriginalSize == 0) return 0;
            return ((double)CompressedSize / OriginalSize) * 100;
        }

        public string GetSummary()
        {
            return $"الحجم الأصلي: {FormatBytes(OriginalSize)}\n" +
            $"الحجم بعد الضغط: {FormatBytes(CompressedSize)}\n" +
            $"نسبة الضغط: {GetCompressionRatio():F2}%";
        }
    }
}
