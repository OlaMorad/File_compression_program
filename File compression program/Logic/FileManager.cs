using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_compression_program.Logic
{
    public static class FileManager
    {
        public static byte[] ReadFile(string path) => File.ReadAllBytes(path);

        public static void WriteFile(string path, byte[] data) => File.WriteAllBytes(path, data);

        public static string GetFileNameWithoutExtension(string path) =>
            Path.GetFileNameWithoutExtension(path);

        public static string GetExtension(string path) =>
            Path.GetExtension(path);


        public static string ReplaceExtension(string path, string newExtension)
        {
            return Path.ChangeExtension(path, newExtension);
        }

    }
}
