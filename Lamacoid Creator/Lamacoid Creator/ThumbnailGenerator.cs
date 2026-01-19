using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
// using PdfiumViewer; // Commented out - causing pdfium.dll dependency issues

namespace Lamacoid_Creator
{
    /// <summary>
    /// Generates and caches thumbnail images from PDF files
    /// </summary>
    public static class ThumbnailGenerator
    {
        private const int ThumbnailWidth = 200;
        private const int ThumbnailHeight = 150;
        private const int DpiResolution = 96;

        /// <summary>
        /// Gets the thumbnail directory path, creating it if needed
        /// </summary>
        public static string GetThumbnailDirectory()
        {
            string thumbnailDir = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "Thumbnails");

            if (!Directory.Exists(thumbnailDir))
            {
                Directory.CreateDirectory(thumbnailDir);
                Logger.Info($"Created thumbnail directory: {thumbnailDir}");
            }

            return thumbnailDir;
        }

        /// <summary>
        /// Generates a thumbnail for a PDF file, or returns existing cached thumbnail
        /// </summary>
        /// <param name="pdfPath">Path to the PDF file</param>
        /// <param name="lamacoidNumber">Lamacoid number for naming the thumbnail</param>
        /// <returns>Path to the thumbnail image, or null if generation failed</returns>
        public static string GenerateThumbnail(string pdfPath, int lamacoidNumber)
        {
            try
            {
                if (!File.Exists(pdfPath))
                {
                    Logger.Warning($"PDF not found for thumbnail generation: {pdfPath}");
                    return GeneratePlaceholderThumbnail();
                }

                // Check if thumbnail already exists and is newer than PDF
                string thumbnailPath = GetThumbnailPath(lamacoidNumber);
                if (File.Exists(thumbnailPath))
                {
                    DateTime pdfModified = File.GetLastWriteTime(pdfPath);
                    DateTime thumbnailModified = File.GetLastWriteTime(thumbnailPath);

                    if (thumbnailModified >= pdfModified)
                    {
                        // Cached thumbnail is up to date
                        return thumbnailPath;
                    }
                }

                // Generate new thumbnail using iTextSharp
                Logger.Debug($"Generating thumbnail for: {pdfPath}");

                using (PdfReader reader = new PdfReader(pdfPath))
                {
                    if (reader.NumberOfPages == 0)
                    {
                        Logger.Warning($"PDF has no pages: {pdfPath}");
                        return GeneratePlaceholderThumbnail();
                    }

                    // Render first page to image using iTextSharp
                    Image thumbnail = RenderPdfPageToImage(reader, 1, ThumbnailWidth, ThumbnailHeight);
                    if (thumbnail != null)
                    {
                        thumbnail.Save(thumbnailPath, ImageFormat.Png);
                        thumbnail.Dispose();
                        Logger.Info($"Thumbnail generated: {thumbnailPath}");
                        return thumbnailPath;
                    }
                    else
                    {
                        Logger.Warning($"Failed to render PDF page: {pdfPath}");
                        return GenerateNumberedPlaceholder(lamacoidNumber, Path.GetFileNameWithoutExtension(pdfPath));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to generate thumbnail for {pdfPath}", ex);
                return GeneratePlaceholderThumbnail();
            }
        }

        /// <summary>
        /// Gets the path where a thumbnail should be stored
        /// </summary>
        public static string GetThumbnailPath(int lamacoidNumber)
        {
            string thumbnailDir = GetThumbnailDirectory();
            return Path.Combine(thumbnailDir, $"{Constants.FileNamePrefix}{lamacoidNumber:D3}.png");
        }

        /// <summary>
        /// Generates a placeholder thumbnail for missing PDFs
        /// </summary>
        public static string GeneratePlaceholderThumbnail()
        {
            try
            {
                string placeholderPath = Path.Combine(GetThumbnailDirectory(), "placeholder.png");

                if (File.Exists(placeholderPath))
                    return placeholderPath;

                // Create a simple placeholder image
                using (Bitmap bmp = new Bitmap(ThumbnailWidth, ThumbnailHeight))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.LightGray);

                    using (Font font = new Font("Arial", 12, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.DarkGray))
                    {
                        StringFormat format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString("No Preview", font, brush,
                            new RectangleF(0, 0, ThumbnailWidth, ThumbnailHeight), format);
                    }

                    bmp.Save(placeholderPath, ImageFormat.Png);
                }

                return placeholderPath;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to generate placeholder thumbnail", ex);
                return null;
            }
        }

