using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using File_compression_program.Logic;

namespace File_compression_program.Ui
{
    public partial class ComparisonUserControl : UserControl
    {
        private Button selectFileButton;
        private Label filePathLabel;
        private Button compareButton;
        private RichTextBox resultsTextBox;
        private string selectedFilePath;

        public ComparisonUserControl()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Padding = new Padding(20);

            // زر اختيار الملف
            selectFileButton = new Button
            {
                Text = "Select File",
                Width = 150,
                Height = 40,
                Top = 20,
                Left = 20,
                Font = new Font("Segoe UI", 10)
            };
            selectFileButton.Click += SelectFileButton_Click;

            // تسمية لعرض مسار الملف
            filePathLabel = new Label
            {
                AutoSize = true,
                Top = 30,
                Left = 190,
                Font = new Font("Segoe UI", 10),
                Text = "No file selected"
            };

            // زر المقارنة
            compareButton = new Button
            {
                Text = "Compare Algorithms",
                Width = 200,
                Height = 50,
                Top = 80,
                Left = 20,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            compareButton.Click += CompareButton_Click;

            // مربع النتائج
            resultsTextBox = new RichTextBox
            {
                Top = 150,
                Left = 20,
                Width = this.Width - 40,
                Height = this.Height - 180,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            this.Controls.Add(selectFileButton);
            this.Controls.Add(filePathLabel);
            this.Controls.Add(compareButton);
            this.Controls.Add(resultsTextBox);
        }

        private void SelectFileButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    filePathLabel.Text = Path.GetFileName(selectedFilePath);
                    compareButton.Enabled = true;
                }
            }
        }

        private void CompareButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath)) return;

            resultsTextBox.Clear();
            resultsTextBox.AppendText("=== Comparing Compression Algorithms ===\n");
            resultsTextBox.AppendText($"File: {Path.GetFileName(selectedFilePath)}\n\n");

            // إنشاء مجلد مؤقت للمقارنة
            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            try
            {
                // ضغط باستخدام Huffman
                var huffmanResults = CompressAndMeasure(selectedFilePath, tempFolder, "huffman", CompressionAlgorithm.Huffman);
                resultsTextBox.AppendText("=== Huffman Results ===\n");
                resultsTextBox.AppendText(huffmanResults + "\n\n");

                // ضغط باستخدام Shannon-Fano
                var shannonFanoResults = CompressAndMeasure(selectedFilePath, tempFolder, "shannonfano", CompressionAlgorithm.ShannonFano);
                resultsTextBox.AppendText("=== Shannon-Fano Results ===\n");
                resultsTextBox.AppendText(shannonFanoResults + "\n\n");
                // إضافة ملخص المقارنة
                resultsTextBox.AppendText("=== Comparison Summary ===\n");
                resultsTextBox.AppendText(GetComparisonSummary(huffmanResults, shannonFanoResults));
            }
            catch (Exception ex)
            {
                resultsTextBox.AppendText($"Error: {ex.Message}");
            }
            finally
            {
                // تنظيف الملفات المؤقتة
                try { Directory.Delete(tempFolder, true); }
                catch { }
            }
        }

        private string CompressAndMeasure(string inputPath, string tempFolder, string prefix, CompressionAlgorithm algorithm)
        {
            string outputPath = Path.Combine(tempFolder, $"{prefix}_{Path.GetFileName(inputPath)}.cmp");

            var compressionManager = new CompressionManager();
            var stopwatch = Stopwatch.StartNew();

            compressionManager.Compress(inputPath, outputPath, algorithm);

            stopwatch.Stop();
            var stats = new CompressionStats(inputPath, outputPath);

            return stats.GetCompressionSummaryWithTime(stopwatch.Elapsed);
        }

        private string GetComparisonSummary(string huffmanResults, string shannonFanoResults)
        {
            // استخراج البيانات من النتائج
            var huffmanData = ExtractData(huffmanResults);
            var shannonData = ExtractData(shannonFanoResults);

            return $"Comparison Summary:\n\n" +
                   $"1. Compression Ratio:\n" +
                   $"   - Huffman: {huffmanData.ratio}%\n" +
                   $"   - Shannon-Fano: {shannonData.ratio}%\n" +
                   $"   - Difference: {Math.Abs(huffmanData.ratio - shannonData.ratio):F2}%\n\n" +
                   $"2. Execution Time:\n" +
                   $"   - Huffman: {huffmanData.time} ms\n" +
                   $"   - Shannon-Fano: {shannonData.time} ms\n" +
                   $"   - Difference: {Math.Abs(huffmanData.time - shannonData.time)} ms\n\n" +
                   $"Conclusion:\n" +
                   $"{(huffmanData.ratio < shannonData.ratio ? "Huffman" : "Shannon-Fano")} achieved better compression (smaller size)\n" +
                   $"{(huffmanData.time < shannonData.time ? "Huffman" : "Shannon-Fano")} was faster";
        }

        private (double ratio, double time) ExtractData(string results)
        {
            // استخراج نسبة الضغط والوقت من النص
            var ratioPart = results.Split("نسبة الضغط:")[1].Split('%')[0].Trim();
            var timePart = results.Split("الزمن:")[1].Split("ميلي ثانية")[0].Trim();

            return (double.Parse(ratioPart), double.Parse(timePart));
        }
    }
}