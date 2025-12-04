using System;
using System.IO;
using System.Text;
using SETUNA.Main.AI.Models;

namespace SETUNA.Main.AI.Export
{
    /// <summary>
    /// Exports AI summary content to markdown files
    /// </summary>
    public static class MarkdownExporter
    {
        /// <summary>
        /// Exports markdown content to file with metadata header
        /// </summary>
        /// <param name="markdownContent">The markdown content to export</param>
        /// <param name="filePath">Target file path</param>
        /// <param name="metadata">Summary metadata</param>
        /// <returns>True if export successful, false otherwise</returns>
        public static bool ExportToFile(string markdownContent, string filePath, AISummaryMetadata metadata)
        {
            if (string.IsNullOrEmpty(markdownContent))
                throw new ArgumentException("Markdown content cannot be null or empty", nameof(markdownContent));

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            try
            {
                // Create metadata header
                var sb = new StringBuilder();
                sb.AppendLine("---");
                sb.AppendLine($"Generated: {metadata.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Model: {metadata.ModelName}");
                sb.AppendLine($"Images: {metadata.ImageCount}");
                sb.AppendLine($"Processing Time: {metadata.ProcessingTime.TotalSeconds:F2}s");
                sb.AppendLine("---");
                sb.AppendLine();
                sb.Append(markdownContent);

                // Write to file with UTF-8 encoding without BOM
                var utf8WithoutBom = new UTF8Encoding(false);
                File.WriteAllText(filePath, sb.ToString(), utf8WithoutBom);

                return true;
            }
            catch (IOException)
            {
                // Unable to write file (permission or path issues)
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // Access denied
                return false;
            }
            catch (Exception)
            {
                // Other unexpected errors
                return false;
            }
        }

        /// <summary>
        /// Generates default export filename based on current timestamp
        /// </summary>
        /// <returns>Filename in format "Summary_yyyyMMdd_HHmmss.md"</returns>
        public static string GetDefaultFilename()
        {
            return $"Summary_{DateTime.Now:yyyyMMdd_HHmmss}.md";
        }
    }
}
