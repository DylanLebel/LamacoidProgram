using System.Drawing;
using System.IO;
using System.Reflection;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Application constants and configuration values
    /// </summary>
    public static class Constants
    {
        // Test Mode Configuration
        // Set to true to use mock DraftSight helper (no DraftSight required)
        // Set to false to use real DraftSight automation
        public const bool UseTestMode = true;

        // Directory paths
        public const string TemplatesPath = @"Y:\Autocad\Template\~LAMACOIDS\Lams\Templates";
        public const string LamacoidDirectory = @"Y:\Autocad\Template\~LAMACOIDS\Lams";

        // Logging configuration
        public static readonly string LogDirectory = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "Logs");
        public const long MaxLogFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        // Legacy log file path (kept for backwards compatibility)
        public const string LogFilePath = @"Y:\Autocad\Template\~LAMACOIDS\Lams\DebugLog.txt";

        // File naming
        public const string FileNamePrefix = "E-LAM-";
        public const string FileNameFormat = "D3"; // Three-digit format
        public const string DwgExtension = ".dwg";
        public const string PdfExtension = ".pdf";

        // PDF Export settings
        public const int MaxPdfExportAttempts = 100;
        public const int PdfExportDelayMs = 5000;
        public const string PdfPrinterName = "PDF";
        public const string PdfPaperSize = "11x17 in mm";
        public const string PdfLayoutSheet = "Layout3";

        // Property update settings
        public const int MaxPropertyUpdateRetries = 3;
        public const int PropertyUpdateRetryDelayMs = 1000;

        // Custom property names
        public const string DescriptionProperty = "DESCRIPTION";
        public const string ContentsProperty = "CONTENTS";
        public const string DescriptionSuffix = " INDICATOR";

        // UI Settings
        public static readonly Color FormBackColor = Color.LightGray;
        public static readonly Color ButtonPrimaryColor = Color.CornflowerBlue;
        public static readonly Color ButtonSuccessColor = Color.ForestGreen;
        public static readonly Color ButtonDangerColor = Color.Crimson;
        public static readonly Color ButtonTextColor = Color.White;
        public static readonly Font DefaultFont = new Font("Segoe UI", 10);

        // Control sizes and positions (for reference, actual layout in form)
        public const int FormWidth = 450;
        public const int FormHeight = 500;
    }
}
