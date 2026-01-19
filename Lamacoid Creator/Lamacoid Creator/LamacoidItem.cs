using System;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Represents a single lamacoid in the library
    /// </summary>
    public class LamacoidItem
    {
        public int Number { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string Contents { get; set; }
        public string Colour { get; set; }
        public string Adhesive { get; set; }
        public string Finish { get; set; }
        public string Thickness { get; set; }
        public string DwgPath { get; set; }
        public string PdfPath { get; set; }
        public string ThumbnailPath { get; set; }
        public bool DwgExists { get; set; }
        public bool PdfExists { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public LamacoidItem()
        {
            FileName = string.Empty;
            Description = string.Empty;
            Contents = string.Empty;
            Colour = string.Empty;
            Adhesive = string.Empty;
            Finish = string.Empty;
            Thickness = string.Empty;
            DwgPath = string.Empty;
            PdfPath = string.Empty;
            ThumbnailPath = string.Empty;
        }

        /// <summary>
        /// Returns a display name for the lamacoid
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(Contents))
                    return Contents;
                if (!string.IsNullOrEmpty(Description))
                    return Description;
                return FileName;
            }
        }

        /// <summary>
        /// Checks if this lamacoid matches a search term
        /// </summary>
        public bool MatchesSearch(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return true;

            searchTerm = searchTerm.ToLowerInvariant();

            return (Contents?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                   (Description?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                   (FileName?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                   (Colour?.ToLowerInvariant().Contains(searchTerm) ?? false);
        }

        public override string ToString()
        {
            return $"{FileName} - {DisplayName}";
        }
    }
}
