namespace Lamacoid_Creator
{
    /// <summary>
    /// Interface for DraftSight automation operations.
    /// Allows for mock implementations for testing without DraftSight installed.
    /// Uses object type for DraftSight-specific types to avoid hard dependency.
    /// </summary>
    public interface IDraftSightHelper
    {
        /// <summary>
        /// Generates the next available file name based on existing files
        /// </summary>
        string GenerateNextFileName(string folderPath);

        /// <summary>
        /// Opens a template file in DraftSight
        /// Returns: DraftSight Document object (or null in mock mode)
        /// </summary>
        object OpenTemplate(string templateFilePath);

        /// <summary>
        /// Updates a custom property in a DraftSight document
        /// </summary>
        /// <param name="dsDoc">DraftSight Document object (or null in mock mode)</param>
        /// <param name="propertyName">Name of the property to update</param>
        /// <param name="propertyValue">New value for the property</param>
        /// <param name="logFilePath">Optional log file path</param>
        bool UpdateCustomProperty(object dsDoc, string propertyName, string propertyValue, string logFilePath = null);

        /// <summary>
        /// Closes a DraftSight document
        /// </summary>
        /// <param name="dsDoc">DraftSight Document object (or null in mock mode)</param>
        /// <param name="documentFilePath">Path to the document file</param>
        void CloseDocument(object dsDoc, string documentFilePath);

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
