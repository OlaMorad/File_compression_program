using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using File_compression_program.Logic;
using System.Text;

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
        private Button btnPauseResume;
        private Button btnCancel;
        private ProgressBar progressBar;
        private Label resultLabel;

        private string[] selectedFilePaths;
        private CompressionTaskManager taskManager;

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

            btnPauseResume = new Button()
            {
                Text = "⏸ Pause",
                Left = btnStart.Right + 20,
                Top = btnStart.Top,
                Width = 100,
                Enabled = false
            };
            btnPauseResume.Click += BtnPauseResume_Click;

            btnCancel = new Button()
            {
                Text = "Cancel",
                Left = btnPauseResume.Right + 20,
                Top = btnStart.Top,
                Width = 100,
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
            this.Controls.Add(btnPauseResume);
            this.Controls.Add(btnCancel);
            this.Controls.Add(progressBar);
            this.Controls.Add(resultLabel);
        }

        private void BtnSelectFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Multiselect = true;
                dlg.Filter = "All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePaths = dlg.FileNames;
                    txtFilePath.Lines = selectedFilePaths;
                    btnStart.Enabled = selectedFilePaths.Length > 0;
                    resultLabel.Text = "";
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (selectedFilePaths == null || selectedFilePaths.Length == 0)
            {
                MessageBox.Show("Please select at least one file to compress.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var file in selectedFilePaths)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show($"File not found: {file}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            btnStart.Enabled = false;
            btnPauseResume.Enabled = true;
            btnCancel.Enabled = true;
            progressBar.Visible = true;
            progressBar.Value = 0;
            resultLabel.Text = "Starting compression...";

            var algorithm = algorithmComboBox.SelectedItem.ToString() == "Huffman"
                ? CompressionAlgorithm.Huffman
                : CompressionAlgorithm.ShannonFano;

            string password = txtPassword.Text;

            taskManager = new CompressionTaskManager();
            taskManager.ProgressChanged += percent =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => progressBar.Value = percent));
                else
                    progressBar.Value = percent;
            };

            taskManager.Completed += message =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() =>
                    {
                        resultLabel.Text = message;
                        ResetButtons();
                    }));
                else
                {
                    resultLabel.Text = message;
                    ResetButtons();
                }
            };

            taskManager.Failed += message =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() =>
                    {
                        resultLabel.Text = message;
                        ResetButtons();
                    }));
                else
                {
                    resultLabel.Text = message;
                    ResetButtons();
                }
            };

            taskManager.StartCompressMultipleFiles(selectedFilePaths, algorithm, password);
        }

        private void BtnPauseResume_Click(object sender, EventArgs e)
        {
            if (taskManager == null) return;

            if (taskManager.IsPaused)
            {
                taskManager.Resume();
                btnPauseResume.Text = "⏸ Pause";
            }
            else
            {
                taskManager.Pause();
                btnPauseResume.Text = "▶ Resume";
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            taskManager?.Cancel();
            btnPauseResume.Enabled = false;
            btnCancel.Enabled = false;
        }

        private void ResetButtons()
        {
            btnStart.Enabled = true;
            btnPauseResume.Enabled = false;
            btnCancel.Enabled = false;
            btnPauseResume.Text = "⏸ Pause";
            progressBar.Visible = false;
        }
    }
}
