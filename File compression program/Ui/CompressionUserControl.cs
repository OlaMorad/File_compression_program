using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using File_compression_program.Logic;
using System.Linq;

namespace File_compression_program.UI
{
    public partial class CompressionUserControl : UserControl
    {
        private TextBox txtFilePath;
        private Button btnSelectFiles;
        private ComboBox algorithmComboBox;
        private TextBox txtPassword;
        private Label lblPassword;
        private Button btnStart;
        private Button btnCancel;
        private ProgressBar progressBar;
        private Label resultLabel;

        private string[] selectedFilePaths;
        private CancellationTokenSource cancellationTokenSource;

        public CompressionUserControl()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;

            txtFilePath = new TextBox()
            {
                ReadOnly = true,
                Width = 450,
                Left = 20,
                Top = 20,
                Multiline = true,
                Height = 60,
                ScrollBars = ScrollBars.Vertical
            };

            btnSelectFiles = new Button()
            {
                Text = "Select Files to Compress",
                Left = txtFilePath.Right + 10,
                Top = txtFilePath.Top,
                Width = 160,
                Height = 27,
            };
            btnSelectFiles.Click += BtnSelectFiles_Click;

            algorithmComboBox = new ComboBox()
            {
                Left = 20,
                Top = txtFilePath.Bottom + 20,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            algorithmComboBox.Items.AddRange(new[] { "Huffman", "Shannon-Fano" });
            algorithmComboBox.SelectedIndex = 0;

            lblPassword = new Label()
            {
                Text = "Password (optional):",
                Left = 20,
                Top = algorithmComboBox.Bottom + 20,
                Width = 150,
            };

            txtPassword = new TextBox()
            {
                Left = lblPassword.Right + 10,
                Top = lblPassword.Top - 3,
                Width = 200,
                UseSystemPasswordChar = true,
            };

            btnStart = new Button()
            {
                Text = "Start Compression",
                Left = 20,
                Top = txtPassword.Bottom + 20,
                Width = 140,
                Height = 35,
                Enabled = false
            };
            btnStart.Click += BtnStart_Click;

            btnCancel = new Button()
            {
                Text = "Cancel",
                Left = btnStart.Right + 20,
                Top = btnStart.Top,
                Width = 100,
                Height = 35,
                Enabled = false
            };
            btnCancel.Click += BtnCancel_Click;

            progressBar = new ProgressBar()
            {
                Left = btnCancel.Right + 20,
                Top = btnStart.Top + 10,
                Width = 200,
                Height = 23,
                Visible = false
            };

            resultLabel = new Label()
            {
                Left = 20,
                Top = btnStart.Bottom + 20,
                Width = 800,
                Height = 200,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DarkGreen,
                AutoSize = false
            };

            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnSelectFiles);
            this.Controls.Add(algorithmComboBox);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnStart);
            this.Controls.Add(btnCancel);
            this.Controls.Add(progressBar);
            this.Controls.Add(resultLabel);
        }

        private void BtnSelectFiles_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                selectedFilePaths = dlg.FileNames;
                txtFilePath.Lines = selectedFilePaths;
                btnStart.Enabled = selectedFilePaths.Length > 0;
                resultLabel.Text = "";
            }
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (selectedFilePaths == null || selectedFilePaths.Length == 0)
            {
                MessageBox.Show("Please select at least one file to compress.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var file in selectedFilePaths)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show($"File not found: {file}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            btnStart.Enabled = false;
            btnCancel.Enabled = true;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            resultLabel.Text = "";

            cancellationTokenSource = new CancellationTokenSource();

            CompressionAlgorithm algorithm = algorithmComboBox.SelectedItem.ToString() == "Huffman"
                ? CompressionAlgorithm.Huffman
                : CompressionAlgorithm.ShannonFano;

            string password = txtPassword.Text;

            try
            {
                var results = new System.Text.StringBuilder();

                await Task.Run(() =>
                {
                    Parallel.ForEach(selectedFilePaths, new ParallelOptions { CancellationToken = cancellationTokenSource.Token }, file =>
                    {
                        string outputPath = file + (algorithm == CompressionAlgorithm.Huffman ? ".huff" : ".sf");
                        var manager = new CompressionManager();

                        var start = DateTime.Now;
                        manager.Compress(file, outputPath, algorithm, password);
                        var duration = DateTime.Now - start;

                        var stats = new CompressionStats(file, outputPath);
                        lock (results)
                        {
                            results.AppendLine($"📄 {Path.GetFileName(file)}");
                            results.AppendLine(stats.GetCompressionSummaryWithTime(duration));
                            results.AppendLine(new string('-', 60));
                        }
                    });
                }, cancellationTokenSource.Token);

                resultLabel.Invoke(() => resultLabel.Text = $"✅ Compression completed for {selectedFilePaths.Length} files:\n\n" + results.ToString());
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Compression canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                resultLabel.Text = "Compression canceled.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during compression:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                resultLabel.Text = "Error occurred during compression.";
            }
            finally
            {
                progressBar.Visible = false;
                btnStart.Enabled = true;
                btnCancel.Enabled = false;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
