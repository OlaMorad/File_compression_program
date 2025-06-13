using File_compression_program.Algorithms;
using File_compression_program.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace File_compression_program
{
    public partial class MainForm : Form
    {
        private Button selectFileButton;
        private Button compressButton;
        private Button decompressButton;
        private ComboBox algorithmComboBox;
        private Label statusLabel;
        private Label timeLabel;
        private ProgressBar progressBar;

        private string[] selectedFiles;

        public MainForm()
        {
            this.Text = "»—‰«„Ã ÷€ÿ «·„·›« ";
            this.Size = new Size(700, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeControls();
        }

        private void InitializeControls()
        {
            selectFileButton = new Button
            {
                Text = " Õ„Ì· „·›« ",
                Location = new Point(30, 30),
                Size = new Size(120, 30)
            };
            selectFileButton.Click += SelectFileButton_Click;
            this.Controls.Add(selectFileButton);

            algorithmComboBox = new ComboBox
            {
                Location = new Point(170, 30),
                Size = new Size(150, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            algorithmComboBox.Items.AddRange(new string[] { "Huffman", "Shannon-Fano" });
            algorithmComboBox.SelectedIndex = 0;
            this.Controls.Add(algorithmComboBox);

            compressButton = new Button
            {
                Text = "÷€ÿ «·„·›« ",
                Location = new Point(340, 30),
                Size = new Size(120, 30),
                Enabled = false
            };
            compressButton.Click += CompressButton_Click;
            this.Controls.Add(compressButton);

            decompressButton = new Button
            {
                Text = "›ﬂ «·÷€ÿ",
                Location = new Point(470, 30),
                Size = new Size(120, 30)
            };
            decompressButton.Click += DecompressButton_Click;
            this.Controls.Add(decompressButton);

            progressBar = new ProgressBar
            {
                Location = new Point(30, 80),
                Size = new Size(600, 20),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            this.Controls.Add(progressBar);

            timeLabel = new Label
            {
                Location = new Point(30, 110),
                Size = new Size(600, 20),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(timeLabel);

            statusLabel = new Label
            {
                Text = "„⁄·Ê„«  «·÷€ÿ ” ŸÂ— Â‰«",
                Location = new Point(30, 140),
                Size = new Size(600, 200),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(statusLabel);
        }

        private void SelectFileButton_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "«Œ — „·› √Ê √ﬂÀ—"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                selectedFiles = dialog.FileNames;
                compressButton.Enabled = selectedFiles.Length > 0;
                statusLabel.Text = $" „ «Œ Ì«— {selectedFiles.Length} „·›(« )";
            }
        }

        private void CompressButton_Click(object sender, EventArgs e)
        {
            if (selectedFiles == null || selectedFiles.Length == 0)
            {
                MessageBox.Show("Ì—ÃÏ «Œ Ì«— „·›«  √Ê·«");
                return;
            }

            string algorithm = algorithmComboBox.SelectedItem.ToString();
            var stopwatch = Stopwatch.StartNew();
            progressBar.Visible = true;

            foreach (string filePath in selectedFiles)
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                string originalExtension = Path.GetExtension(filePath);
                string baseOutputPath = filePath;

                byte[] compressedData = null;

                if (algorithm == "Huffman")
                {
                    var encoder = new HuffmanEncoder();
                    var result = encoder.Compress(fileData);
                    compressedData = result.CompressedData;

                    File.WriteAllBytes(baseOutputPath + ".huff", compressedData);

                    using var writer = new BinaryWriter(File.Open(baseOutputPath + ".huff.meta", FileMode.Create));
                    writer.Write(fileData.Length);
                    writer.Write(originalExtension);
                    SaveHuffmanTree(writer, result.Tree);
                }
                else if (algorithm == "Shannon-Fano")
                {
                    var encoder = new Shannon_Fano_Encoder();
                    compressedData = encoder.Compress(fileData, out var encodingTable);

                    File.WriteAllBytes(baseOutputPath + ".sf", compressedData);

                    using var writer = new BinaryWriter(File.Open(baseOutputPath + ".sf.meta", FileMode.Create));
                    writer.Write(fileData.Length);
                    writer.Write(originalExtension);
                    writer.Write(encodingTable.Count);
                    foreach (var kvp in encodingTable)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value);
                    }
                }
                else
                {
                    MessageBox.Show("ŒÊ«—“„Ì… €Ì— „œ⁄Ê„…");
                    progressBar.Visible = false;
                    return;
                }
            }

            stopwatch.Stop();
            progressBar.Visible = false;

            statusLabel.Text = $"?  „ ÷€ÿ {selectedFiles.Length} „·›(« ) »‰Ã«Õ.";
            timeLabel.Text = $"«·“„‰ «·„” €—ﬁ: {stopwatch.ElapsedMilliseconds} „··Ì À«‰Ì…";
        }

        private void DecompressButton_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "«Œ — „·› „÷€Êÿ ·›ﬂ «·÷€ÿ"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;

            string compressedPath = dialog.FileName;
            string metaPath = compressedPath + ".meta";

            if (!File.Exists(metaPath))
            {
                MessageBox.Show("„·› .meta €Ì— „ÊÃÊœ!");
                return;
            }

            byte[] compressedData = File.ReadAllBytes(compressedPath);
            string algorithm = algorithmComboBox.SelectedItem.ToString();

            int originalLength;
            string originalExtension;
            byte[] decompressedData;

            progressBar.Visible = true;
            var stopwatch = Stopwatch.StartNew();

            if (algorithm == "Huffman")
            {
                HuffmanNode tree;
                using var reader = new BinaryReader(File.OpenRead(metaPath));
                originalLength = reader.ReadInt32();
                originalExtension = reader.ReadString();
                tree = LoadHuffmanTree(reader);

                var decoder = new HuffmanDecoder();
                decompressedData = decoder.Decompress(compressedData, tree, originalLength);
            }
            else if (algorithm == "Shannon-Fano")
            {
                Dictionary<byte, string> encodingTable = new();
                using var reader = new BinaryReader(File.OpenRead(metaPath));
                originalLength = reader.ReadInt32();
                originalExtension = reader.ReadString();
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    byte symbol = reader.ReadByte();
                    string code = reader.ReadString();
                    encodingTable[symbol] = code;
                }

                var decoder = new Shannon_Fano_Decoder();
                decompressedData = decoder.Decompress(compressedData, encodingTable, originalLength);
            }
            else
            {
                MessageBox.Show("ŒÊ«—“„Ì… €Ì— „œ⁄Ê„….");
                return;
            }

            stopwatch.Stop();
            progressBar.Visible = false;

            string outputPath = Path.Combine(
                Path.GetDirectoryName(compressedPath),
                Path.GetFileNameWithoutExtension(compressedPath) + "_restored" + originalExtension
            );

            File.WriteAllBytes(outputPath, decompressedData);
            statusLabel.Text = $"?  „ ›ﬂ «·÷€ÿ ≈·Ï: {Path.GetFileName(outputPath)}";
            timeLabel.Text = $"«·“„‰ «·„” €—ﬁ: {stopwatch.ElapsedMilliseconds} „··Ì À«‰Ì…";
        }

        private void SaveHuffmanTree(BinaryWriter writer, HuffmanNode node)
        {
            if (node.IsLeaf)
            {
                writer.Write(true);
                writer.Write(node.Symbol.Value);
            }
            else
            {
                writer.Write(false);
                SaveHuffmanTree(writer, node.Left);
                SaveHuffmanTree(writer, node.Right);
            }
        }

        private HuffmanNode LoadHuffmanTree(BinaryReader reader)
        {
            bool isLeaf = reader.ReadBoolean();
            if (isLeaf)
            {
                byte symbol = reader.ReadByte();
                return new HuffmanNode { Symbol = symbol };
            }

            return new HuffmanNode
            {
                Left = LoadHuffmanTree(reader),
                Right = LoadHuffmanTree(reader)
            };
        }
    }
}
