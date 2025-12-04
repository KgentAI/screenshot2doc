using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
    /// Qwen3-VL-Flash cloud API service implementation
    /// </summary>
    public class QwenVLService : IMultimodalService, IDisposable
    {
        private const string BaseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1";
        private const string ModelId = "qwen3-vl-flash";
        private const int MaxRetries = 3;
        private const int InitialRetryDelayMs = 2000;

        private readonly string _apiKey;
        private readonly int _timeoutSeconds;
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        public string ModelName => "Qwen3-VL-Flash";
        public int MaxImageCount => 10;

        public QwenVLService(string apiKey, int timeoutSeconds = 30)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

            _apiKey = apiKey;
            _timeoutSeconds = timeoutSeconds;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_timeoutSeconds)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
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

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    // Prepare image content items
                    var contentItems = new List<object>();
                    
                    // Add text prompt
                    contentItems.Add(new { type = "text", text = request.Prompt });

                    // Add image URLs (as data URIs)
                    foreach (var image in request.Images)
                    {
                        var imageBytes = ImageHelper.CompressImage(image);
                        var dataUri = ImageHelper.ToBase64DataUri(imageBytes);
                        contentItems.Add(new 
                        { 
                            type = "image_url", 
                            image_url = new { url = dataUri } 
                        });
                    }

                    // Create OpenAI-compatible request payload
                    var requestPayload = new
                    {
                        model = ModelId,
                        messages = new[]
                        {
                            new
                            {
                                role = "user",
                                content = contentItems
                            }
                        },
                        max_tokens = request.MaxTokens,
                        temperature = request.Temperature
                    };

                    var jsonContent = JsonConvert.SerializeObject(requestPayload);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Send request to cloud API
                    var chatUrl = $"{BaseUrl}/chat/completions";
                    var response = await _httpClient.PostAsync(chatUrl, content, cancellationToken).ConfigureAwait(false);

                    // Handle rate limiting and server errors with retry
                    if ((int)response.StatusCode == 429 || ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600))
                    {
                        if (attempt < MaxRetries - 1)
                        {
                            var delay = InitialRetryDelayMs * (int)Math.Pow(2, attempt);
                            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                            continue;
                        }
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        
                        if ((int)response.StatusCode == 401 || (int)response.StatusCode == 403)
                        {
                            throw new AIServiceAuthenticationException("Invalid API key. Please check your configuration.");
                        }

                        return MultimodalResponse.CreateError($"HTTP {(int)response.StatusCode}: {errorContent}");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    stopwatch.Stop();

                    // Extract markdown content from OpenAI-compatible response
                    string markdownContent = responseObject?.choices?[0]?.message?.content?.ToString() ?? responseContent;

                    return MultimodalResponse.CreateSuccess(markdownContent, stopwatch.ElapsedMilliseconds);
                }
                catch (TaskCanceledException ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return MultimodalResponse.CreateError("Analysis was cancelled");
                    }
                    
                    if (attempt < MaxRetries - 1)
                    {
                        continue;
                    }
                    
                    throw new AIServiceTimeoutException($"Request timed out after {_timeoutSeconds} seconds", ex);
                }
                catch (HttpRequestException ex)
                {
                    if (attempt < MaxRetries - 1)
                    {
                        var delay = InitialRetryDelayMs * (int)Math.Pow(2, attempt);
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    throw new AIServiceNetworkException("Unable to connect to cloud service. Check your internet connection.", ex);
                }
                catch (JsonException ex)
                {
                    throw new AIServiceException("Invalid response format from cloud service", ex);
                }
            }

            return MultimodalResponse.CreateError("Request failed after multiple retries");
        }

        public async Task<bool> IsAvailableAsync()
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Length < 8)
                return false;

            try
            {
                // Simple validation: check if API key is properly formatted
                // Optionally could send a test request to /models endpoint
                return await Task.FromResult(true).ConfigureAwait(false);
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
