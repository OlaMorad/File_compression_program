using System.IO;
using System.IO.Compression;
using System.Linq;

namespace File_compression_program.Logic
{
    public static class FolderManager
    {
        public static void CompressFolder(string folderPath, string outputZipFilePath)
        {
            if (File.Exists(outputZipFilePath))
            {
                File.Delete(outputZipFilePath);
            }

            ZipFile.CreateFromDirectory(folderPath, outputZipFilePath, CompressionLevel.Fastest, includeBaseDirectory: false);
            //ZipFile.CreateFromDirectory(folderPath, outputZipFilePath, CompressionLevel.Optimal, includeBaseDirectory: false);
        }

        public static void DecompressToFolder(string zipFilePath, string outputFolderPath)
        {
            if (Directory.Exists(outputFolderPath))
            {
                Directory.Delete(outputFolderPath, true);
            }

            ZipFile.ExtractToDirectory(zipFilePath, outputFolderPath);
        }

        public static void ExtractSingleFileFromZip(string zipFilePath, string fileNameInArchive, string destinationPath)
        {
            using ZipArchive archive = ZipFile.OpenRead(zipFilePath);
            var entry = archive.GetEntry(fileNameInArchive);

            if (entry == null)
                throw new FileNotFoundException("File not found inside the archive: " + fileNameInArchive);

            string fullOutputPath = Path.Combine(destinationPath, Path.GetFileName(fileNameInArchive));
            entry.ExtractToFile(fullOutputPath, overwrite: true);
        }

        public static long GetFolderSize(string path)
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                            .Sum(file => new FileInfo(file).Length);
        }

        public static string[] GetZipEntryNames(string zipFilePath)
        {
            using ZipArchive archive = ZipFile.OpenRead(zipFilePath);
            return archive.Entries.Select(e => e.FullName).ToArray();
        }
    }
}
