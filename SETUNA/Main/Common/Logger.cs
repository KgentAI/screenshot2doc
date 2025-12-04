using System;
using System.IO;
using System.Text;

namespace SETUNA.Main.Common
{
    /// <summary>
    /// Simple file logger for debugging
    /// </summary>
    public static class Logger
    {
        private static readonly object _lockObj = new object();
        private static string _logFilePath;

        static Logger()
        {
            try
            {
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var exeDir = Path.GetDirectoryName(exePath);
                _logFilePath = Path.Combine(exeDir, "run.log");
            }
            catch
            {
                _logFilePath = "run.log";
            }
        }

        /// <summary>
        /// Write a log message to the log file
        /// </summary>
        public static void Log(string message)
        {
#if DEBUG
            try
            {
                lock (_lockObj)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logMessage = $"[{timestamp}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, logMessage, Encoding.UTF8);
                }
            }
            catch
            {
                // Silently ignore logging errors
            }
#endif
        }

        /// <summary>
        /// Write a log message with exception details
        /// </summary>
        public static void LogError(string message, Exception ex)
        {
#if DEBUG
            try
            {
                var fullMessage = $"{message}: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}";
                Log($"ERROR: {fullMessage}");
            }
            catch
            {
                // Silently ignore logging errors
            }
#endif
        }
    }
}
