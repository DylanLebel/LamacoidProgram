using DraftSight.Interop.dsAutomation;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Interface for DraftSight automation operations.
    /// Allows for mock implementations for testing without DraftSight installed.
    /// </summary>
    public interface IDraftSightHelper
    {
        /// <summary>
        /// Generates the next available file name based on existing files
        /// </summary>
        string GenerateNextFileName(string folderPath);

        /// <summary>
        /// Opens a template file in DraftSight
        /// </summary>
        Document OpenTemplate(string templateFilePath);

        /// <summary>
        /// Updates a custom property in a DraftSight document
        /// </summary>
        bool UpdateCustomProperty(Document dsDoc, string propertyName, string propertyValue, string logFilePath = null);

        /// <summary>
        /// Closes a DraftSight document
        /// </summary>
        void CloseDocument(Document dsDoc, string documentFilePath);

        /// <summary>
        /// Gets a custom property value from drawing properties
        /// </summary>
        string GetPropertyValue(object customProps, string propertyName);

        /// <summary>
        /// Exports a DWG file to PDF format
        /// </summary>
        bool ExportToPdf(string dwgFilePath, string pdfFilePath);
    }
}
