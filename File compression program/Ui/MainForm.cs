using File_compression_program.Ui;
using File_compression_program.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace File_compression_program
{
    public partial class MainForm : Form
    {
        private Panel panelMain;
        private Button compressButton;
        private Button decompressButton;
        private Button compareButton; // «·“— «·ÃœÌœ

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomUI();
        }

        private void InitializeCustomUI()
        {
            this.Text = "File Compression Program";
            this.Width = 800; // “œ‰« «·⁄—÷ ·«” Ì⁄«» «·“— «·ÃœÌœ
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // √“—«— «·÷€ÿ Ê›ﬂ «·÷€ÿ Ê«·„ﬁ«—‰… ›Ì «·√⁄·Ï
            compressButton = new Button()
            {
                Text = "Compress File",
                Width = 150,
                Height = 40,
                Top = 10,
                Left = 20,
                Font = new Font("Segoe UI", 10)
            };
            compressButton.Click += CompressButton_Click;

            decompressButton = new Button()
            {
                Text = "Decompress File",
                Width = 150,
                Height = 40,
                Top = 10,
                Left = 190,
                Font = new Font("Segoe UI", 10)
            };
            decompressButton.Click += DecompressButton_Click;

            // «·“— «·ÃœÌœ ··„ﬁ«—‰…
            compareButton = new Button()
            {
                Text = "Compare Algorithms",
                Width = 150,
                Height = 40,
                Top = 10,
                Left = 360,
                Font = new Font("Segoe UI", 10)
            };
            compareButton.Click += CompareButton_Click;

            // Panel ·Ì⁄—÷ «·„Õ ÊÏ
            panelMain = new Panel()
            {
                Top = 60,
                Left = 10,
                Width = this.ClientSize.Width - 20,
                Height = this.ClientSize.Height - 70,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.Add(compressButton);
            this.Controls.Add(decompressButton);
            this.Controls.Add(compareButton); // ≈÷«›… «·“— «·ÃœÌœ
            this.Controls.Add(panelMain);

            // ⁄—÷ —”«·…  —ÕÌ» √Ê·« œ«Œ· «·‹ Panel («Œ Ì«—Ì)
            ShowWelcomeMessage();
        }

        private void ShowWelcomeMessage()
        {
            panelMain.Controls.Clear();
            var welcomeLabel = new Label()
            {
                Text = "Welcome to the File Compression Program!\nPlease choose an option above to start.",
                AutoSize = false,
                Width = panelMain.Width - 20,
                Height = panelMain.Height - 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(welcomeLabel);
        }

        private void CompressButton_Click(object sender, EventArgs e)
        {
            panelMain.Controls.Clear();
            var compressionControl = new CompressionUserControl();
            compressionControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(compressionControl);
        }

        private void DecompressButton_Click(object sender, EventArgs e)
        {
            panelMain.Controls.Clear();
            var decompressionControl = new DecompressionUserControl();
            decompressionControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(decompressionControl);
        }

        // «·œ«·… «·ÃœÌœ… ·“— «·„ﬁ«—‰…
        private void CompareButton_Click(object sender, EventArgs e)
        {
            panelMain.Controls.Clear();
            var comparisonControl = new ComparisonUserControl();
            comparisonControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(comparisonControl);
        }
    }
}