using System;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Base interface for all Neo RPC response objects.
    /// Provides common functionality for response handling and validation.
    /// </summary>
    public interface IResponse<T>
    {
        /// <summary>The JSON-RPC protocol version</summary>
        string JsonRpc { get; set; }
        
        /// <summary>The request ID</summary>
        int Id { get; set; }
        
        /// <summary>The result data</summary>
        T Result { get; set; }
        
        /// <summary>Error information if the request failed</summary>
        ResponseError Error { get; set; }
        
        /// <summary>Raw JSON response for debugging</summary>
        string RawResponse { get; set; }
        
        /// <summary>Whether the response contains an error</summary>
        [JsonIgnore]
        bool HasError { get; }
        
        /// <summary>Whether the response is successful</summary>
        [JsonIgnore]
        bool IsSuccess { get; }
        
        /// <summary>
        /// Gets the result data or throws an exception if there was an error.
        /// </summary>
        /// <returns>The result data</returns>
        /// <exception cref="NeoRpcException">If the response contains an error</exception>
        T GetResult();
    }
}