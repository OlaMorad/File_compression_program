using System;

namespace File_compression_program.Logic
{
    public class CompressionStats
    {
        public long OriginalSizeBytes { get; private set; }
        public long CompressedSizeBytes { get; private set; }

        public CompressionStats(string originalFilePath, string compressedFilePath)
        {
            OriginalSizeBytes = new FileInfo(originalFilePath).Length;
            CompressedSizeBytes = new FileInfo(compressedFilePath).Length;
        }

        public CompressionStats(long originalBytes, long compressedBytes)
        {
            OriginalSizeBytes = originalBytes;
            CompressedSizeBytes = compressedBytes;
        }

        public string GetOriginalSizeFormatted() => FormatSize(OriginalSizeBytes);
        public string GetCompressedSizeFormatted() => FormatSize(CompressedSizeBytes);

        public double GetCompressionRatio()
        {
            if (OriginalSizeBytes == 0) return 0;
            return (((double)CompressedSizeBytes / OriginalSizeBytes)) * 100;
        }

        public string GetCompressionSummaryWithTime(TimeSpan duration)
        {
            return $"الحجم الأصلي: {GetOriginalSizeFormatted()} | " +
                   $"الحجم بعد الضغط: {GetCompressedSizeFormatted()} | " +
                   $"نسبة الضغط: {GetCompressionRatio():F2}% | " +
                   $"الزمن: {duration.TotalMilliseconds:F0} ميلي ثانية";
        }

        private string FormatSize(long bytes)
        {
            const double KB = 1024;
            const double MB = KB * 1024;
            const double GB = MB * 1024;

            if (bytes < KB)
                return $"{bytes} B";
            else if (bytes < MB)
                return $"{(bytes / KB):F2} KB";
            else if (bytes < GB)
                return $"{(bytes / MB):F2} MB";
            else
                return $"{(bytes / GB):F2} GB";
        }
    }
}
