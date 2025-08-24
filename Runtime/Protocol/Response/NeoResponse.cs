using System;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Exceptions;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Base implementation of Neo RPC responses.
    /// Handles JSON-RPC 2.0 protocol compliance and error handling.
    /// </summary>
    /// <typeparam name="T">The result data type</typeparam>
    [System.Serializable]
    public class NeoResponse<T> : IResponse<T>
    {
        #region Properties
        
        /// <summary>The JSON-RPC protocol version (always "2.0")</summary>
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";
        
        /// <summary>The request ID that matches the original request</summary>
        [JsonProperty("id")]
        public int Id { get; set; }
        
        /// <summary>The result data if the request was successful</summary>
        [JsonProperty("result")]
        public T Result { get; set; }
        
        /// <summary>Error information if the request failed</summary>
        [JsonProperty("error")]
        public ResponseError Error { get; set; }
        
        /// <summary>Raw JSON response string for debugging purposes</summary>
        [JsonIgnore]
        public string RawResponse { get; set; }
        
        /// <summary>Whether this response contains an error</summary>
        [JsonIgnore]
        public bool HasError => Error != null;
        
        /// <summary>Whether this response is successful (no error)</summary>
        [JsonIgnore]
        public bool IsSuccess => Error == null;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoResponse()
        {
            JsonRpc = "2.0";
        }
        
        /// <summary>
        /// Creates a successful response with result data.
        /// </summary>
        /// <param name="result">The result data</param>
        /// <param name="id">The request ID</param>
        public NeoResponse(T result, int id = 1)
        {
            JsonRpc = "2.0";
            Id = id;
            Result = result;
            Error = null;
        }
        
        /// <summary>
        /// Creates an error response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoResponse(ResponseError error, int id = 1)
        {
            JsonRpc = "2.0";
            Id = id;
            Result = default(T);
            Error = error;
        }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Gets the result data or throws an exception if there was an error.
        /// </summary>
        /// <returns>The result data</returns>
        /// <exception cref="NeoRpcException">If the response contains an error</exception>
        public T GetResult()
        {
            if (HasError)
            {
                throw new NeoRpcException($"RPC Error {Error.Code}: {Error.Message}", Error);
            }
            
            return Result;
        }
        
        /// <summary>
        /// Validates that this response is successful and has a non-null result.
        /// </summary>
        /// <exception cref="NeoRpcException">If the response has an error or null result</exception>
        public void ValidateResult()
        {
            if (HasError)
            {
                throw new NeoRpcException($"RPC Error {Error.Code}: {Error.Message}", Error);
            }
            
            if (Result == null)
            {
                throw new NeoRpcException("Response result is null", null);
            }
        }
        
        /// <summary>
        /// Tries to get the result data without throwing exceptions.
        /// </summary>
        /// <param name="result">The result data if successful</param>
        /// <returns>True if successful, false if there was an error</returns>
        public bool TryGetResult(out T result)
        {
            if (IsSuccess)
            {
                result = Result;
                return true;
            }
            
            result = default(T);
            return false;
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this response.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (HasError)
            {
                return $"NeoResponse<{typeof(T).Name}>(Error: {Error.Code} - {Error.Message})";
            }
            
            return $"NeoResponse<{typeof(T).Name}>(Success: {Result != null})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents an error in a Neo RPC response.
    /// </summary>
    [System.Serializable]
    public class ResponseError
    {
        /// <summary>The error code</summary>
        [JsonProperty("code")]
        public int Code { get; set; }
        
        /// <summary>The error message</summary>
        [JsonProperty("message")]
        public string Message { get; set; }
        
        /// <summary>Additional error data (optional)</summary>
        [JsonProperty("data")]
        public string Data { get; set; }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public ResponseError()
        {
        }
        
        /// <summary>
        /// Creates a new response error.
        /// </summary>
        /// <param name="code">The error code</param>
        /// <param name="message">The error message</param>
        /// <param name="data">Additional error data</param>
        public ResponseError(int code, string message, string data = null)
        {
            Code = code;
            Message = message;
            Data = data;
        }
        
        /// <summary>
        /// Returns a string representation of this error.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var result = $"Error {Code}: {Message}";
            if (!string.IsNullOrEmpty(Data))
            {
                result += $" (Data: {Data})";
            }
            return result;
        }
    }
    
    /// <summary>
    /// Exception thrown when Neo RPC operations fail.
    /// </summary>
    public class NeoRpcException : NeoUnityException
    {
        /// <summary>The associated error details</summary>
        public ResponseError Error { get; }
        
        /// <summary>
        /// Creates a new Neo RPC exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="error">The associated error details</param>
        public NeoRpcException(string message, ResponseError error) : base(message)
        {
            Error = error;
        }
        
        /// <summary>
        /// Creates a new Neo RPC exception with inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="error">The associated error details</param>
        /// <param name="innerException">The inner exception</param>
        public NeoRpcException(string message, ResponseError error, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }
    }
}