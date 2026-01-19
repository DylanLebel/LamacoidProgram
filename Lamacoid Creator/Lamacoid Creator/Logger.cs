using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Comprehensive logging system with multiple log levels and automatic log rotation
    /// </summary>
    public static class Logger
    {
        private static readonly object lockObject = new object();
        private static string currentLogFilePath;
        private static long maxLogFileSizeBytes = 10 * 1024 * 1024; // 10 MB default

        public enum LogLevel
        {
            DEBUG,
            INFO,
            WARNING,
            ERROR,
            FATAL
        }

        /// <summary>
        /// Initializes the logger with a specific log directory
        /// </summary>
        public static void Initialize(string logDirectory, long maxFileSizeBytes = 10485760)
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                maxLogFileSizeBytes = maxFileSizeBytes;
                currentLogFilePath = Path.Combine(logDirectory, $"LamCreator_{DateTime.Now:yyyyMMdd}.log");
            }
            catch (Exception ex)
            {
                // Fallback to console if we can't create log directory
                Console.WriteLine($"Failed to initialize logger: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        public static void Debug(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.DEBUG, message, null, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public static void Info(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.INFO, message, null, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public static void Warning(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.WARNING, message, null, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Logs an error message with optional exception
        /// </summary>
        public static void Error(string message, Exception ex = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.ERROR, message, ex, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Logs a fatal error message with optional exception
        /// </summary>
        public static void Fatal(string message, Exception ex = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogLevel.FATAL, message, ex, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Logs performance metrics for an operation
        /// </summary>
        public static void LogPerformance(string operationName, Stopwatch stopwatch,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            string message = $"Performance: {operationName} completed in {stopwatch.ElapsedMilliseconds}ms";
            Log(LogLevel.INFO, message, null, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Core logging method
        /// </summary>
        private static void Log(LogLevel level, string message, Exception ex,
            string memberName, string sourceFilePath, int sourceLineNumber)
        {
            lock (lockObject)
            {
                try
                {
                    // Check if log rotation is needed
                    if (File.Exists(currentLogFilePath))
                    {
                        FileInfo fileInfo = new FileInfo(currentLogFilePath);
                        if (fileInfo.Length > maxLogFileSizeBytes)
                        {
                            RotateLogFile();
                        }
                    }

                    // Build log entry
                    StringBuilder logEntry = new StringBuilder();

                    // Timestamp and level
                    logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level,-7}] {message}");

                    // Source information
                    string fileName = Path.GetFileName(sourceFilePath);
                    logEntry.AppendLine($"    Source: {fileName} -> {memberName}() [Line {sourceLineNumber}]");

                    // Exception details if present
                    if (ex != null)
                    {
                        logEntry.AppendLine($"    Exception Type: {ex.GetType().FullName}");
                        logEntry.AppendLine($"    Exception Message: {ex.Message}");
                        logEntry.AppendLine($"    Stack Trace:");

                        // Format stack trace with indentation
                        string[] stackTraceLines = ex.StackTrace?.Split(new[] { Environment.NewLine }, StringSplitOptions.None) ?? new string[0];
                        foreach (string line in stackTraceLines)
                        {
                            logEntry.AppendLine($"        {line.Trim()}");
                        }

                        // Log inner exceptions
                        if (ex.InnerException != null)
                        {
                            logEntry.AppendLine($"    Inner Exception: {ex.InnerException.GetType().FullName}");
                            logEntry.AppendLine($"    Inner Message: {ex.InnerException.Message}");

                            string[] innerStackTraceLines = ex.InnerException.StackTrace?.Split(new[] { Environment.NewLine }, StringSplitOptions.None) ?? new string[0];
                            foreach (string line in innerStackTraceLines)
                            {
                                logEntry.AppendLine($"        {line.Trim()}");
                            }
                        }
                    }

                    logEntry.AppendLine(); // Empty line for readability

                    // Write to file
                    if (!string.IsNullOrEmpty(currentLogFilePath))
                    {
                        File.AppendAllText(currentLogFilePath, logEntry.ToString());
                    }

                    // Also write to console for immediate visibility
                    Console.Write(logEntry.ToString());

                    // Write errors and fatal messages to debug output
                    if (level == LogLevel.ERROR || level == LogLevel.FATAL)
                    {
                        System.Diagnostics.Debug.Write(logEntry.ToString());
                    }
                }
                catch (Exception logEx)
                {
                    // If logging fails, at least try to write to console
                    Console.WriteLine($"LOGGER FAILURE: {logEx.Message}");
                    Console.WriteLine($"Original message: [{level}] {message}");
                    if (ex != null)
                    {
                        Console.WriteLine($"Original exception: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Rotates the log file when it exceeds the maximum size
        /// </summary>
        private static void RotateLogFile()
        {
            try
            {
                string directory = Path.GetDirectoryName(currentLogFilePath);
                string baseFileName = Path.GetFileNameWithoutExtension(currentLogFilePath);
                string extension = Path.GetExtension(currentLogFilePath);

                // Create archive file name with timestamp
                string archiveFileName = $"{baseFileName}_{DateTime.Now:HHmmss}{extension}";
                string archiveFilePath = Path.Combine(directory, archiveFileName);

                // Rename current log file to archive
                if (File.Exists(currentLogFilePath))
                {
                    File.Move(currentLogFilePath, archiveFilePath);
                }

                // Clean up old log files (keep only last 10 archives)
                CleanOldLogFiles(directory, baseFileName, extension);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to rotate log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleans up old log files, keeping only the most recent ones
        /// </summary>
        private static void CleanOldLogFiles(string directory, string baseFileName, string extension, int keepCount = 10)
        {
            try
            {
                var logFiles = Directory.GetFiles(directory, $"{baseFileName}_*{extension}");
                if (logFiles.Length > keepCount)
                {
                    // Sort by creation time and delete oldest
                    Array.Sort(logFiles, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

                    int filesToDelete = logFiles.Length - keepCount;
                    for (int i = 0; i < filesToDelete; i++)
                    {
                        File.Delete(logFiles[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to clean old log files: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current log file path
        /// </summary>
        public static string GetCurrentLogFilePath()
        {
            return currentLogFilePath;
        }

        /// <summary>
        /// Logs system information for diagnostics
        /// </summary>
        public static void LogSystemInfo()
        {
            try
            {
                Info("=== System Information ===");
                Info($"Operating System: {Environment.OSVersion}");
                Info($"OS Version: {Environment.OSVersion.Version}");
                Info($"64-bit OS: {Environment.Is64BitOperatingSystem}");
                Info($"64-bit Process: {Environment.Is64BitProcess}");
                Info($"Machine Name: {Environment.MachineName}");
                Info($"User Name: {Environment.UserName}");
                Info($"CLR Version: {Environment.Version}");
                Info($"Working Directory: {Environment.CurrentDirectory}");
                Info($"System Directory: {Environment.SystemDirectory}");
                Info("=========================");
            }
            catch (Exception ex)
            {
                Error("Failed to log system information", ex);
            }
        }
    }
}
