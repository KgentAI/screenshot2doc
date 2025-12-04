using System;
using System.Threading.Tasks;
using SETUNA.Main.AI.Exceptions;
using SETUNA.Main.Option;

namespace SETUNA.Main.AI.Services
{
    /// <summary>
    /// Factory for creating multimodal AI service instances
    /// </summary>
    public static class MultimodalServiceFactory
    {
        /// <summary>
        /// Creates an AI service instance based on configuration
        /// </summary>
        /// <param name="config">AI summary configuration</param>
        /// <returns>Configured service instance</returns>
        /// <exception cref="AIServiceConfigurationException">Thrown when configuration is invalid</exception>
        public static async Task<IMultimodalService> GetServiceAsync(AISummaryConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            IMultimodalService service;

            switch (config.Engine?.ToLowerInvariant())
            {
                case "minicpm-v4.5":
                case "minicpm":
                    if (string.IsNullOrWhiteSpace(config.LocalEndpoint))
                    {
                        throw new AIServiceConfigurationException("Local endpoint is not configured");
                    }

                    service = new MiniCPMService(config.LocalEndpoint, config.TimeoutSeconds);

                    // Validate endpoint availability
                    var isAvailable = await service.IsAvailableAsync().ConfigureAwait(false);
                    if (!isAvailable)
                    {
                        throw new AIServiceConfigurationException(
                            $"Local AI service at {config.LocalEndpoint} is not responding. " +
                            "Please ensure the server is running."
                        );
                    }
                    break;

                case "qwen3-vl-flash":
                case "qwen":
                case "qwen-vl":
                    if (string.IsNullOrWhiteSpace(config.ApiKey))
                    {
                        throw new AIServiceConfigurationException("API key is not configured for cloud service");
                    }

                    if (config.ApiKey.Length < 8)
                    {
                        throw new AIServiceConfigurationException("API key appears to be invalid (too short)");
                    }

                    service = new QwenVLService(config.ApiKey, config.TimeoutSeconds);
                    break;

                default:
                    throw new AIServiceConfigurationException(
                        $"Unknown AI engine: {config.Engine}. " +
                        "Supported engines are 'minicpm-v4.5' and 'qwen3-vl-flash'"
                    );
            }

            return service;
        }

        /// <summary>
        /// Creates an AI service instance based on configuration (synchronous wrapper)
        /// </summary>
        /// <param name="config">AI summary configuration</param>
        /// <returns>Configured service instance</returns>
        public static IMultimodalService GetService(AISummaryConfig config)
        {
            return GetServiceAsync(config).GetAwaiter().GetResult();
        }
    }
}
