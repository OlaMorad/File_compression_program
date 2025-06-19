using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using File_compression_program.Logic;

namespace File_compression_program.UI
{
    public class DecompressionUserControl : UserControl
    {
        private Button btnSelectFile;
        private TextBox txtFilePath;
        private TextBox txtPassword; // جديد: لإدخال كلمة السر عند فك الضغط
        private Label lblPassword;
        private Button btnDecompress;
        private ProgressBar progressBar;
        private Button btnCancel;
        private Label resultLabel;

        private BackgroundWorker bgWorker;
        private bool cancelRequested = false;
        private string userSelectedOutputPath;

        public DecompressionUserControl()
        {
            InitializeComponents();
            InitializeBackgroundWorker();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;

            txtFilePath = new TextBox()
            {
                ReadOnly = true,
                Width = 300,
                Left = 20,
                Top = 20,
            };

            btnSelectFile = new Button()
            {
                Text = "Select Compressed File",
                Left = txtFilePath.Right + 10,
                Top = txtFilePath.Top - 2,
                Width = 140,
            };
            btnSelectFile.Click += BtnSelectFile_Click;

            lblPassword = new Label()
            {
                Text = "Password (if any):",
                Left = 20,
                Top = txtFilePath.Bottom + 20,
                Width = 150,
            };

            txtPassword = new TextBox()
            {
                Left = lblPassword.Right + 10,
                Top = lblPassword.Top - 3,
                Width = 200,
                UseSystemPasswordChar = true,
            };

            btnDecompress = new Button()
            {
                Text = "Decompress",
                Left = 20,
                Top = lblPassword.Bottom + 20,
                Width = 120,
                Enabled = false,
            };
            btnDecompress.Click += BtnDecompress_Click;

            progressBar = new ProgressBar()
            {
                Left = btnDecompress.Right + 20,
                Top = btnDecompress.Top + 5,
                Width = 200,
                Height = 23,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Visible = false,
            };

            btnCancel = new Button()
            {
                Text = "Cancel",
                Left = progressBar.Right + 20,
                Top = btnDecompress.Top,
                Width = 80,
                Enabled = false,
            };
            btnCancel.Click += BtnCancel_Click;

            resultLabel = new Label()
            {
                Left = 20,
                Top = btnDecompress.Bottom + 20,
                Width = 600,
                Height = 60,
                AutoSize = false,
                ForeColor = System.Drawing.Color.DarkGreen,
            };

            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnSelectFile);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnDecompress);
            this.Controls.Add(progressBar);
            this.Controls.Add(btnCancel);
            this.Controls.Add(resultLabel);
        }

        private void InitializeBackgroundWorker()
        {
            bgWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select Compressed File";
            dlg.Filter = "Compressed files (*.huff;*.sf)|*.huff;*.sf|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dlg.FileName;
                btnDecompress.Enabled = true;
                resultLabel.Text = "";
            }
        }

        private void BtnDecompress_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("Please select a valid compressed file first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save Decompressed File As";
            saveDialog.FileName = Path.GetFileNameWithoutExtension(txtFilePath.Text);
            saveDialog.Filter = "All files (*.*)|*.*";

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            userSelectedOutputPath = saveDialog.FileName;

            btnDecompress.Enabled = false;
            btnCancel.Enabled = true;
            progressBar.Visible = true;
            progressBar.Value = 0;
            resultLabel.Text = "";
            cancelRequested = false;

            bgWorker.RunWorkerAsync((txtFilePath.Text, userSelectedOutputPath, txtPassword.Text));
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                cancelRequested = true;
                bgWorker.CancelAsync();
                btnCancel.Enabled = false;
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var (inputFile, outputFile, password) = ((string, string, string))e.Argument;

            var compressionManager = new CompressionManager();

            var startTime = DateTime.Now;

            // محاكاة تقدم فك الضغط - يمكن تعديلها حسب الحاجة
            for (int i = 0; i <= 100; i += 10)
            {
                if (bgWorker.CancellationPending || cancelRequested)
                {
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(100);
                bgWorker.ReportProgress(i);
            }

            try
            {
                var algorithm = GetAlgorithmByExtension(inputFile);
                compressionManager.Decompress(inputFile, outputFile, algorithm, password);

                var elapsed = DateTime.Now - startTime;

                e.Result = elapsed;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnCancel.Enabled = false;
            btnDecompress.Enabled = true;
            progressBar.Visible = false;

            if (e.Cancelled)
            {
                resultLabel.ForeColor = System.Drawing.Color.DarkOrange;
                resultLabel.Text = "Decompression canceled by user.";
                return;
            }

            if (e.Result is Exception ex)
            {
                resultLabel.ForeColor = System.Drawing.Color.Red;
                resultLabel.Text = $"Error during decompression: {ex.Message}";
                return;
            }

            if (e.Result is TimeSpan elapsed)
            {
                resultLabel.ForeColor = System.Drawing.Color.DarkGreen;
                resultLabel.Text = $"✅ Decompression completed!\n🕒 Time taken: {elapsed.TotalMilliseconds:F2} ms";
            }
        }

        private CompressionAlgorithm GetAlgorithmByExtension(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            return ext switch
            {
                ".huff" => CompressionAlgorithm.Huffman,
                ".sf" => CompressionAlgorithm.ShannonFano,
                _ => throw new NotSupportedException("Unsupported compression file extension."),
            };
        }
    }
}
