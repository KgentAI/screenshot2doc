using System;

namespace SETUNA.Main.Option
{
    /// <summary>
    /// Configuration for AI Screenshot Summary feature
    /// </summary>
    public class AISummaryConfig : ICloneable
    {
        /// <summary>
        /// Enable/disable AI summary feature
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// AI engine selection: "minicpm-v4.5" or "qwen3-vl-flash"
        /// </summary>
        public string Engine { get; set; }

        /// <summary>
        /// Local inference server endpoint URL
        /// </summary>
        public string LocalEndpoint { get; set; }

        /// <summary>
        /// API key for cloud services
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Maximum number of images to process at once (1-10)
        /// </summary>
        public int MaxImages { get; set; }

        /// <summary>
        /// Custom prompt template for AI analysis
        /// </summary>
        public string PromptTemplate { get; set; }

        /// <summary>
        /// Use WebView2 for enhanced markdown rendering
        /// </summary>
        public bool UseWebView2 { get; set; }

        /// <summary>
        /// Request timeout in seconds (10-120)
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// User has consented to data transmission
        /// </summary>
        public bool ConsentGiven { get; set; }

        public AISummaryConfig()
        {
            Enabled = false;
            Engine = "minicpm-v4.5";
            LocalEndpoint = "http://localhost:8080";
            ApiKey = string.Empty;
            MaxImages = 5;
            PromptTemplate = GetDefaultPrompt();
            UseWebView2 = false;
            TimeoutSeconds = 30;
            ConsentGiven = false;
        }

        public static string GetDefaultPrompt()
        {
            return "Analyze the provided screenshots and generate a comprehensive summary in markdown format. " +
                   "Include: 1) Overview section describing the main content, " +
                   "2) Detailed findings organized with headings and bullet points, " +
                   "3) Tables for any structured data observed, " +
                   "4) Key observations section. " +
                   "Use clear hierarchical structure with H1-H3 headings.";
        }

        public object Clone()
        {
            return new AISummaryConfig
            {
                Enabled = this.Enabled,
                Engine = this.Engine,
                LocalEndpoint = this.LocalEndpoint,
                ApiKey = this.ApiKey,
                MaxImages = this.MaxImages,
                PromptTemplate = this.PromptTemplate,
                UseWebView2 = this.UseWebView2,
                TimeoutSeconds = this.TimeoutSeconds,
                ConsentGiven = this.ConsentGiven
            };
        }
    }
}
