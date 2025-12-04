using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SETUNA.Main.AI.Exceptions;
using SETUNA.Main.AI.Models;
using SETUNA.Main.AI.Utils;

namespace SETUNA.Main.AI.Services
{
    /// <summary>
    /// Local MiniCPM-V-4.5 model service implementation
    /// </summary>
    public class MiniCPMService : IMultimodalService, IDisposable
    {
        private readonly string _endpoint;
        private readonly int _timeoutSeconds;
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        public string ModelName => "MiniCPM-V-4.5";
        public int MaxImageCount => 10;

        public MiniCPMService(string endpoint, int timeoutSeconds = 30)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

            _endpoint = endpoint.TrimEnd('/');
            _timeoutSeconds = timeoutSeconds;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_timeoutSeconds)
            };
        }

        public async Task<MultimodalResponse> AnalyzeImagesAsync(MultimodalRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Images == null || request.Images.Count == 0)
                return MultimodalResponse.CreateError("No images provided");

            if (request.Images.Count > MaxImageCount)
                return MultimodalResponse.CreateError($"Too many images. Maximum is {MaxImageCount}");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Prepare image data URIs
                var imageDataUris = new List<string>();
                foreach (var image in request.Images)
                {
                    var imageBytes = ImageHelper.CompressImage(image);
                    var dataUri = ImageHelper.ToBase64DataUri(imageBytes);
                    imageDataUris.Add(dataUri);
                }

                // Create request payload
                var requestPayload = new
                {
                    images = imageDataUris,
                    prompt = request.Prompt,
                    max_tokens = request.MaxTokens,
                    temperature = request.Temperature
                };

                var jsonContent = JsonConvert.SerializeObject(requestPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send request to local endpoint
                var analyzeUrl = $"{_endpoint}/analyze";
                var response = await _httpClient.PostAsync(analyzeUrl, content, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return MultimodalResponse.CreateError($"HTTP {(int)response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                stopwatch.Stop();

                // Extract markdown content from response
                string markdownContent = responseObject?.result?.ToString() ?? responseObject?.content?.ToString() ?? responseContent;

                return MultimodalResponse.CreateSuccess(markdownContent, stopwatch.ElapsedMilliseconds);
            }
            catch (TaskCanceledException ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return MultimodalResponse.CreateError("Analysis was cancelled");
                }
                throw new AIServiceTimeoutException($"Request timed out after {_timeoutSeconds} seconds", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new AIServiceNetworkException("Local endpoint unreachable. Please ensure the server is running.", ex);
            }
            catch (JsonException ex)
            {
                throw new AIServiceException("Invalid server response format", ex);
            }
            catch (Exception ex)
            {
                throw new AIServiceException($"Unexpected error during analysis: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                // Send HEAD request to check server availability
                var request = new HttpRequestMessage(HttpMethod.Head, _endpoint);
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                
                // Accept 200 OK or 404 Not Found (both indicate server is responding)
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
