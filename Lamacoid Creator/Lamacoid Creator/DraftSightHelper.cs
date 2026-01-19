using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DraftSight.Interop.dsAutomation;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Helper class for DraftSight automation operations
    /// </summary>
    public static class DraftSightHelper
    {
        private static DraftSight.Interop.dsAutomation.Application dsApp;

        /// <summary>
        /// Generates the next available file name based on existing files
        /// </summary>
        public static string GenerateNextFileName(string folderPath)
        {
            Logger.Debug($"Generating next file name for folder: {folderPath}");
            int currentMax = FindHighestFileNumber(folderPath);
            int nextNumber = currentMax + 1;
            string fileName = $"{Constants.FileNamePrefix}{nextNumber.ToString(Constants.FileNameFormat)}";
            Logger.Debug($"Generated file name: {fileName} (next number: {nextNumber})");
            return fileName;
        }

        /// <summary>
        /// Finds the highest numbered file in the directory
        /// </summary>
        private static int FindHighestFileNumber(string folderPath)
        {
            var regex = new Regex($@"{Constants.FileNamePrefix}(\d+){Constants.DwgExtension}");
            int maxNumber = 0;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                return maxNumber;
            }

            foreach (var file in Directory.EnumerateFiles(folderPath, $"{Constants.FileNamePrefix}*{Constants.DwgExtension}"))
            {
                var match = regex.Match(Path.GetFileName(file));
                if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                {
                    maxNumber = Math.Max(maxNumber, number);
                }
            }

            return maxNumber;
        }

        /// <summary>
        /// Initializes connection to DraftSight application
        /// </summary>
        private static DraftSight.Interop.dsAutomation.Application InitializeDraftSight()
        {
            Logger.Info("Attempting to connect to DraftSight application...");
            try
            {
                dsApp = (DraftSight.Interop.dsAutomation.Application)Marshal.GetActiveObject("DraftSight.Application");
                Logger.Info("Successfully connected to DraftSight application");
                return dsApp;
            }
            catch (Exception ex)
            {
                Logger.Fatal("Failed to connect to DraftSight application", ex);
                MessageBox.Show(
                    "Error: DraftSight application not found. Please ensure DraftSight is running.",
                    "DraftSight Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Opens a template file in DraftSight
        /// </summary>
        public static Document OpenTemplate(string templateFilePath)
        {
            Logger.Debug($"OpenTemplate called for: {templateFilePath}");

            if (dsApp == null)
            {
                Logger.Warning("DraftSight application reference is null, initializing...");
                dsApp = InitializeDraftSight();
                if (dsApp == null)
                {
                    Logger.Error("DraftSight application is not running");
                    return null;
                }
            }

            Stopwatch openTimer = Stopwatch.StartNew();
            try
            {
                Document dsDoc = dsApp.OpenDocument2(
                    templateFilePath,
                    dsDocumentOpenOption_e.dsDocumentOpen_Default,
                    dsEncoding_e.dsEncoding_Default);
                openTimer.Stop();
                Logger.LogPerformance($"Opening document {Path.GetFileName(templateFilePath)}", openTimer);
                Logger.Info($"Successfully opened template: {Path.GetFileName(templateFilePath)}");
                return dsDoc;
            }
            catch (Exception ex)
            {
                openTimer.Stop();
                Logger.Error($"Failed to open template: {Path.GetFileName(templateFilePath)}", ex);
                return null;
            }
        }

        /// <summary>
        /// Updates a custom property in a DraftSight document
        /// </summary>
        public static bool UpdateCustomProperty(
            Document dsDoc,
            string propertyName,
            string propertyValue,
            string logFilePath = null)
        {
            Logger.Debug($"UpdateCustomProperty called: {propertyName} = {propertyValue}");

            if (dsDoc == null)
            {
                Logger.Error("Document is null, cannot update custom property");
                return false;
            }

            bool updateSuccessful = false;
            int retryCount = 0;

            while (!updateSuccessful && retryCount < Constants.MaxPropertyUpdateRetries)
            {
                try
                {
                    Logger.Debug($"Attempt {retryCount + 1}/{Constants.MaxPropertyUpdateRetries} to update property '{propertyName}'");
                    DrawingProperties dsDrawingProperties = dsDoc.GetDrawingProperties();

                    if (dsDrawingProperties.HasCustomProperty(propertyName))
                    {
                        Logger.Debug($"Property '{propertyName}' exists, updating value");
                        dsDrawingProperties.SetCustomProperty(propertyName, propertyValue);
                    }
                    else
                    {
                        Logger.Debug($"Property '{propertyName}' does not exist, adding new property");
                        dsDrawingProperties.AddCustomProperty(propertyName, propertyValue);
                    }

                    dsDoc.Save();

                    // Validate the updated property
                    string updatedValue = dsDrawingProperties.GetCustomProperty(propertyName);
                    if (updatedValue == propertyValue)
                    {
                        Logger.Info($"Successfully updated property '{propertyName}' to '{propertyValue}'");
                        updateSuccessful = true;
                    }
                    else
                    {
                        Logger.Warning($"Property update validation failed. Expected: '{propertyValue}', Found: '{updatedValue}'");
                        throw new InvalidOperationException("Property update validation failed.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error updating property '{propertyName}' (attempt {retryCount + 1})", ex);
                    retryCount++;
                    if (retryCount < Constants.MaxPropertyUpdateRetries)
                    {
                        Logger.Info($"Waiting {Constants.PropertyUpdateRetryDelayMs}ms before retry...");
                        Thread.Sleep(Constants.PropertyUpdateRetryDelayMs);
                    }
                }
            }

            if (!updateSuccessful)
            {
                Logger.Error($"Failed to update property '{propertyName}' after {retryCount} attempts");
            }

            return updateSuccessful;
        }

        /// <summary>
        /// Closes a DraftSight document
        /// </summary>
        public static void CloseDocument(Document dsDoc, string documentFilePath)
        {
            Logger.Debug($"CloseDocument called for: {Path.GetFileName(documentFilePath)}");

            if (dsDoc != null && dsApp != null)
            {
                try
                {
                    dsApp.CloseDocument(documentFilePath, false);
                    Logger.Info($"Successfully closed document: {Path.GetFileName(documentFilePath)}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error closing document: {Path.GetFileName(documentFilePath)}", ex);
                }
            }
            else
            {
                Logger.Warning($"Cannot close document - dsDoc or dsApp is null");
            }
        }

        /// <summary>
        /// Gets a custom property value from drawing properties
        /// </summary>
        public static string GetPropertyValue(object customProps, string propertyName)
        {
            try
            {
                if (customProps == null)
                    return string.Empty;

                // DraftSight API: Get property value using dynamic to handle COM interop
                dynamic props = customProps;
                string value = props.Item(propertyName);
                return value ?? string.Empty;
            }
            catch
            {
                // Property doesn't exist or error reading it
                return string.Empty;
            }
        }

        /// <summary>
        /// Exports a DWG file to PDF format
        /// </summary>
        public static bool ExportToPdf(string dwgFilePath, string pdfFilePath)
        {
            Logger.Info($"Starting PDF export: {Path.GetFileName(dwgFilePath)} -> {Path.GetFileName(pdfFilePath)}");
            Stopwatch exportTimer = Stopwatch.StartNew();
            bool success = false;
            int attempt = 0;

            while (!success && attempt < Constants.MaxPdfExportAttempts)
            {
                try
                {
                    attempt++;
                    Logger.Debug($"PDF export attempt {attempt}/{Constants.MaxPdfExportAttempts}");

                    DraftSight.Interop.dsAutomation.Application dsAppInstance = (DraftSight.Interop.dsAutomation.Application)Marshal.GetActiveObject("DraftSight.Application");
                    dsAppInstance.AbortRunningCommand();
                    Logger.Debug("Aborted any running commands in DraftSight");

                    Document dsDoc = dsAppInstance.GetActiveDocument();
                    if (dsDoc == null)
                    {
                        throw new InvalidOperationException("No active document found in DraftSight.");
                    }
                    Logger.Debug("Active document obtained");

                    PrintManager dsPrintMgr = dsAppInstance.GetPrintManager();
                    dsPrintMgr.Printer = Constants.PdfPrinterName;
                    dsPrintMgr.PaperSize = Constants.PdfPaperSize;
                    dsPrintMgr.Orientation = dsPrintOrientation_e.dsPrintOrientation_Landscape;
                    Logger.Debug($"Print settings configured: Printer={Constants.PdfPrinterName}, Paper={Constants.PdfPaperSize}, Orientation=Landscape");

                    string[] sheetArray = new string[1] { Constants.PdfLayoutSheet };
                    dsPrintMgr.SetSheets(sheetArray);
                    Logger.Debug($"Sheet set to: {Constants.PdfLayoutSheet}");

                    dsPrintMgr.PrintOut(1, pdfFilePath);
                    exportTimer.Stop();
                    Logger.LogPerformance($"PDF Export for {Path.GetFileName(pdfFilePath)}", exportTimer);
                    Logger.Info($"PDF exported successfully: {Path.GetFileName(pdfFilePath)}");
                    success = true;
                }
                catch (Exception ex)
                {
                    if (attempt < Constants.MaxPdfExportAttempts)
                    {
                        Logger.Warning($"PDF export attempt {attempt}/{Constants.MaxPdfExportAttempts} failed: {ex.Message}. Retrying in {Constants.PdfExportDelayMs / 1000} seconds...");
                        Thread.Sleep(Constants.PdfExportDelayMs);
                    }
                    else
                    {
                        exportTimer.Stop();
                        Logger.Error($"Failed to export PDF after {Constants.MaxPdfExportAttempts} attempts", ex);
                    }
                }
            }

            return success;
        }

    }
}
