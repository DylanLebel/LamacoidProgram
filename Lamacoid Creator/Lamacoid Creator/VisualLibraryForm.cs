using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Visual lamacoid library browser with thumbnail gallery
    /// </summary>
    public class VisualLibraryForm : Form
    {
        // Data
        private LamacoidIndex index;
        private List<LamacoidItem> currentResults;
        private LamacoidItem selectedItem;

        // UI Controls
        private ComboBox templateComboBox;
        private TextBox searchTextBox;
        private FlowLayoutPanel thumbnailGallery;
        private Panel previewPanel;
        private PictureBox previewPictureBox;
        private Label previewLabel;
        private Button openPdfButton;
        private Button openDwgButton;
        private Button createNewButton;
        private ProgressBar indexProgressBar;
        private Label statusLabel;
        private BackgroundWorker indexWorker;

        public VisualLibraryForm()
        {
            InitializeLogging();
            InitializeComponents();
            InitializeIndex();
        }

        private void InitializeLogging()
        {
            try
            {
                Logger.Initialize(Constants.LogDirectory, Constants.MaxLogFileSizeBytes);
                Logger.Info("=== Visual Lamacoid Library Started ===");
                Logger.LogSystemInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize logging: {ex.Message}",
                    "Logging Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void InitializeComponents()
        {
            // Form settings
            this.Text = "Lamacoid Library";
            this.Size = new Size(1200, 800);
            this.BackColor = Constants.FormBackColor;
            this.Font = Constants.DefaultFont;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Template selector
            Label templateLabel = new Label
            {
                Text = "Template:",
                Location = new Point(10, 15),
                AutoSize = true
            };
            this.Controls.Add(templateLabel);

            templateComboBox = new ComboBox
            {
                Location = new Point(80, 10),
                Size = new Size(200, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            templateComboBox.SelectedIndexChanged += TemplateComboBox_SelectedIndexChanged;
            this.Controls.Add(templateComboBox);

            // Search box
            Label searchLabel = new Label
            {
                Text = "Search:",
                Location = new Point(300, 15),
                AutoSize = true
            };
            this.Controls.Add(searchLabel);

            searchTextBox = new TextBox
            {
                Location = new Point(360, 10),
                Size = new Size(300, 30)
            };
            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            this.Controls.Add(searchTextBox);

            // Status label
            statusLabel = new Label
            {
                Location = new Point(680, 15),
                Size = new Size(300, 20),
                Text = "Loading library...",
                AutoSize = false
            };
            this.Controls.Add(statusLabel);

            // Progress bar (for indexing)
            indexProgressBar = new ProgressBar
            {
                Location = new Point(10, 50),
                Size = new Size(1170, 20),
                Visible = false
            };
            this.Controls.Add(indexProgressBar);

            // Thumbnail gallery (left side)
            thumbnailGallery = new FlowLayoutPanel
            {
                Location = new Point(10, 80),
                Size = new Size(700, 650),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(thumbnailGallery);

            // Preview panel (right side)
            previewPanel = new Panel
            {
                Location = new Point(720, 80),
                Size = new Size(460, 650),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(previewPanel);

            // Preview picture box
            previewPictureBox = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(440, 330),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            previewPanel.Controls.Add(previewPictureBox);

            // Preview label
            previewLabel = new Label
            {
                Location = new Point(10, 350),
                Size = new Size(440, 200),
                Text = "Select a lamacoid to preview",
                Font = new Font("Segoe UI", 10),
                AutoSize = false
            };
            previewPanel.Controls.Add(previewLabel);

            // Action buttons
            openPdfButton = new Button
            {
                Location = new Point(10, 560),
                Size = new Size(140, 35),
                Text = "Open PDF",
                BackColor = Constants.ButtonPrimaryColor,
                ForeColor = Constants.ButtonTextColor,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            openPdfButton.FlatAppearance.BorderSize = 0;
            openPdfButton.Click += OpenPdfButton_Click;
            previewPanel.Controls.Add(openPdfButton);

            openDwgButton = new Button
            {
                Location = new Point(160, 560),
                Size = new Size(140, 35),
                Text = "Open DWG",
                BackColor = Constants.ButtonPrimaryColor,
                ForeColor = Constants.ButtonTextColor,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            openDwgButton.FlatAppearance.BorderSize = 0;
            openDwgButton.Click += OpenDwgButton_Click;
            previewPanel.Controls.Add(openDwgButton);

            createNewButton = new Button
            {
                Location = new Point(10, 605),
                Size = new Size(440, 35),
                Text = "+ Create New Lamacoid",
                BackColor = Constants.ButtonSuccessColor,
                ForeColor = Constants.ButtonTextColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            createNewButton.FlatAppearance.BorderSize = 0;
            createNewButton.Click += CreateNewButton_Click;
            previewPanel.Controls.Add(createNewButton);

            LoadTemplates();
        }

        private void InitializeIndex()
        {
            index = new LamacoidIndex();
            currentResults = new List<LamacoidItem>();

            // Create background worker for indexing
            indexWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            indexWorker.DoWork += IndexWorker_DoWork;
            indexWorker.ProgressChanged += IndexWorker_ProgressChanged;
            indexWorker.RunWorkerCompleted += IndexWorker_Completed;

            // Subscribe to index events
            index.ProgressChanged += Index_ProgressChanged;
            index.IndexCompleted += Index_IndexCompleted;

            // Start building index in background
            indexProgressBar.Visible = true;
            statusLabel.Text = "Indexing library...";
            indexWorker.RunWorkerAsync();
        }

        private void IndexWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            index.BuildIndex();
        }

        private void IndexWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is IndexProgressEventArgs args)
            {
                indexProgressBar.Value = args.PercentComplete;
                statusLabel.Text = $"Indexing: {args.Current}/{args.Total} ({args.PercentComplete}%)";
            }
        }

        private void IndexWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            indexProgressBar.Visible = false;
            statusLabel.Text = $"Library loaded: {index.Count} lamacoids";
            RefreshGallery();
        }

        private void Index_ProgressChanged(object sender, IndexProgressEventArgs e)
        {
            if (indexWorker != null && indexWorker.IsBusy)
            {
                indexWorker.ReportProgress(e.PercentComplete, e);
            }
        }

        private void Index_IndexCompleted(object sender, EventArgs e)
        {
            Logger.Info($"Index completed with {index.Count} lamacoids");
        }

        private void LoadTemplates()
        {
            try
            {
                if (!Directory.Exists(Constants.TemplatesPath))
                {
                    Logger.Warning($"Templates directory not found: {Constants.TemplatesPath}");
                    templateComboBox.Items.Add("All Templates");
                    templateComboBox.SelectedIndex = 0;
                    return;
                }

                string[] templateFiles = Directory.GetFiles(Constants.TemplatesPath, $"*{Constants.DwgExtension}");

                templateComboBox.Items.Add("All Templates");
                foreach (string filePath in templateFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    templateComboBox.Items.Add(fileName);
                }

                if (templateComboBox.Items.Count > 0)
                {
                    templateComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading templates", ex);
            }
        }

        private void TemplateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshGallery();
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            RefreshGallery();
        }

        private void RefreshGallery()
        {
            if (index == null || !index.IsIndexed)
                return;

            thumbnailGallery.Controls.Clear();
            selectedItem = null;
            UpdatePreview();

            try
            {
                // Get search results
                string searchTerm = searchTextBox.Text.Trim();
                currentResults = string.IsNullOrWhiteSpace(searchTerm)
                    ? index.AllLamacoids
                    : index.Search(searchTerm);

                // Update status
                statusLabel.Text = $"Found {currentResults.Count} lamacoid(s)";

                // Create thumbnail cards
                foreach (var item in currentResults)
                {
                    CreateThumbnailCard(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error refreshing gallery", ex);
            }
        }

        private void CreateThumbnailCard(LamacoidItem item)
        {
            Panel card = new Panel
            {
                Size = new Size(210, 190),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = item
            };

            // Thumbnail image
            PictureBox thumbnail = new PictureBox
            {
                Size = new Size(200, 120),
                Location = new Point(5, 5),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            if (!string.IsNullOrEmpty(item.ThumbnailPath) && File.Exists(item.ThumbnailPath))
            {
                try
                {
                    thumbnail.Image = Image.FromFile(item.ThumbnailPath);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to load thumbnail: {item.ThumbnailPath} - {ex.Message}");
                }
            }

            card.Controls.Add(thumbnail);

            // File number label
            Label numberLabel = new Label
            {
                Text = item.FileName,
                Location = new Point(5, 130),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(numberLabel);

            // Contents label
            Label contentsLabel = new Label
            {
                Text = item.DisplayName,
                Location = new Point(5, 150),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 8),
                TextAlign = ContentAlignment.TopCenter,
                AutoEllipsis = true
            };
            card.Controls.Add(contentsLabel);

            // Click event
            card.Click += (s, e) => SelectLamacoid(item);
            thumbnail.Click += (s, e) => SelectLamacoid(item);
            numberLabel.Click += (s, e) => SelectLamacoid(item);
            contentsLabel.Click += (s, e) => SelectLamacoid(item);

            thumbnailGallery.Controls.Add(card);
        }

        private void SelectLamacoid(LamacoidItem item)
        {
            selectedItem = item;
            UpdatePreview();
            Logger.Info($"Selected lamacoid: {item.FileName}");
        }

        private void UpdatePreview()
        {
            if (selectedItem == null)
            {
                previewPictureBox.Image = null;
                previewLabel.Text = "Select a lamacoid to preview";
                openPdfButton.Enabled = false;
                openDwgButton.Enabled = false;
                return;
            }

            // Load preview image
            if (!string.IsNullOrEmpty(selectedItem.ThumbnailPath) && File.Exists(selectedItem.ThumbnailPath))
            {
                try
                {
                    previewPictureBox.Image = Image.FromFile(selectedItem.ThumbnailPath);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to load preview: {selectedItem.ThumbnailPath} - {ex.Message}");
                }
            }

            // Update preview text
            previewLabel.Text = $"File: {selectedItem.FileName}\n" +
                               $"Contents: {selectedItem.Contents}\n" +
                               $"Description: {selectedItem.Description}\n" +
                               $"Colour: {selectedItem.Colour}\n" +
                               $"Adhesive: {selectedItem.Adhesive}\n" +
                               $"Finish: {selectedItem.Finish}\n" +
                               $"Thickness: {selectedItem.Thickness}\n" +
                               $"\nCreated: {selectedItem.CreatedDate:yyyy-MM-dd}\n" +
                               $"Modified: {selectedItem.ModifiedDate:yyyy-MM-dd}";

            openPdfButton.Enabled = selectedItem.PdfExists;
            openDwgButton.Enabled = selectedItem.DwgExists;
        }

        private void OpenPdfButton_Click(object sender, EventArgs e)
        {
            if (selectedItem != null && selectedItem.PdfExists)
            {
                try
                {
                    System.Diagnostics.Process.Start(selectedItem.PdfPath);
                    Logger.Info($"Opened PDF: {selectedItem.PdfPath}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to open PDF: {selectedItem.PdfPath}", ex);
                    MessageBox.Show($"Failed to open PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenDwgButton_Click(object sender, EventArgs e)
        {
            if (selectedItem != null && selectedItem.DwgExists)
            {
                try
                {
                    System.Diagnostics.Process.Start(selectedItem.DwgPath);
                    Logger.Info($"Opened DWG: {selectedItem.DwgPath}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to open DWG: {selectedItem.DwgPath}", ex);
                    MessageBox.Show($"Failed to open DWG: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CreateNewButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement creation workflow
            MessageBox.Show("Creation workflow coming soon!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
