using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace SETUNA.Main.AI.Export
{
    /// <summary>
    /// Exports markdown tables to Excel files
    /// </summary>
    public static class ExcelExporter
    {
        static ExcelExporter()
        {
            // Set EPPlus license context for non-commercial use
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Exports markdown tables to Excel file
        /// </summary>
        /// <param name="markdownContent">Markdown content containing tables</param>
        /// <param name="filePath">Target Excel file path</param>
        /// <param name="errorMessage">Error message if export fails</param>
        /// <returns>True if export successful, false otherwise</returns>
        public static bool ExportTablesToExcel(string markdownContent, string filePath, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrEmpty(markdownContent))
            {
                errorMessage = "Markdown content cannot be null or empty";
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                errorMessage = "File path cannot be null or empty";
                return false;
            }

            try
            {
                var tables = ParseMarkdownTables(markdownContent);

                if (tables == null || tables.Count == 0)
                {
                    errorMessage = "No tables detected in summary.";
                    return false;
                }

                using (var package = new ExcelPackage())
                {
                    int tableIndex = 1;
                    foreach (var table in tables)
                    {
                        var worksheet = package.Workbook.Worksheets.Add($"Table {tableIndex}");
                        WriteTableToWorksheet(worksheet, table);
                        tableIndex++;
                    }

                    var fileInfo = new FileInfo(filePath);
                    package.SaveAs(fileInfo);
                }

                return true;
            }
            catch (IOException ex)
            {
                errorMessage = $"Unable to write file: {ex.Message}";
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                errorMessage = $"Access denied: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Unexpected error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Parses markdown content to extract tables
        /// </summary>
        private static List<MarkdownTable> ParseMarkdownTables(string markdownContent)
        {
            var tables = new List<MarkdownTable>();
            var lines = markdownContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Check if line starts a table (contains |)
                if (!line.Contains("|"))
                    continue;

                // Check if next line is separator row
                if (i + 1 >= lines.Length)
                    continue;

                var nextLine = lines[i + 1].Trim();
                if (!IsSeparatorRow(nextLine))
                    continue;

                // Parse table
                var table = new MarkdownTable();
                table.Headers = ParseTableRow(line);

                // Skip separator row
                i += 2;

                // Parse data rows
                while (i < lines.Length)
                {
                    var dataLine = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(dataLine) || !dataLine.Contains("|"))
                        break;

                    var row = ParseTableRow(dataLine);
                    if (row.Count > 0)
                    {
                        table.Rows.Add(row);
                    }
                    i++;
                }

                if (table.Headers.Count > 0)
                {
                    tables.Add(table);
                }

                i--; // Adjust for outer loop increment
            }

            return tables;
        }

        /// <summary>
        /// Checks if a line is a table separator row (contains dashes and pipes)
        /// </summary>
        private static bool IsSeparatorRow(string line)
        {
            if (!line.Contains("|"))
                return false;

            // Remove pipes and trim
            var content = line.Replace("|", "").Trim();
            
            // Should contain only dashes, spaces, and colons
            return Regex.IsMatch(content, @"^[\s\-:]+$");
        }

        /// <summary>
        /// Parses a table row into individual cells
        /// </summary>
        private static List<string> ParseTableRow(string line)
        {
            var cells = new List<string>();
            
            // Split by pipe, trim start/end pipes
            var parts = line.Split('|');
            
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed) || parts.Length > 2)
                {
                    cells.Add(trimmed);
                }
            }

            // Remove first and last if they're empty (from leading/trailing pipes)
            if (cells.Count > 0 && string.IsNullOrEmpty(cells[0]))
                cells.RemoveAt(0);
            if (cells.Count > 0 && string.IsNullOrEmpty(cells[cells.Count - 1]))
                cells.RemoveAt(cells.Count - 1);

            return cells;
        }

        /// <summary>
        /// Writes a table to an Excel worksheet
        /// </summary>
        private static void WriteTableToWorksheet(ExcelWorksheet worksheet, MarkdownTable table)
        {
            int row = 1;
            int col = 1;

            // Write headers
            foreach (var header in table.Headers)
            {
                var cell = worksheet.Cells[row, col];
                cell.Value = header;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                col++;
            }

            // Write data rows
            row++;
            foreach (var dataRow in table.Rows)
            {
                col = 1;
                foreach (var cellValue in dataRow)
                {
                    var cell = worksheet.Cells[row, col];
                    cell.Value = cellValue;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    col++;
                }
                row++;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Freeze header row
            worksheet.View.FreezePanes(2, 1);
        }

        /// <summary>
        /// Generates default Excel export filename
        /// </summary>
        public static string GetDefaultFilename()
        {
            return $"Summary_Tables_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        }

        /// <summary>
        /// Internal class to represent a parsed markdown table
        /// </summary>
        private class MarkdownTable
        {
            public List<string> Headers { get; set; } = new List<string>();
            public List<List<string>> Rows { get; set; } = new List<List<string>>();
        }
    }
}
