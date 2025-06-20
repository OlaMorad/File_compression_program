using System;
using System.Drawing;
using System.Windows.Forms;
using File_compression_program.Ui;
using File_compression_program.UI; 

namespace File_compression_program
{
    public partial class MainForm : Form
    {
        private Panel panelMain;
        private Button compressButton;
        private Button decompressButton;
        private Button FolderButton;
        private Button compareButton; // «·“— «·ÃœÌœ

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomUI();
        }
        private void InitializeCustomUI()
        {
            this.Text = "File Compression Program";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int buttonWidth = 150;
            int buttonHeight = 40;
            int spacing = 20;
            int startLeft = 30;
            int topPosition = 10;

            compressButton = new Button()
            {
                Text = "Compress File",
                Width = buttonWidth,
                Height = buttonHeight,
                Top = topPosition,
                Left = startLeft,
                Font = new Font("Segoe UI", 10)
            };
            compressButton.Click += CompressButton_Click;

            decompressButton = new Button()
            {
                Text = "Decompress File",
                Width = buttonWidth,
                Height = buttonHeight,
                Top = topPosition,
                Left = compressButton.Right + spacing,
                Font = new Font("Segoe UI", 10)
            };
            decompressButton.Click += DecompressButton_Click;

            compareButton = new Button()
            {
                Text = "Compare Algorithms",
                Width = buttonWidth,
                Height = buttonHeight,
                Top = topPosition,
                Left = decompressButton.Right + spacing,
                Font = new Font("Segoe UI", 10)
            };
            compareButton.Click += CompareButton_Click;

            FolderButton = new Button()
            {
                Text = "Folder Compression",
                Width = buttonWidth,
                Height = buttonHeight,
                Top = topPosition,
                Left = compareButton.Right + spacing,
                Font = new Font("Segoe UI", 10)
            };
            FolderButton.Click += FolderButton_Click;

            panelMain = new Panel()
            {
                Top = compressButton.Bottom + 10,
                Left = 10,
                Width = this.ClientSize.Width - 20,
                Height = this.ClientSize.Height - (compressButton.Bottom + 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            panelMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.Add(compressButton);
            this.Controls.Add(decompressButton);
            this.Controls.Add(compareButton);
            this.Controls.Add(FolderButton);
            this.Controls.Add(panelMain);

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
private void FolderButton_Click(object sender, EventArgs e)
        {
            panelMain.Controls.Clear();
            var folderControl = new FolderCompressionControl(); // ·«“„  ‰‘∆ Â–« «·ÌÊ“— ﬂÊ‰ —Ê·
            folderControl.Dock = DockStyle.Fill;
            panelMain.Controls.Add(folderControl);
        }

    }
}
