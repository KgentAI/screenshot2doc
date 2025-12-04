using System;

namespace SETUNA.Main.AI.Models
{
    /// <summary>
    /// Metadata for AI summary export
    /// </summary>
    public class AISummaryMetadata
    {
        /// <summary>
        /// Summary generation timestamp
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// AI model name used for generation
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Number of screenshots analyzed
        /// </summary>
        public int ImageCount { get; set; }

        /// <summary>
        /// Total processing time
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }

        public AISummaryMetadata()
        {
            GeneratedAt = DateTime.Now;
            ModelName = string.Empty;
            ImageCount = 0;
            ProcessingTime = TimeSpan.Zero;
        }
    }
}
