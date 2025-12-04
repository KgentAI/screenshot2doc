using System;

namespace SETUNA.Main.AI.Models
{
    /// <summary>
    /// Response data model from multimodal AI service
    /// </summary>
    public class MultimodalResponse
    {
        /// <summary>
        /// Generated markdown summary content
        /// </summary>
        public string MarkdownContent { get; set; }

        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if Success is false (null when successful)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Processing time in milliseconds
        /// </summary>
        public long ProcessingTimeMs { get; set; }

        public MultimodalResponse()
        {
            MarkdownContent = string.Empty;
            Success = false;
            ErrorMessage = null;
            ProcessingTimeMs = 0;
        }

        public static MultimodalResponse CreateSuccess(string markdownContent, long processingTimeMs)
        {
            return new MultimodalResponse
            {
                MarkdownContent = markdownContent,
                Success = true,
                ErrorMessage = null,
                ProcessingTimeMs = processingTimeMs
            };
        }

        public static MultimodalResponse CreateError(string errorMessage)
        {
            return new MultimodalResponse
            {
                MarkdownContent = string.Empty,
                Success = false,
                ErrorMessage = errorMessage,
                ProcessingTimeMs = 0
            };
        }
    }
}
