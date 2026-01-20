using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Lamacoid_Creator
{
    public class LamacoidForm : Form
    {
        // DraftSight Helper
        private readonly IDraftSightHelper draftSightHelper;

        // UI Controls
        private Button addButton;
        private Button continueButton;
        private Button deleteButton;
        private TextBox textBoxName;
        private ListBox listBoxNames;
        private NumericUpDown variationsNumericUpDown;
        private ComboBox templateComboBox;
        private ProgressBar progressBar;
        private CheckBox cycleModeCheckBox;

        // Data tracking
        private HashSet<string> uniqueNames = new HashSet<string>();
        private Dictionary<string, bool> pdfCreationStatus = new Dictionary<string, bool>();

        public LamacoidForm()
        {
            // Initialize logging system
            try
            {
                Logger.Initialize(Constants.LogDirectory, Constants.MaxLogFileSizeBytes);
                Logger.Info("=== Lamacoid Creator Application Started ===");
                Logger.LogSystemInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize logging system: {ex.Message}\nThe application will continue but logging may not work.",
                    "Logging Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            // Initialize DraftSight helper based on test mode
            draftSightHelper = DraftSightHelperFactory.Create();

            InitializeComponents();
            LoadTemplates();
        }

        private void InitializeComponents()
        {
            // Set form properties
            this.Text = "Lamacoid Creator";
            this.BackColor = Constants.FormBackColor;
            this.Font = Constants.DefaultFont;
            this.Size = new Size(Constants.FormWidth, Constants.FormHeight);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Initialize textBoxName
            textBoxName = new TextBox();
            textBoxName.Location = new Point(10, 10);
            textBoxName.Size = new Size(200, 30);
            textBoxName.Text = ""; // PlaceholderText not available in .NET Framework 4.7.2
            textBoxName.KeyPress += TextBoxName_KeyPress;
            this.Controls.Add(textBoxName);

            // Initialize addButton
            addButton = new Button();
            addButton.Text = "Add";
            addButton.Location = new Point(10, 40);
            addButton.Size = new Size(100, 30);
            addButton.BackColor = Constants.ButtonPrimaryColor;
            addButton.ForeColor = Constants.ButtonTextColor;
            addButton.FlatStyle = FlatStyle.Flat;
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += AddButton_Click;
            this.Controls.Add(addButton);

            // Initialize deleteButton
            deleteButton = new Button();
            deleteButton.Text = "Delete";
            deleteButton.Location = new Point(220, 40);
            deleteButton.Size = new Size(100, 30);
            deleteButton.BackColor = Constants.ButtonDangerColor;
            deleteButton.ForeColor = Constants.ButtonTextColor;
            deleteButton.FlatStyle = FlatStyle.Flat;
            deleteButton.FlatAppearance.BorderSize = 0;
            deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(deleteButton);

            // Initialize continueButton
            continueButton = new Button();
            continueButton.Text = "Continue";
            continueButton.Location = new Point(330, 40);
            continueButton.Size = new Size(100, 30);
            continueButton.BackColor = Constants.ButtonSuccessColor;
            continueButton.ForeColor = Constants.ButtonTextColor;
            continueButton.FlatStyle = FlatStyle.Flat;
            continueButton.FlatAppearance.BorderSize = 0;
            continueButton.Click += ContinueButton_Click;
            this.Controls.Add(continueButton);

            // Initialize listBoxNames
            listBoxNames = new ListBox();
            listBoxNames.Location = new Point(10, 80);
            listBoxNames.Size = new Size(420, 150);
            this.Controls.Add(listBoxNames);

            // Initialize variationsNumericUpDown
            variationsNumericUpDown = new NumericUpDown();
            variationsNumericUpDown.Location = new Point(10, 240);
            variationsNumericUpDown.Size = new Size(100, 30);
            variationsNumericUpDown.Minimum = 0;
            variationsNumericUpDown.Maximum = 10;
            variationsNumericUpDown.Value = 1;
            this.Controls.Add(variationsNumericUpDown);

            // Initialize cycleModeCheckBox
            cycleModeCheckBox = new CheckBox();
            cycleModeCheckBox.Location = new Point(120, 240);
            cycleModeCheckBox.Text = "Cycle Mode";
            cycleModeCheckBox.AutoSize = true;
            this.Controls.Add(cycleModeCheckBox);

            // Initialize templateComboBox
            templateComboBox = new ComboBox();
            templateComboBox.Location = new Point(10, 280);
            templateComboBox.Size = new Size(200, 30);
            templateComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(templateComboBox);

            // Initialize progressBar
            progressBar = new ProgressBar();
            progressBar.Location = new Point(10, 320);
            progressBar.Size = new Size(420, 30);
            this.Controls.Add(progressBar);

            // Add labels for clarity
            Label variationsLabel = new Label();
            variationsLabel.Text = "Variations:";
            variationsLabel.Location = new Point(10, 220);
            variationsLabel.AutoSize = true;
            this.Controls.Add(variationsLabel);

            Label templateLabel = new Label();
            templateLabel.Text = "Template:";
            templateLabel.Location = new Point(10, 260);
            templateLabel.AutoSize = true;
            this.Controls.Add(templateLabel);
        }

        private void LoadTemplates()
        {
            Logger.Info($"Loading templates from: {Constants.TemplatesPath}");

            if (!Directory.Exists(Constants.TemplatesPath))
            {
                Logger.Error($"Templates directory not found at: {Constants.TemplatesPath}");
                MessageBox.Show(
                    $"Templates directory not found at:\n{Constants.TemplatesPath}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string[] templateFiles = Directory.GetFiles(Constants.TemplatesPath, $"*{Constants.DwgExtension}");
            Logger.Info($"Found {templateFiles.Length} template files");

            foreach (string filePath in templateFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                templateComboBox.Items.Add(fileName);
                Logger.Debug($"Added template: {fileName}");
            }

            if (templateComboBox.Items.Count > 0)
            {
                templateComboBox.SelectedIndex = 0;
                Logger.Info($"Default template selected: {templateComboBox.SelectedItem}");
            }
            else
            {
                Logger.Warning("No templates found in directory");
                MessageBox.Show(
                    "No templates found in directory.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void TextBoxName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                AddNameToList();
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddNameToList();
        }

        private void AddNameToList()
        {
            string nameToAdd = textBoxName.Text.Trim();

            if (string.IsNullOrEmpty(nameToAdd))
            {
                MessageBox.Show(
                    "Please enter a name.",
                    "Empty Name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Case-insensitive duplicate check
            if (uniqueNames.Any(n => n.Equals(nameToAdd, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(
                    "This name already exists in the list.",
                    "Duplicate Name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            listBoxNames.Items.Add(nameToAdd);
            uniqueNames.Add(nameToAdd);
            textBoxName.Clear();
            textBoxName.Focus();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (listBoxNames.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Please select a name to delete.",
                    "No Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(
                "Are you sure you want to delete this name?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string nameToRemove = listBoxNames.SelectedItem.ToString();
                listBoxNames.Items.RemoveAt(listBoxNames.SelectedIndex);
                uniqueNames.Remove(nameToRemove);
            }
        }

        private void ContinueButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            ProcessLamacoids();
        }

        private bool ValidateInputs()
        {
            if (templateComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a template first.",
                    "Template Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            if (listBoxNames.Items.Count == 0)
            {
                MessageBox.Show(
                    "Please add names to the list.",
                    "Names Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ProcessLamacoids()
        {
            Stopwatch totalTimer = Stopwatch.StartNew();
            Logger.Info("=== Starting Lamacoid Processing ===");

            int numberOfVariations = (int)variationsNumericUpDown.Value;
            bool cycleMode = cycleModeCheckBox.Checked;

            Logger.Info($"Processing Parameters:");
            Logger.Info($"  - Number of variations: {numberOfVariations}");
            Logger.Info($"  - Cycle mode: {cycleMode}");
            Logger.Info($"  - Names to process: {listBoxNames.Items.Count}");

            progressBar.Maximum = listBoxNames.Items.Count * (cycleMode ? numberOfVariations : 1);
            progressBar.Value = 0;
            pdfCreationStatus.Clear();

            string templateFilePath = GetTemplateFilePath();
            if (templateFilePath == null)
            {
                Logger.Error("Template file path is null, aborting processing");
                return;
            }

            Logger.Info($"Using template: {templateFilePath}");

            ProcessAllNames(templateFilePath, numberOfVariations);

            totalTimer.Stop();
            Logger.LogPerformance("Initial Lamacoid Processing", totalTimer);

            MessageBox.Show(
                "Initial processing complete. Checking for any failed PDF creations.",
                "Info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            RetryFailedPdfCreations();

            Logger.Info("=== Lamacoid Processing Complete ===");

            MessageBox.Show(
                "Processing Complete!",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            PromptForMoreWork();
        }

        private string GetTemplateFilePath()
        {
            string selectedTemplateName = templateComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTemplateName))
            {
                MessageBox.Show(
                    "Template not selected.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return null;
            }

            string templateFilePath = Path.Combine(
                Constants.TemplatesPath,
                selectedTemplateName + Constants.DwgExtension);

            if (!File.Exists(templateFilePath))
            {
                MessageBox.Show(
                    $"Template file not found:\n{templateFilePath}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return null;
            }

            return templateFilePath;
        }

        private void ProcessAllNames(string templateFilePath, int numberOfVariations)
        {
            foreach (string baseName in listBoxNames.Items)
            {
                ProcessSingleName(baseName, templateFilePath, numberOfVariations);
            }
        }

        private void ProcessSingleName(string baseName, string templateFilePath, int numberOfVariations)
        {
            int startNumber = cycleModeCheckBox.Checked ? 1 : numberOfVariations;
            int endNumber = cycleModeCheckBox.Checked ? numberOfVariations : startNumber;

            for (int i = startNumber; i <= endNumber; i++)
            {
                ProcessSingleVariation(baseName, i, templateFilePath);
            }
        }

        private void ProcessSingleVariation(string baseName, int variationNumber, string templateFilePath)
        {
            Stopwatch variationTimer = Stopwatch.StartNew();
            string variantName = $"{baseName}-{variationNumber:D2}";
            string newFileName = draftSightHelper.GenerateNextFileName(Constants.LamacoidDirectory) + Constants.DwgExtension;
            string newFilePath = Path.Combine(Constants.LamacoidDirectory, newFileName);

            Logger.Info($"Processing variation: {variantName}");
            Logger.Debug($"  - New file name: {newFileName}");
            Logger.Debug($"  - New file path: {newFilePath}");

            try
            {
                // Copy template
                Logger.Debug($"Copying template from {templateFilePath} to {newFilePath}");
                File.Copy(templateFilePath, newFilePath, true);
                Logger.Info($"Template copied successfully to {newFileName}");

                // Open and update document
                Logger.Debug($"Opening template document: {newFilePath}");
                DraftSight.Interop.dsAutomation.Document copiedDoc = draftSightHelper.OpenTemplate(newFilePath);
                if (copiedDoc == null)
                {
                    Logger.Error($"Failed to open copied template for {variantName}");
                    return;
                }
                Logger.Info($"Template opened successfully for {variantName}");

                // Update properties
                Logger.Debug($"Updating custom properties for {variantName}");
                draftSightHelper.UpdateCustomProperty(
                    copiedDoc,
                    Constants.DescriptionProperty,
                    $"{variantName}{Constants.DescriptionSuffix}");

                draftSightHelper.UpdateCustomProperty(
                    copiedDoc,
                    Constants.ContentsProperty,
                    variantName);

                copiedDoc.Save();
                Logger.Info($"Properties updated and DWG saved: {newFileName}");

                // Export to PDF
                string pdfFilePath = newFilePath.Replace(Constants.DwgExtension, Constants.PdfExtension);
                Logger.Debug($"Exporting to PDF: {pdfFilePath}");
                bool isPdfCreated = draftSightHelper.ExportToPdf(newFilePath, pdfFilePath);
                pdfCreationStatus.Add(pdfFilePath, isPdfCreated);

                if (!isPdfCreated)
                {
                    Logger.Warning($"Failed to create PDF: {pdfFilePath}");
                }
                else
                {
                    Logger.Info($"PDF created successfully: {Path.GetFileName(pdfFilePath)}");
                }

                // Close document
                Logger.Debug($"Closing document: {newFilePath}");
                draftSightHelper.CloseDocument(copiedDoc, newFilePath);
                Logger.Info($"Document closed: {newFileName}");

                variationTimer.Stop();
                Logger.LogPerformance($"Processing {variantName}", variationTimer);

                // Update progress bar
                this.Invoke(new Action(() => progressBar.Value++));
            }
            catch (Exception ex)
            {
                Logger.Error($"Error processing variation {variantName}", ex);
                MessageBox.Show(
                    $"Error processing {variantName}:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RetryFailedPdfCreations()
        {
            var failedPdfs = pdfCreationStatus.Where(e => !e.Value).ToList();

            if (failedPdfs.Count == 0)
            {
                Logger.Info("No failed PDF creations to retry");
                return;
            }

            Logger.Warning($"Retrying {failedPdfs.Count} failed PDF creations...");

            int successCount = 0;
            int failCount = 0;

            foreach (var entry in failedPdfs)
            {
                string dwgFilePath = entry.Key.Replace(Constants.PdfExtension, Constants.DwgExtension);
                Logger.Info($"Retrying PDF creation for: {Path.GetFileName(dwgFilePath)}");

                bool retrySuccess = draftSightHelper.ExportToPdf(dwgFilePath, entry.Key);
                if (!retrySuccess)
                {
                    Logger.Error($"Retry failed for PDF: {Path.GetFileName(entry.Key)}");
                    failCount++;
                }
                else
                {
                    Logger.Info($"Retry succeeded for PDF: {Path.GetFileName(entry.Key)}");
                    successCount++;
                }
            }

            Logger.Info($"PDF Retry Results: {successCount} succeeded, {failCount} failed");
        }

        private void PromptForMoreWork()
        {
            var result = MessageBox.Show(
                "Do you want to process more lamacoids?",
                "Continue",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ResetForm();
            }
            else
            {
                this.Close();
            }
        }

        private void ResetForm()
        {
            Logger.Info("Resetting form for new batch");
            listBoxNames.Items.Clear();
            uniqueNames.Clear();
            progressBar.Value = 0;
            pdfCreationStatus.Clear();
            textBoxName.Clear();
            textBoxName.Focus();
            Logger.Info("Form reset complete");
        }
    }
}
