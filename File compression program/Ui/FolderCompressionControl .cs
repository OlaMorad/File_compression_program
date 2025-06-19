using File_compression_program.Logic;
using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace File_compression_program.UI
{
    public partial class FolderCompressionControl : UserControl
    {
        private Button btnSelectFolder;
        private Button btnCompress;
        private Button btnDecompress;
        private Button btnExtractFile;
        private TextBox txtFolderPath;
        private Label lblStatus;

        public FolderCompressionControl()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;

            txtFolderPath = new TextBox()
            {
                Left = 20,
                Top = 20,
                Width = 500,
                ReadOnly = true
            };

            btnSelectFolder = new Button()
            {
                Text = "Select Folder",
                Left = txtFolderPath.Right + 10,
                Top = txtFolderPath.Top,
                Width = 120
            };
            btnSelectFolder.Click += BtnSelectFolder_Click;

            btnCompress = new Button()
            {
                Text = "Compress Folder",
                Left = 20,
                Top = txtFolderPath.Bottom + 20,
                Width = 150
            };
            btnCompress.Click += BtnCompress_Click;

            btnDecompress = new Button()
            {
                Text = "Decompress Zip",
                Left = btnCompress.Right + 20,
                Top = btnCompress.Top,
                Width = 150
            };
            btnDecompress.Click += BtnDecompress_Click;

            btnExtractFile = new Button()
            {
                Text = "Extract File From Zip",
                Left = btnDecompress.Right + 20,
                Top = btnDecompress.Top,
                Width = 180
            };
            btnExtractFile.Click += BtnExtractFile_Click;

            lblStatus = new Label()
            {
                Left = 20,
                Top = btnCompress.Bottom + 40,
                Width = 800,
                Height = 100,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DarkGreen
            };

            this.Controls.Add(txtFolderPath);
            this.Controls.Add(btnSelectFolder);
            this.Controls.Add(btnCompress);
            this.Controls.Add(btnDecompress);
            this.Controls.Add(btnExtractFile);
            this.Controls.Add(lblStatus);
        }

        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = dialog.SelectedPath;
                lblStatus.Text = "";
            }
        }

        private void BtnCompress_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtFolderPath.Text))
            {
                MessageBox.Show("Please select a valid folder first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string folderPath = txtFolderPath.Text;
            string outputZip = folderPath + ".zip";

            try
            {
                var start = DateTime.Now;
                FolderManager.CompressFolder(folderPath, outputZip);
                var duration = DateTime.Now - start;

                long originalSize = FolderManager.GetFolderSize(folderPath);
                long compressedSize = new FileInfo(outputZip).Length;

                var stats = new CompressionStats(originalSize, compressedSize);
                lblStatus.Text = "✅ Folder compressed!\n" + stats.GetCompressionSummaryWithTime(duration);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Compression failed:\n" + ex.Message);
            }
        }

        private void BtnDecompress_Click(object sender, EventArgs e)
        {
            using OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "ZIP files (*.zip)|*.zip";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string zipPath = dlg.FileName;
                string extractPath = Path.Combine(Path.GetDirectoryName(zipPath), Path.GetFileNameWithoutExtension(zipPath) + "_unzipped");

                try
                {
                    var start = DateTime.Now;
                    FolderManager.DecompressToFolder(zipPath, extractPath);
                    var duration = DateTime.Now - start;

                    long compressedSize = new FileInfo(zipPath).Length;
                    long extractedSize = FolderManager.GetFolderSize(extractPath);
                    var stats = new CompressionStats(extractedSize, compressedSize);

                    lblStatus.Text = "✅ Archive extracted!\n" + stats.GetCompressionSummaryWithTime(duration);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Extraction failed:\n" + ex.Message);
                }
            }
        }

        private void BtnExtractFile_Click(object sender, EventArgs e)
        {
            using OpenFileDialog zipDialog = new OpenFileDialog();
            zipDialog.Filter = "ZIP files (*.zip)|*.zip";
            if (zipDialog.ShowDialog() != DialogResult.OK) return;

            string zipPath = zipDialog.FileName;
            string[] entries = FolderManager.GetZipEntryNames(zipPath);

            if (entries.Length == 0)
            {
                MessageBox.Show("No entries found in archive.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedFile = entries.Length == 1
                ? entries[0]
                : PromptSelectEntry(entries);

            if (selectedFile == null) return;

            using FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                FolderManager.ExtractSingleFileFromZip(zipPath, selectedFile, folderDialog.SelectedPath);
                lblStatus.Text = $"✅ File '{selectedFile}' extracted to:\n{folderDialog.SelectedPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Extraction failed:\n" + ex.Message);
            }
        }

        private string PromptSelectEntry(string[] entries)
        {
            using Form prompt = new Form()
            {
                Width = 400,
                Height = 300,
                Text = "Select File from ZIP"
            };

            ListBox listBox = new ListBox() { Dock = DockStyle.Fill };
            listBox.Items.AddRange(entries);
            listBox.SelectedIndex = 0;

            Button okButton = new Button()
            {
                Text = "Extract",
                Dock = DockStyle.Bottom,
                Height = 30
            };

            okButton.Click += (s, e) => prompt.DialogResult = DialogResult.OK;

            prompt.Controls.Add(listBox);
            prompt.Controls.Add(okButton);

            return prompt.ShowDialog() == DialogResult.OK ? listBox.SelectedItem.ToString() : null;
        }
    }
}
