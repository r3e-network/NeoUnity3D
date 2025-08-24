using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Neo.Unity.SDK.Utils;

namespace Neo.Unity.SDK.Core
{
    /// <summary>
    /// Base service interface for Neo Unity SDK network operations.
    /// Defines the contract for sending requests and receiving responses from Neo RPC nodes.
    /// </summary>
    public interface INeoUnityService
    {
        /// <summary>Whether to include raw JSON responses in the response objects</summary>
        bool IncludeRawResponses { get; }
        
        /// <summary>
        /// Send a request to the Neo RPC node and receive a typed response.
        /// </summary>
        /// <typeparam name="TResponse">Response type implementing IResponse</typeparam>
        /// <typeparam name="TResult">Result data type</typeparam>
        /// <param name="request">The request to send</param>
        /// <returns>The typed response</returns>
        Task<TResponse> SendAsync<TResponse, TResult>(Request<TResponse, TResult> request) 
            where TResponse : IResponse<TResult>, new();
            
        /// <summary>
        /// Perform raw I/O operation with the Neo RPC node.
        /// </summary>
        /// <param name="payload">Request payload as JSON bytes</param>
        /// <returns>Response data as JSON bytes</returns>
        Task<byte[]> PerformIOAsync(byte[] payload);
    }
    
    /// <summary>
    /// Unity-optimized HTTP service for Neo blockchain RPC communication.
    /// Uses UnityWebRequest for cross-platform compatibility and proper Unity integration.
    /// </summary>
    public class NeoUnityHttpService : INeoUnityService
    {
        private readonly string baseUrl;
        private readonly float timeoutSeconds;
        private readonly bool enableDebugLogging;
        
        /// <summary>
        /// Whether to include raw JSON responses in response objects.
        /// </summary>
        public bool IncludeRawResponses { get; }
        
        /// <summary>
        /// Create a new Neo Unity HTTP service.
        /// </summary>
        /// <param name="baseUrl">Neo RPC node base URL</param>
        /// <param name="includeRawResponses">Whether to include raw responses</param>
        /// <param name="timeoutSeconds">Request timeout in seconds</param>
        /// <param name="enableDebugLogging">Enable debug logging</param>
        public NeoUnityHttpService(string baseUrl, bool includeRawResponses = false, float timeoutSeconds = 30f, bool enableDebugLogging = false)
        {
            this.baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            this.IncludeRawResponses = includeRawResponses;
            this.timeoutSeconds = timeoutSeconds;
            this.enableDebugLogging = enableDebugLogging;
        }
        
        /// <summary>
        /// Send a request to the Neo RPC node and receive a typed response.
        /// </summary>
        /// <typeparam name="TResponse">Response type implementing IResponse</typeparam>
        /// <typeparam name="TResult">Result data type</typeparam>
        /// <param name="request">The request to send</param>
        /// <returns>The typed response</returns>
        public async Task<TResponse> SendAsync<TResponse, TResult>(Request<TResponse, TResult> request) 
            where TResponse : IResponse<TResult>, new()
        {
            try
            {
                // Serialize request to JSON
                var json = JsonConvert.SerializeObject(request, JsonSettings.Default);
                var payload = Encoding.UTF8.GetBytes(json);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"[NeoUnityHttpService] Sending request: {json}");
                }
                
