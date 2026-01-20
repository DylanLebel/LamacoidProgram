using System;
using System.IO;
using System.Text.RegularExpressions;
using DraftSight.Interop.dsAutomation;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Mock implementation of IDraftSightHelper for testing without DraftSight installed.
    /// Simulates DraftSight operations using file system operations and logging.
    /// </summary>
    public class MockDraftSightHelper : IDraftSightHelper
    {
        /// <summary>
        /// Generates the next available file name based on existing files
        /// </summary>
        public string GenerateNextFileName(string folderPath)
        {
            Logger.Debug($"[MOCK] Generating next file name for folder: {folderPath}");
            int currentMax = FindHighestFileNumber(folderPath);
            int nextNumber = currentMax + 1;
            string fileName = $"{Constants.FileNamePrefix}{nextNumber.ToString(Constants.FileNameFormat)}";
            Logger.Debug($"[MOCK] Generated file name: {fileName} (next number: {nextNumber})");
            return fileName;
        }

        /// <summary>
        /// Finds the highest numbered file in the directory
        /// </summary>
        private int FindHighestFileNumber(string folderPath)
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
        /// Simulates opening a template file (returns null in mock mode)
        /// </summary>
        public Document OpenTemplate(string templateFilePath)
        {
            Logger.Info($"[MOCK] Simulating opening template: {Path.GetFileName(templateFilePath)}");

            if (!File.Exists(templateFilePath))
            {
                Logger.Error($"[MOCK] Template file not found: {templateFilePath}");
                return null;
            }

            Logger.Info($"[MOCK] Template file exists, simulating successful open");
            // In mock mode, we return null since we can't create actual Document objects
            // The calling code must handle null documents gracefully
            return null;
        }

        /// <summary>
        /// Simulates updating a custom property in a DraftSight document
        /// </summary>
        public bool UpdateCustomProperty(Document dsDoc, string propertyName, string propertyValue, string logFilePath = null)
        {
            Logger.Info($"[MOCK] Simulating property update: {propertyName} = {propertyValue}");

            // In mock mode, always succeed
            Logger.Info($"[MOCK] Property '{propertyName}' updated successfully to '{propertyValue}'");
            return true;
        }

        /// <summary>
        /// Simulates closing a DraftSight document
        /// </summary>
        public void CloseDocument(Document dsDoc, string documentFilePath)
        {
            Logger.Info($"[MOCK] Simulating closing document: {Path.GetFileName(documentFilePath)}");
            Logger.Info($"[MOCK] Document closed successfully");
        }

        /// <summary>
        /// Simulates getting a custom property value
        /// </summary>
        public string GetPropertyValue(object customProps, string propertyName)
        {
            Logger.Debug($"[MOCK] Simulating property read: {propertyName}");
            // Return empty string in mock mode
            return string.Empty;
        }

        /// <summary>
        /// Simulates exporting a DWG file to PDF format by creating a dummy PDF file
        /// </summary>
        public bool ExportToPdf(string dwgFilePath, string pdfFilePath)
        {
            Logger.Info($"[MOCK] Simulating PDF export: {Path.GetFileName(dwgFilePath)} -> {Path.GetFileName(pdfFilePath)}");

            try
            {
                // Create a dummy PDF file for testing
                // This is a minimal valid PDF file structure
                string dummyPdfContent = @"%PDF-1.4
1 0 obj
<<
/Type /Catalog
/Pages 2 0 R
>>
endobj
2 0 obj
<<
/Type /Pages
/Kids [3 0 R]
/Count 1
>>
endobj
3 0 obj
<<
/Type /Page
/Parent 2 0 R
/MediaBox [0 0 612 792]
/Contents 4 0 R
/Resources <<
/Font <<
/F1 <<
/Type /Font
/Subtype /Type1
/BaseFont /Helvetica
>>
>>
>>
>>
endobj
4 0 obj
<<
/Length 44
>>
stream
BT
/F1 12 Tf
100 700 Td
(Mock Lamacoid) Tj
ET
endstream
endobj
xref
0 5
0000000000 65535 f
0000000009 00000 n
0000000058 00000 n
0000000115 00000 n
0000000317 00000 n
trailer
<<
/Size 5
/Root 1 0 R
>>
startxref
410
%%EOF";

                File.WriteAllText(pdfFilePath, dummyPdfContent);
                Logger.Info($"[MOCK] Dummy PDF created successfully: {Path.GetFileName(pdfFilePath)}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"[MOCK] Failed to create dummy PDF: {Path.GetFileName(pdfFilePath)}", ex);
                return false;
            }
        }
    }
}
