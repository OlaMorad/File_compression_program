using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace File_compression_program.Logic
{
    public class CompressionTaskManager
    {
        private CancellationTokenSource _cts;
        private ManualResetEventSlim _pauseEvent;
        private bool _isPaused;
        private int _progress;
        public event Action<int> ProgressChanged;
        public event Action<string> Completed;
        public event Action<string> Failed;

        public bool IsPaused => _isPaused;

        public void StartCompress(string folderPath, string zipOutputPath)
        {
            _cts = new CancellationTokenSource();
            _pauseEvent = new ManualResetEventSlim(true);
            _isPaused = false;

            Task.Run(() =>
            {
                try
                {
                    string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                    using FileStream zipToOpen = new FileStream(zipOutputPath, FileMode.Create);
                    using ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create);

                    for (int i = 0; i < files.Length; i++)
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        _pauseEvent.Wait();

                        string entryName = Path.GetRelativePath(folderPath, files[i]);
                        archive.CreateEntryFromFile(files[i], entryName, CompressionLevel.Optimal);

                        _progress = (int)((i + 1) * 100 / files.Length);
                        ProgressChanged?.Invoke(_progress);
                    }

                    Completed?.Invoke($"✅ Compression finished: {zipOutputPath}");
                }
                catch (OperationCanceledException)
                {
                    Failed?.Invoke("❌ Compression canceled.");
                }
                catch (Exception ex)
                {
                    Failed?.Invoke("❌ Compression failed:\n" + ex.Message);
                }
            });
        }
        public void StartCompressMultipleFiles(string[] files, CompressionAlgorithm algorithm, string password)
        {
            _cts = new CancellationTokenSource();
            _pauseEvent = new ManualResetEventSlim(true);
            _isPaused = false;

            Task.Run(() =>
            {
                try
                {
                    int totalFiles = files.Length;
                    StringBuilder results = new StringBuilder();

                    for (int i = 0; i < totalFiles; i++)
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        _pauseEvent.Wait();

                        string inputFile = files[i];
                        string outputFile = inputFile + (algorithm == CompressionAlgorithm.Huffman ? ".huff" : ".sf");

                        // افترض وجود CompressionManager للضغط الفعلي
                        var manager = new CompressionManager();
                        var start = DateTime.Now;
                        manager.Compress(inputFile, outputFile, algorithm, password);
                        var duration = DateTime.Now - start;

                        var stats = new CompressionStats(inputFile, outputFile);
                        string fileResult = $"📄 {Path.GetFileName(inputFile)}\n" +
                                            $"{stats.GetCompressionSummaryWithTime(duration)}\n" +
                                            $"{new string('-', 40)}\n";

                        results.Append(fileResult);

                        _progress = (int)((i + 1) * 100 / totalFiles);
                        ProgressChanged?.Invoke(_progress);
                    }

                    Completed?.Invoke("✅ Compression completed for all files:\n\n" + results.ToString());
                }
                catch (OperationCanceledException)
                {
                    Failed?.Invoke("❌ Compression canceled.");
                }
                catch (Exception ex)
                {
                    Failed?.Invoke("❌ Compression failed:\n" + ex.Message);
                }
            });
        }


        public void Pause()
        {
            _pauseEvent?.Reset();
            _isPaused = true;
        }

        public void Resume()
        {
            _pauseEvent?.Set();
            _isPaused = false;
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }
    }
}
