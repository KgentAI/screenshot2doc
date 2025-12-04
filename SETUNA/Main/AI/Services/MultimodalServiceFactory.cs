using System;
using System.Threading.Tasks;
using SETUNA.Main.AI.Exceptions;
using SETUNA.Main.Option;
using AISummaryConfig = SETUNA.Main.Option.SetunaOption.AISummaryConfig;

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

            // Determine service type based on engine type
            var engineType = config.EngineType?.ToLowerInvariant();
            
            if (engineType == "local")
            {
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
            }
            else if (engineType == "cloud")
            {
                if (string.IsNullOrWhiteSpace(config.ApiKey))
                {
                    throw new AIServiceConfigurationException("API key is not configured for cloud service");
                }

                if (config.ApiKey.Length < 8)
                {
                    throw new AIServiceConfigurationException("API key appears to be invalid (too short)");
                }

                service = new QwenVLService(config.ApiKey, config.TimeoutSeconds);
            }
            else
            {
                throw new AIServiceConfigurationException(
                    $"Unknown AI engine type: {config.EngineType}. " +
                    "Supported engine types are 'local' and 'cloud'"
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
