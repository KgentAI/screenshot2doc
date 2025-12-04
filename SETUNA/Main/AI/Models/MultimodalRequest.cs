using System;
using System.Collections.Generic;
using System.Drawing;

namespace SETUNA.Main.AI.Models
{
    /// <summary>
    /// Request data model for multimodal AI service
    /// </summary>
    public class MultimodalRequest
    {
        /// <summary>
        /// Collection of screenshot images to analyze
        /// </summary>
        public List<Image> Images { get; set; }

        /// <summary>
        /// Analysis instruction prompt
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Maximum tokens in response (default: 2000)
        /// </summary>
        public int MaxTokens { get; set; }

        /// <summary>
        /// Temperature parameter for randomness (default: 0.7)
        /// </summary>
        public float Temperature { get; set; }

        public MultimodalRequest()
        {
            Images = new List<Image>();
            Prompt = string.Empty;
            MaxTokens = 2000;
            Temperature = 0.7f;
        }
    }
}
