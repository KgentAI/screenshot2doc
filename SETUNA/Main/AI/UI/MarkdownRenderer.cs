using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SETUNA.Main.AI.UI
{
    /// <summary>
    /// Renders markdown content in a RichTextBox with basic formatting
    /// </summary>
    public static class MarkdownRenderer
    {
        /// <summary>
        /// Renders markdown content to a RichTextBox with formatting
        /// </summary>
        /// <param name="richTextBox">Target RichTextBox control</param>
        /// <param name="markdownContent">Markdown content to render</param>
        public static void Render(RichTextBox richTextBox, string markdownContent)
        {
            if (richTextBox == null)
                throw new ArgumentNullException(nameof(richTextBox));

            if (string.IsNullOrEmpty(markdownContent))
            {
                richTextBox.Clear();
                return;
            }

            richTextBox.Clear();
            richTextBox.SelectionStart = 0;

            var lines = markdownContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            bool inCodeBlock = false;

            foreach (var line in lines)
            {
                // Check for code block markers
                if (line.Trim().StartsWith("```"))
                {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }

                if (inCodeBlock)
                {
                    // Code block formatting
                    RenderCodeBlock(richTextBox, line);
                }
                else if (line.StartsWith("### "))
                {
                    // H3 heading
                    RenderHeading(richTextBox, line.Substring(4), 12, Color.DarkSlateGray);
                }
                else if (line.StartsWith("## "))
                {
                    // H2 heading
                    RenderHeading(richTextBox, line.Substring(3), 14, Color.DarkSlateGray);
                }
                else if (line.StartsWith("# "))
                {
                    // H1 heading
                    RenderHeading(richTextBox, line.Substring(2), 16, Color.Navy);
                }
                else if (line.TrimStart().StartsWith("- ") || line.TrimStart().StartsWith("* "))
                {
                    // List item
                    RenderListItem(richTextBox, line);
                }
                else if (line.Contains("|") && !string.IsNullOrWhiteSpace(line))
                {
                    // Table row
                    RenderTableRow(richTextBox, line);
                }
                else
                {
                    // Regular text with inline formatting
                    RenderText(richTextBox, line);
                }

                richTextBox.AppendText(Environment.NewLine);
            }

            // Scroll to top
            richTextBox.SelectionStart = 0;
            richTextBox.ScrollToCaret();
        }

        private static void RenderHeading(RichTextBox rtb, string text, int fontSize, Color color)
        {
            rtb.SelectionFont = new Font(rtb.Font.FontFamily, fontSize, FontStyle.Bold);
            rtb.SelectionColor = color;
            rtb.AppendText(text);
            rtb.SelectionFont = rtb.Font;
            rtb.SelectionColor = rtb.ForeColor;
        }

        private static void RenderListItem(RichTextBox rtb, string line)
        {
            rtb.SelectionIndent = 20;
            var text = line.TrimStart().Substring(2); // Remove "- " or "* "
            rtb.AppendText("• " + text);
            rtb.SelectionIndent = 0;
        }

        private static void RenderTableRow(RichTextBox rtb, string line)
        {
            rtb.SelectionFont = new Font("Consolas", 10);
            rtb.AppendText(line);
            rtb.SelectionFont = rtb.Font;
        }

        private static void RenderCodeBlock(RichTextBox rtb, string line)
        {
            rtb.SelectionFont = new Font("Consolas", 10);
            rtb.SelectionBackColor = Color.LightGray;
            rtb.AppendText(line);
            rtb.SelectionBackColor = rtb.BackColor;
            rtb.SelectionFont = rtb.Font;
        }

        private static void RenderText(RichTextBox rtb, string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            // Handle bold text **text**
            var boldPattern = @"\*\*(.+?)\*\*";
            var parts = Regex.Split(line, boldPattern);

            bool isBold = false;
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                if (isBold)
                {
                    rtb.SelectionFont = new Font(rtb.Font, FontStyle.Bold);
                    rtb.AppendText(part);
                    rtb.SelectionFont = rtb.Font;
                }
                else
                {
                    rtb.AppendText(part);
                }

                isBold = !isBold;
            }
        }
    }
}
