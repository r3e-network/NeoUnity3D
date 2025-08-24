using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using Neo.Unity.SDK.Protocol;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// HTTP client utilities for Unity compatible networking operations.
    /// Provides a unified interface for HTTP operations across different Unity platforms
    /// with proper async/await support, error handling, and timeout management.
    /// </summary>
    public static class HttpClientUtils
    {
        #region Constants
        
        /// <summary>Default timeout for HTTP requests in milliseconds</summary>
        public const int DEFAULT_TIMEOUT_MS = 30000; // 30 seconds
        
        /// <summary>JSON content type header</summary>
        public const string JSON_CONTENT_TYPE = "application/json; charset=utf-8";
        
        /// <summary>Default User-Agent for requests</summary>
        public const string DEFAULT_USER_AGENT = "NeoUnity-SDK/1.0";
        
        #endregion
        
        #region Private Fields
        
        private static readonly Dictionary<string, HttpClient> clientCache = new Dictionary<string, HttpClient>();
        private static readonly object cacheLock = new object();
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Performs an HTTP POST request with JSON data.
        /// </summary>
        /// <param name="url">The target URL</param>
        /// <param name="jsonData">The JSON data to send</param>
        /// <param name="headers">Additional HTTP headers</param>
        /// <param name="timeoutMs">Request timeout in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The response data as string</returns>
        public static async Task<string> PostJsonAsync(string url, string jsonData, 
                                                      Dictionary<string, string> headers = null,
                                                      int timeoutMs = DEFAULT_TIMEOUT_MS,
                                                      CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            
            if (string.IsNullOrEmpty(jsonData))
                throw new ArgumentException("JSON data cannot be null or empty", nameof(jsonData));
            
            try
            {
                using (var client = CreateHttpClient(timeoutMs))
                {
                    // Add custom headers
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    
                    // Create content
                    using (var content = new StringContent(jsonData, Encoding.UTF8, JSON_CONTENT_TYPE))
                    {
                        using (var response = await client.PostAsync(url, content, cancellationToken))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                var errorContent = await response.Content.ReadAsStringAsync();
                                throw ProtocolException.ClientConnection(
                                    $"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorContent}",
                                    url);
                            }
                            
                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || cancellationToken.IsCancellationRequested)
            {
                throw ProtocolException.NetworkTimeout("POST request", timeoutMs);
            }
            catch (HttpRequestException ex)
            {
                throw ProtocolException.ClientConnection($"HTTP request failed: {ex.Message}", url);
            }
            catch (Exception ex) when (!(ex is ProtocolException))
            {
                throw ProtocolException.ClientConnection($"Unexpected error during HTTP request: {ex.Message}", url);
            }
        }
        
        /// <summary>
        /// Performs an HTTP GET request.
        /// </summary>
        /// <param name="url">The target URL</param>
        /// <param name="headers">Additional HTTP headers</param>
        /// <param name="timeoutMs">Request timeout in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The response data as string</returns>
        public static async Task<string> GetAsync(string url, 
                                                 Dictionary<string, string> headers = null,
                                                 int timeoutMs = DEFAULT_TIMEOUT_MS,
                                                 CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            
            try
            {
                using (var client = CreateHttpClient(timeoutMs))
                {
                    // Add custom headers
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    
                    using (var response = await client.GetAsync(url, cancellationToken))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            throw ProtocolException.ClientConnection(
                                $"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorContent}",
                                url);
                        }
                        
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || cancellationToken.IsCancellationRequested)
            {
                throw ProtocolException.NetworkTimeout("GET request", timeoutMs);
            }
            catch (HttpRequestException ex)
            {
                throw ProtocolException.ClientConnection($"HTTP request failed: {ex.Message}", url);
            }
            catch (Exception ex) when (!(ex is ProtocolException))
            {
                throw ProtocolException.ClientConnection($"Unexpected error during HTTP request: {ex.Message}", url);
            }
        }
        
        /// <summary>
        /// Performs an HTTP PUT request with JSON data.
        /// </summary>
        /// <param name="url">The target URL</param>
        /// <param name="jsonData">The JSON data to send</param>
        /// <param name="headers">Additional HTTP headers</param>
        /// <param name="timeoutMs">Request timeout in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The response data as string</returns>
        public static async Task<string> PutJsonAsync(string url, string jsonData,
                                                     Dictionary<string, string> headers = null,
                                                     int timeoutMs = DEFAULT_TIMEOUT_MS,
                                                     CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            
            try
            {
                using (var client = CreateHttpClient(timeoutMs))
                {
                    // Add custom headers
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    
                    // Create content
                    using (var content = jsonData != null 
                        ? new StringContent(jsonData, Encoding.UTF8, JSON_CONTENT_TYPE)
                        : new StringContent("", Encoding.UTF8, JSON_CONTENT_TYPE))
                    {
                        using (var response = await client.PutAsync(url, content, cancellationToken))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                var errorContent = await response.Content.ReadAsStringAsync();
                                throw ProtocolException.ClientConnection(
                                    $"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorContent}",
                                    url);
                            }
                            
                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || cancellationToken.IsCancellationRequested)
            {
                throw ProtocolException.NetworkTimeout("PUT request", timeoutMs);
            }
            catch (HttpRequestException ex)
            {
                throw ProtocolException.ClientConnection($"HTTP request failed: {ex.Message}", url);
            }
            catch (Exception ex) when (!(ex is ProtocolException))
            {
                throw ProtocolException.ClientConnection($"Unexpected error during HTTP request: {ex.Message}", url);
            }
        }
        
        /// <summary>
        /// Performs an HTTP DELETE request.
        /// </summary>
        /// <param name="url">The target URL</param>
        /// <param name="headers">Additional HTTP headers</param>
        /// <param name="timeoutMs">Request timeout in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The response data as string</returns>
        public static async Task<string> DeleteAsync(string url,
                                                    Dictionary<string, string> headers = null,
                                                    int timeoutMs = DEFAULT_TIMEOUT_MS,
                                                    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
            
            try
            {
                using (var client = CreateHttpClient(timeoutMs))
                {
                    // Add custom headers
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }
                    
                    using (var response = await client.DeleteAsync(url, cancellationToken))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            throw ProtocolException.ClientConnection(
                                $"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorContent}",
                                url);
                        }
                        
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || cancellationToken.IsCancellationRequested)
            {
                throw ProtocolException.NetworkTimeout("DELETE request", timeoutMs);
            }
            catch (HttpRequestException ex)
            {
                throw ProtocolException.ClientConnection($"HTTP request failed: {ex.Message}", url);
            }
            catch (Exception ex) when (!(ex is ProtocolException))
            {
                throw ProtocolException.ClientConnection($"Unexpected error during HTTP request: {ex.Message}", url);
            }
        }
        
        /// <summary>
        /// Performs an HTTP request with retry logic.
        /// </summary>
        /// <param name="requestFunc">The HTTP request function to execute</param>
        /// <param name="maxRetries">Maximum number of retries</param>
        /// <param name="retryDelay">Delay between retries in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The response data as string</returns>
        public static async Task<string> RequestWithRetryAsync(Func<Task<string>> requestFunc,
                                                              int maxRetries = 3,
                                                              int retryDelay = 1000,
                                                              CancellationToken cancellationToken = default)
        {
            if (requestFunc == null)
                throw new ArgumentNullException(nameof(requestFunc));
            
            Exception lastException = null;
            
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await requestFunc();
                }
                catch (ProtocolException ex) when (
                    ex.ErrorType == ProtocolErrorType.NetworkTimeout ||
                    ex.ErrorType == ProtocolErrorType.ClientConnection ||
                    ex.ErrorType == ProtocolErrorType.RateLimitExceeded)
                {
                    lastException = ex;
                    
                    if (attempt < maxRetries)
                    {
                        if (NeoUnity.Instance?.Config?.EnableDebugLogging == true)
                        {
                            Debug.LogWarning($"[HttpClientUtils] Request failed (attempt {attempt + 1}/{maxRetries + 1}): {ex.Message}. Retrying in {retryDelay}ms...");
                        }
                        
                        await Task.Delay(retryDelay, cancellationToken);
                        retryDelay *= 2; // Exponential backoff
                    }
                }
                catch (Exception ex)
                {
                    // Don't retry for other types of exceptions
                    throw;
                }
            }
            
            throw lastException ?? new ProtocolException("Request failed after all retry attempts");
        }
        
        #endregion
        
        #region Unity-Specific Methods
        
        /// <summary>
        /// Checks network reachability using Unity's Application class.
        /// </summary>
        /// <returns>True if network is reachable</returns>
        public static bool IsNetworkReachable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        
        /// <summary>
        /// Gets the current network type.
        /// </summary>
        /// <returns>Network reachability type</returns>
        public static NetworkReachability GetNetworkType()
        {
            return Application.internetReachability;
        }
        
        /// <summary>
        /// Waits for network connectivity with timeout.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if network becomes available</returns>
        public static async Task<bool> WaitForNetworkAsync(int timeoutMs = 10000, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            
            while (!IsNetworkReachable() && (DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;
                
                await Task.Delay(100, cancellationToken); // Check every 100ms
            }
            
            return IsNetworkReachable();
        }
        
        #endregion
        
        #region Private Helper Methods
        
        /// <summary>
        /// Creates a configured HttpClient instance.
        /// </summary>
        /// <param name="timeoutMs">Request timeout in milliseconds</param>
        /// <returns>Configured HttpClient</returns>
        private static HttpClient CreateHttpClient(int timeoutMs)
        {
            var client = new HttpClient()
            {
                Timeout = TimeSpan.FromMilliseconds(timeoutMs)
            };
            
            // Set default headers
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", DEFAULT_USER_AGENT);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", JSON_CONTENT_TYPE);
            
            return client;
        }
        
        /// <summary>
        /// Gets or creates a cached HttpClient for long-lived connections.
        /// </summary>
        /// <param name="baseUrl">Base URL for the client</param>
        /// <param name="timeoutMs">Request timeout in milliseconds</param>
        /// <returns>Cached HttpClient</returns>
        private static HttpClient GetCachedClient(string baseUrl, int timeoutMs = DEFAULT_TIMEOUT_MS)
        {
            lock (cacheLock)
            {
                if (!clientCache.TryGetValue(baseUrl, out var client))
                {
                    client = new HttpClient()
                    {
                        BaseAddress = new Uri(baseUrl),
                        Timeout = TimeSpan.FromMilliseconds(timeoutMs)
                    };
                    
                    // Set default headers
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", DEFAULT_USER_AGENT);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", JSON_CONTENT_TYPE);
                    
                    clientCache[baseUrl] = client;
                }
                
                return client;
            }
        }
        
        #endregion
        
        #region Cleanup
        
        /// <summary>
        /// Cleans up cached HttpClient instances. Call this when shutting down.
        /// </summary>
        public static void Cleanup()
        {
            lock (cacheLock)
            {
                foreach (var client in clientCache.Values)
                {
                    try
                    {
                        client.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[HttpClientUtils] Error disposing HttpClient: {ex.Message}");
                    }
                }
                
                clientCache.Clear();
            }
        }
        
        #endregion
        
        #region Extension Methods
        
        /// <summary>
        /// Extension methods for HttpClient to support the URLRequester interface pattern from Swift.
        /// </summary>
        public static class HttpClientExtensions
        {
            /// <summary>
            /// Performs a data request similar to URLSession.dataTask.
            /// </summary>
            /// <param name="client">The HttpClient instance</param>
            /// <param name="request">The HTTP request message</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Tuple of data bytes and response</returns>
            public static async Task<(byte[] Data, HttpResponseMessage Response)> GetDataAsync(
                this HttpClient client, 
                HttpRequestMessage request, 
                CancellationToken cancellationToken = default)
            {
                if (client == null)
                    throw new ArgumentNullException(nameof(client));
                
                if (request == null)
                    throw new ArgumentNullException(nameof(request));
                
                try
                {
                    var response = await client.SendAsync(request, cancellationToken);
                    var data = await response.Content.ReadAsByteArrayAsync();
                    
                    return (data, response);
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || cancellationToken.IsCancellationRequested)
                {
                    throw ProtocolException.NetworkTimeout("HTTP request", (int)client.Timeout.TotalMilliseconds);
                }
                catch (HttpRequestException ex)
                {
                    throw ProtocolException.ClientConnection($"HTTP request failed: {ex.Message}", request.RequestUri?.ToString());
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface that mimics URLRequester protocol from Swift for compatibility.
    /// </summary>
    public interface IHttpRequester
    {
        /// <summary>
        /// Performs an HTTP request and returns the response data.
        /// </summary>
        /// <param name="request">The HTTP request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response data and HTTP response</returns>
        Task<(byte[] Data, HttpResponseMessage Response)> GetDataAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Default implementation of IHttpRequester using HttpClient.
    /// </summary>
    public class DefaultHttpRequester : IHttpRequester
    {
        private readonly HttpClient _httpClient;
        
        /// <summary>
        /// Creates a new instance with the provided HttpClient.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use</param>
        public DefaultHttpRequester(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }
        
        /// <summary>
        /// Performs an HTTP request and returns the response data.
        /// </summary>
        /// <param name="request">The HTTP request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response data and HTTP response</returns>
        public async Task<(byte[] Data, HttpResponseMessage Response)> GetDataAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetDataAsync(request, cancellationToken);
        }
        
        /// <summary>
        /// Disposes the underlying HttpClient.
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}