        /// <summary>
        /// Renders a PDF page to an image using iTextSharp
        /// </summary>
        private static Image RenderPdfPageToImage(PdfReader reader, int pageNumber, int width, int height)
        {
            try
            {
                // Get page size
                var pageSize = reader.GetPageSize(pageNumber);
                float pdfWidth = pageSize.Width;
                float pdfHeight = pageSize.Height;

                // Calculate scale to fit in thumbnail
                float scaleX = width / pdfWidth;
                float scaleY = height / pdfHeight;
                float scale = Math.Min(scaleX, scaleY);

                int scaledWidth = (int)(pdfWidth * scale);
                int scaledHeight = (int)(pdfHeight * scale);

                // Create bitmap
                Bitmap bitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Fill background white
                    g.Clear(Color.White);

                    // Set high quality rendering
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    // Center the PDF in the thumbnail
                    int offsetX = (width - scaledWidth) / 2;
                    int offsetY = (height - scaledHeight) / 2;

                    // Extract text and images from PDF
                    string extractedText = PdfTextExtractor.GetTextFromPage(reader, pageNumber);

                    // Create a simple visual representation
                    // Draw a border to represent the page
                    using (Pen borderPen = new Pen(Color.Black, 1))
                    {
                        g.DrawRectangle(borderPen, offsetX, offsetY, scaledWidth - 1, scaledHeight - 1);
                    }

                    // Draw extracted text (simplified rendering)
                    if (!string.IsNullOrWhiteSpace(extractedText))
                    {
                        using (Font font = new Font("Arial", 8))
                        using (Brush textBrush = new SolidBrush(Color.Black))
                        {
                            // Draw text in the center
                            StringFormat format = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center,
                                Trimming = StringTrimming.EllipsisWord
                            };

                            // Limit text to first 200 characters
                            string displayText = extractedText.Length > 200
                                ? extractedText.Substring(0, 200) + "..."
                                : extractedText;

                            RectangleF textRect = new RectangleF(
                                offsetX + 5,
                                offsetY + 5,
                                scaledWidth - 10,
                                scaledHeight - 10);

                            g.DrawString(displayText, font, textBrush, textRect, format);
                        }
                    }
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to render PDF page to image", ex);
                return null;
            }
        }

        /// <summary>
        /// Generates a numbered placeholder thumbnail with lamacoid info
        /// </summary>
        private static string GenerateNumberedPlaceholder(int lamacoidNumber, string fileName)
        {
            try
            {
                string thumbnailPath = GetThumbnailPath(lamacoidNumber);

                // Create a placeholder with the lamacoid number
                using (Bitmap bmp = new Bitmap(ThumbnailWidth, ThumbnailHeight))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // Background gradient
                    using (System.Drawing.Drawing2D.LinearGradientBrush brush =
                        new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Rectangle(0, 0, ThumbnailWidth, ThumbnailHeight),
                            Color.FromArgb(240, 240, 240),
                            Color.FromArgb(200, 200, 200),
                            45f))
                    {
                        g.FillRectangle(brush, 0, 0, ThumbnailWidth, ThumbnailHeight);
                    }

                    // Draw border
                    using (Pen pen = new Pen(Color.Gray, 2))
                    {
                        g.DrawRectangle(pen, 1, 1, ThumbnailWidth - 2, ThumbnailHeight - 2);
                    }

                    // Draw file number
                    using (Font font = new Font("Arial", 14, FontStyle.Bold))
                    using (Brush textBrush = new SolidBrush(Color.DarkSlateGray))
                    {
                        StringFormat format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(fileName, font, textBrush,
                            new RectangleF(0, 0, ThumbnailWidth, ThumbnailHeight), format);
                    }

                    // Draw "Preview" label at bottom
                    using (Font smallFont = new Font("Arial", 8))
                    using (Brush smallBrush = new SolidBrush(Color.Gray))
                    {
                        g.DrawString("(Preview unavailable)", smallFont, smallBrush, 5, ThumbnailHeight - 20);
                    }

                    bmp.Save(thumbnailPath, ImageFormat.Png);
                }

                return thumbnailPath;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to generate numbered placeholder for {fileName}", ex);
                return GeneratePlaceholderThumbnail();
            }
        }

        /// <summary>
        /// Clears all cached thumbnails
        /// </summary>
        public static void ClearThumbnailCache()
        {
            try
            {
                string thumbnailDir = GetThumbnailDirectory();
                if (Directory.Exists(thumbnailDir))
                {
                    foreach (string file in Directory.GetFiles(thumbnailDir, "*.png"))
                    {
                        File.Delete(file);
                    }
                    Logger.Info("Thumbnail cache cleared");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to clear thumbnail cache", ex);
            }
        }
    }
}
