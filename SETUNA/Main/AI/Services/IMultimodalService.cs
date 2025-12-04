using System.Threading;
using System.Threading.Tasks;
using SETUNA.Main.AI.Models;

namespace SETUNA.Main.AI.Services
{
    /// <summary>
    /// Interface for multimodal AI service implementations
    /// </summary>
    public interface IMultimodalService
    {
        /// <summary>
        /// Executes AI analysis on provided images
        /// </summary>
        /// <param name="request">Analysis request with images and prompt</param>
        /// <param name="cancellationToken">Cancellation token for operation</param>
        /// <returns>Analysis response with markdown content</returns>
        Task<MultimodalResponse> AnalyzeImagesAsync(MultimodalRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Health check for service availability
        /// </summary>
        /// <returns>True if service is available and ready</returns>
        Task<bool> IsAvailableAsync();

        /// <summary>
        /// Gets the underlying model name
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// Gets the maximum number of images supported in one request
        /// </summary>
        int MaxImageCount { get; }
    }
}