                // Perform I/O operation
                var responseBytes = await PerformIOAsync(payload);
                var responseJson = Encoding.UTF8.GetString(responseBytes);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"[NeoUnityHttpService] Received response: {responseJson}");
                }
                
                // Deserialize response
                var response = JsonConvert.DeserializeObject<TResponse>(responseJson, JsonSettings.Default);
                
                // Store raw response if requested
                if (IncludeRawResponses && response is IRawResponse rawResponse)
                {
                    rawResponse.RawResponse = responseJson;
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NeoUnityHttpService] Request failed: {ex.Message}");
                throw new NeoUnityException($"Failed to send request: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Perform raw I/O operation with the Neo RPC node using UnityWebRequest.
        /// </summary>
        /// <param name="payload">Request payload as JSON bytes</param>
        /// <returns>Response data as JSON bytes</returns>
        public async Task<byte[]> PerformIOAsync(byte[] payload)
        {
            using (var request = new UnityWebRequest(baseUrl, "POST"))
            {
                // Setup request
                request.uploadHandler = new UploadHandlerRaw(payload);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = (int)timeoutSeconds;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                
                // Send request asynchronously
                var operation = request.SendWebRequest();
                
                // Wait for completion using Unity's async operation
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                // Handle response
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.data;
                }
                else
                {
                    var errorMessage = $"HTTP {request.responseCode}: {request.error}";
                    if (enableDebugLogging)
                    {
                        Debug.LogError($"[NeoUnityHttpService] HTTP request failed: {errorMessage}");
                        Debug.LogError($"[NeoUnityHttpService] Response: {request.downloadHandler?.text}");
                    }
                    throw new NeoUnityException($"HTTP request failed: {errorMessage}");
                }
            }
        }
    }
    
    /// <summary>
    /// Request object for Neo RPC calls.
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="TResult">Result data type</typeparam>
    [System.Serializable]
    public class Request<TResponse, TResult> 
        where TResponse : IResponse<TResult>
    {
        /// <summary>JSON-RPC version</summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";
        
        /// <summary>Request ID for correlation</summary>
        [JsonProperty("id")]
        public int Id { get; set; }
        
        /// <summary>RPC method name</summary>
        [JsonProperty("method")]
        public string Method { get; set; }
        
        /// <summary>Method parameters</summary>
        [JsonProperty("params")]
        public object[] Params { get; set; }
        
        /// <summary>Service instance for sending the request</summary>
        [JsonIgnore]
        public INeoUnityService Service { get; set; }
        
        /// <summary>
        /// Create a new request.
        /// </summary>
        /// <param name="method">RPC method name</param>
        /// <param name="parameters">Method parameters</param>
        /// <param name="service">Service instance</param>
        public Request(string method, object[] parameters, INeoUnityService service)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Params = parameters ?? new object[0];
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Id = RequestCounter.GetNextId();
        }
        
        /// <summary>
        /// Send this request and return the response.
        /// </summary>
        /// <returns>The response from the Neo RPC node</returns>
        public async Task<TResponse> SendAsync()
        {
            return await Service.SendAsync<TResponse, TResult>(this);
        }
    }
    
    /// <summary>
    /// Base interface for all Neo RPC responses.
    /// </summary>
    /// <typeparam name="T">Result data type</typeparam>
    public interface IResponse<T>
    {
        /// <summary>JSON-RPC version</summary>
        string JsonRpc { get; set; }
        
        /// <summary>Request ID for correlation</summary>
        int Id { get; set; }
        
        /// <summary>Response result</summary>
        T Result { get; set; }
        
        /// <summary>Response error (if any)</summary>
        RpcError Error { get; set; }
        
        /// <summary>Whether the response contains an error</summary>
        bool HasError { get; }
        
        /// <summary>Get the result or throw if there's an error</summary>
        T GetResult();
    }
    
    /// <summary>
    /// Interface for responses that can include raw JSON.
    /// </summary>
    public interface IRawResponse
    {
        /// <summary>Raw JSON response</summary>
        string RawResponse { get; set; }
    }
    
    /// <summary>
    /// Base response implementation for Neo RPC calls.
    /// </summary>
    /// <typeparam name="T">Result data type</typeparam>
    [System.Serializable]
    public class Response<T> : IResponse<T>, IRawResponse
    {
        /// <summary>JSON-RPC version</summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; }
        
        /// <summary>Request ID for correlation</summary>
        [JsonProperty("id")]
        public int Id { get; set; }
        
        /// <summary>Response result</summary>
        [JsonProperty("result")]
        public T Result { get; set; }
        
        /// <summary>Response error (if any)</summary>
        [JsonProperty("error")]
        public RpcError Error { get; set; }
        
        /// <summary>Raw JSON response (populated if IncludeRawResponses is true)</summary>
        [JsonIgnore]
        public string RawResponse { get; set; }
        
        /// <summary>Whether the response contains an error</summary>
        [JsonIgnore]
        public bool HasError => Error != null;
        
        /// <summary>
        /// Get the result or throw if there's an error.
        /// </summary>
        /// <returns>The response result</returns>
        /// <exception cref="NeoUnityException">If the response contains an error</exception>
        public T GetResult()
        {
            if (HasError)
            {
                throw new NeoUnityException($"RPC Error {Error.Code}: {Error.Message}");
            }
            return Result;
        }
    }
    
    /// <summary>
    /// RPC error information.
    /// </summary>
    [System.Serializable]
    public class RpcError
    {
        /// <summary>Error code</summary>
        [JsonProperty("code")]
        public int Code { get; set; }
        
        /// <summary>Error message</summary>
        [JsonProperty("message")]
        public string Message { get; set; }
        
        /// <summary>Additional error data</summary>
        [JsonProperty("data")]
        public object Data { get; set; }
    }
    
    /// <summary>
    /// Thread-safe request ID counter.
    /// </summary>
    internal static class RequestCounter
    {
        private static readonly object lockObject = new object();
        private static int counter = 1;
        
        /// <summary>
        /// Get the next request ID.
        /// </summary>
        /// <returns>Unique request ID</returns>
        public static int GetNextId()
        {
            lock (lockObject)
            {
                return counter++;
            }
        }
        
        /// <summary>
        /// Reset the counter to 1.
        /// </summary>
        public static void Reset()
        {
            lock (lockObject)
            {
                counter = 1;
            }
        }
    }
}