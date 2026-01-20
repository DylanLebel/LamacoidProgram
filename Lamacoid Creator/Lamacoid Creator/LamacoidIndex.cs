using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Manages the index of all lamacoids in the library
    /// Handles scanning, searching, and caching
    /// </summary>
    public class LamacoidIndex
    {
        private readonly IDraftSightHelper draftSightHelper;
        private List<LamacoidItem> allLamacoids = new List<LamacoidItem>();
        private bool isIndexed = false;

        public event EventHandler<IndexProgressEventArgs> ProgressChanged;
        public event EventHandler IndexCompleted;

        /// <summary>
        /// Constructor - initializes DraftSight helper based on configuration
        /// </summary>
        public LamacoidIndex()
        {
            draftSightHelper = DraftSightHelperFactory.Create();
        }

        /// <summary>
        /// Gets all lamacoids in the index
        /// </summary>
        public List<LamacoidItem> AllLamacoids => new List<LamacoidItem>(allLamacoids);

        /// <summary>
        /// Gets whether the index has been built
        /// </summary>
        public bool IsIndexed => isIndexed;

        /// <summary>
        /// Gets the total count of lamacoids
        /// </summary>
        public int Count => allLamacoids.Count;

        /// <summary>
        /// Builds the index by scanning the lamacoid directory
        /// </summary>
        public void BuildIndex()
        {
            Stopwatch timer = Stopwatch.StartNew();
            Logger.Info("=== Starting Lamacoid Index Build ===");

            allLamacoids.Clear();
            isIndexed = false;

            try
            {
                if (!Directory.Exists(Constants.LamacoidDirectory))
                {
                    Logger.Error($"Lamacoid directory not found: {Constants.LamacoidDirectory}");
                    return;
                }

                // Find all DWG files
                string[] dwgFiles = Directory.GetFiles(Constants.LamacoidDirectory, $"{Constants.FileNamePrefix}*{Constants.DwgExtension}");
                Logger.Info($"Found {dwgFiles.Length} DWG files to index");

                int processedCount = 0;

                foreach (string dwgPath in dwgFiles)
                {
                    try
                    {
                        LamacoidItem item = ProcessLamacoidFile(dwgPath);
                        if (item != null)
                        {
                            allLamacoids.Add(item);
                        }

                        processedCount++;
                        OnProgressChanged(processedCount, dwgFiles.Length, Path.GetFileName(dwgPath));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error processing file {dwgPath}", ex);
                    }
                }

                // Sort by number
                allLamacoids = allLamacoids.OrderBy(x => x.Number).ToList();

                isIndexed = true;
                timer.Stop();
                Logger.LogPerformance("Index Build", timer);
                Logger.Info($"Index build complete: {allLamacoids.Count} lamacoids indexed");

                OnIndexCompleted();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to build lamacoid index", ex);
            }
        }

        /// <summary>
        /// Processes a single lamacoid file and extracts its data
        /// </summary>
        private LamacoidItem ProcessLamacoidFile(string dwgPath)
        {
            try
            {
                // Extract number from filename (e.g., E-LAM-123.dwg -> 123)
                string fileName = Path.GetFileNameWithoutExtension(dwgPath);
                Match match = Regex.Match(fileName, $@"{Constants.FileNamePrefix}(\d+)");

                if (!match.Success)
                {
                    Logger.Warning($"Could not parse lamacoid number from: {fileName}");
                    return null;
                }

                int number = int.Parse(match.Groups[1].Value);

                LamacoidItem item = new LamacoidItem
                {
                    Number = number,
                    FileName = fileName,
                    DwgPath = dwgPath,
                    DwgExists = File.Exists(dwgPath),
                    CreatedDate = File.GetCreationTime(dwgPath),
                    ModifiedDate = File.GetLastWriteTime(dwgPath)
                };

                // Check for PDF
                string pdfPath = dwgPath.Replace(Constants.DwgExtension, Constants.PdfExtension);
                item.PdfPath = pdfPath;
                item.PdfExists = File.Exists(pdfPath);

                // Try to extract properties from DWG
                ExtractPropertiesFromDwg(item, dwgPath);

                // Generate thumbnail if PDF exists
                if (item.PdfExists)
                {
                    item.ThumbnailPath = ThumbnailGenerator.GenerateThumbnail(pdfPath, number);
                }

                if (string.IsNullOrEmpty(item.ThumbnailPath))
                {
                    item.ThumbnailPath = ThumbnailGenerator.GeneratePlaceholderThumbnail();
                }

                return item;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error processing lamacoid file: {dwgPath}", ex);
                return null;
            }
        }

        /// <summary>
        /// Extracts custom properties from a DWG file
        /// </summary>
        private void ExtractPropertiesFromDwg(LamacoidItem item, string dwgPath)
        {
            try
            {
                // In test mode, skip property extraction since DraftSight isn't available
                if (Constants.UseTestMode)
                {
                    Logger.Debug($"[TEST MODE] Skipping property extraction for {item.FileName}");
                    return;
                }

                // Try to open the DWG and read properties
                var doc = draftSightHelper.OpenTemplate(dwgPath);
                if (doc != null)
                {
                    try
                    {
                        // Cast to DraftSight Document type to access properties
                        #if !MOCK_MODE
                        var dsDoc = doc as DraftSight.Interop.dsAutomation.Document;
                        if (dsDoc != null)
                        {
                            var props = dsDoc.GetDrawingProperties();

                            // Read properties directly using GetCustomProperty
                            item.Description = GetPropertySafe(props, "DESCRIPTION");
                            item.Contents = GetPropertySafe(props, "CONTENTS");
                            item.Colour = GetPropertySafe(props, "COLOUR");
                            item.Adhesive = GetPropertySafe(props, "ADHESIVE");
                            item.Finish = GetPropertySafe(props, "FINISH");
                            item.Thickness = GetPropertySafe(props, "THICKNESS");

                            Logger.Debug($"Extracted properties from {item.FileName}: Contents='{item.Contents}', Colour='{item.Colour}'");
                        }
                        #endif
                    }
                    finally
                    {
                        draftSightHelper.CloseDocument(doc, dwgPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Could not extract properties from {dwgPath}: {ex.Message}");
                // Continue without properties - better to have the item in the index
            }
        }

        /// <summary>
        /// Safely gets a custom property value from DrawingProperties
        /// </summary>
        private string GetPropertySafe(DraftSight.Interop.dsAutomation.DrawingProperties props, string propertyName)
        {
            try
            {
                if (props == null)
                    return string.Empty;

                if (props.HasCustomProperty(propertyName))
                {
                    string value = props.GetCustomProperty(propertyName);
                    return value ?? string.Empty;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Searches lamacoids by text content
        /// </summary>
        public List<LamacoidItem> Search(string searchTerm)
        {
            if (!isIndexed)
            {
                Logger.Warning("Attempted to search before index was built");
                return new List<LamacoidItem>();
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return AllLamacoids;
            }

            return allLamacoids.Where(x => x.MatchesSearch(searchTerm)).ToList();
        }

        /// <summary>
        /// Finds lamacoids by exact contents match
        /// </summary>
        public List<LamacoidItem> FindByContents(string contents)
        {
            if (!isIndexed || string.IsNullOrWhiteSpace(contents))
                return new List<LamacoidItem>();

            return allLamacoids
                .Where(x => x.Contents != null && x.Contents.Equals(contents, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Finds lamacoids by colour
        /// </summary>
        public List<LamacoidItem> FindByColour(string colour)
        {
            if (!isIndexed || string.IsNullOrWhiteSpace(colour))
                return new List<LamacoidItem>();

            return allLamacoids
                .Where(x => x.Colour != null && x.Colour.IndexOf(colour, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        /// <summary>
        /// Gets the next available lamacoid number
        /// </summary>
        public int GetNextAvailableNumber()
        {
            if (!isIndexed || allLamacoids.Count == 0)
                return 1;

            return allLamacoids.Max(x => x.Number) + 1;
        }

        /// <summary>
        /// Refreshes a single lamacoid in the index
        /// </summary>
        public void RefreshLamacoid(int number)
        {
            try
            {
                string dwgPath = Path.Combine(Constants.LamacoidDirectory, $"{Constants.FileNamePrefix}{number:D3}{Constants.DwgExtension}");

                if (!File.Exists(dwgPath))
                {
                    // Remove from index if file no longer exists
                    allLamacoids.RemoveAll(x => x.Number == number);
                    return;
                }

                // Remove existing entry
                allLamacoids.RemoveAll(x => x.Number == number);

                // Add updated entry
                LamacoidItem item = ProcessLamacoidFile(dwgPath);
                if (item != null)
                {
                    allLamacoids.Add(item);
                    allLamacoids = allLamacoids.OrderBy(x => x.Number).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to refresh lamacoid {number}", ex);
            }
        }

        protected virtual void OnProgressChanged(int current, int total, string fileName)
        {
            ProgressChanged?.Invoke(this, new IndexProgressEventArgs
            {
                Current = current,
                Total = total,
                CurrentFileName = fileName,
                PercentComplete = total > 0 ? (int)((current / (double)total) * 100) : 0
            });
        }

        protected virtual void OnIndexCompleted()
        {
            IndexCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Event args for index progress updates
    /// </summary>
    public class IndexProgressEventArgs : EventArgs
    {
        public int Current { get; set; }
        public int Total { get; set; }
        public string CurrentFileName { get; set; }
        public int PercentComplete { get; set; }
    }
}
