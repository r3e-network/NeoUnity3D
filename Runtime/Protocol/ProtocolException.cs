using System;
using Neo.Unity.SDK.Types;
using UnityEngine;

namespace Neo.Unity.SDK.Protocol
{
    /// <summary>
    /// Exception types specific to Neo protocol operations.
    /// Provides detailed error information for various protocol-level failures including
    /// RPC communication, VM execution, and client connection issues.
    /// Unity-optimized with proper serialization support.
    /// </summary>
    [System.Serializable]
    public class ProtocolException : NeoUnityException
    {
        /// <summary>
        /// The type of protocol error.
        /// </summary>
        public ProtocolErrorType ErrorType { get; }
        
        /// <summary>
        /// Additional error context data.
        /// </summary>
        public object ErrorData { get; }
        
        /// <summary>
        /// The error code if available (for RPC errors).
        /// </summary>
        public int? ErrorCode { get; }
        
        /// <summary>
        /// Initializes a new instance of the ProtocolException class.
        /// </summary>
        /// <param name="errorType">The type of protocol error</param>
        /// <param name="message">The error message</param>
        /// <param name="errorData">Additional error context data</param>
        /// <param name="errorCode">Error code if available</param>
        /// <param name="innerException">The inner exception</param>
        public ProtocolException(ProtocolErrorType errorType, string message, object errorData = null, 
                               int? errorCode = null, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorType = errorType;
            ErrorData = errorData;
            ErrorCode = errorCode;
        }
        
        /// <summary>
        /// Initializes a new instance of the ProtocolException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ProtocolException(string message, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorType = ProtocolErrorType.General;
        }
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates an exception for RPC response errors.
        /// </summary>
        /// <param name="error">The RPC error message</param>
        /// <param name="errorCode">The RPC error code</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException RpcResponseError(string error, int? errorCode = null)
        {
            return new ProtocolException(
                ProtocolErrorType.RpcResponseError,
                $"The Neo node responded with an error: {error}",
                error,
                errorCode
            );
        }
        
        /// <summary>
        /// Creates an exception for VM invocation FAULT states.
        /// </summary>
        /// <param name="error">The VM fault error message</param>
        /// <param name="vmState">The VM state information</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException InvocationFaultState(string error, object vmState = null)
        {
            return new ProtocolException(
                ProtocolErrorType.InvocationFaultState,
                $"The invocation resulted in a FAULT VM state. The VM exited due to the following exception: {error}",
                new { Error = error, VmState = vmState }
            );
        }
        
        /// <summary>
        /// Creates an exception for client connection issues.
        /// </summary>
        /// <param name="message">The connection error message</param>
        /// <param name="endpoint">The endpoint that failed</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException ClientConnection(string message, string endpoint = null)
        {
            return new ProtocolException(
                ProtocolErrorType.ClientConnection,
                message,
                endpoint
            );
        }
        
        /// <summary>
        /// Creates an exception for stack item casting errors.
        /// </summary>
        /// <param name="stackItem">The stack item that failed to cast</param>
        /// <param name="targetType">The target type name</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException StackItemCastError(StackItem stackItem, string targetType)
        {
            var itemType = stackItem?.Type.ToString() ?? "null";
            var itemValue = stackItem?.GetString() ?? "null";
            
            return new ProtocolException(
                ProtocolErrorType.StackItemCastError,
                $"Cannot cast stack item {itemValue} (type: {itemType}) to a {targetType}.",
                new { StackItem = stackItem, TargetType = targetType }
            );
        }
        
        /// <summary>
        /// Creates an exception for network timeout issues.
        /// </summary>
        /// <param name="operation">The operation that timed out</param>
        /// <param name="timeoutMs">The timeout duration in milliseconds</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException NetworkTimeout(string operation, int timeoutMs)
        {
            return new ProtocolException(
                ProtocolErrorType.NetworkTimeout,
                $"Network timeout occurred during {operation} after {timeoutMs}ms",
                new { Operation = operation, TimeoutMs = timeoutMs }
            );
        }
        
        /// <summary>
        /// Creates an exception for invalid response format.
        /// </summary>
        /// <param name="expectedFormat">The expected response format</param>
        /// <param name="actualContent">The actual response content</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException InvalidResponseFormat(string expectedFormat, string actualContent)
        {
            return new ProtocolException(
                ProtocolErrorType.InvalidResponseFormat,
                $"Invalid response format. Expected: {expectedFormat}",
                new { Expected = expectedFormat, Actual = actualContent }
            );
        }
        
        /// <summary>
        /// Creates an exception for unsupported protocol versions.
        /// </summary>
        /// <param name="supportedVersion">The supported protocol version</param>
        /// <param name="receivedVersion">The received protocol version</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException UnsupportedProtocolVersion(string supportedVersion, string receivedVersion)
        {
            return new ProtocolException(
                ProtocolErrorType.UnsupportedProtocolVersion,
                $"Unsupported protocol version. Supported: {supportedVersion}, Received: {receivedVersion}",
                new { Supported = supportedVersion, Received = receivedVersion }
            );
        }
        
        /// <summary>
        /// Creates an exception for authentication failures.
        /// </summary>
        /// <param name="message">The authentication error message</param>
        /// <param name="authMethod">The authentication method that failed</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException AuthenticationFailed(string message, string authMethod = null)
        {
            return new ProtocolException(
                ProtocolErrorType.AuthenticationFailed,
                $"Authentication failed: {message}",
                authMethod
            );
        }
        
        /// <summary>
        /// Creates an exception for rate limiting.
        /// </summary>
        /// <param name="retryAfterSeconds">Seconds to wait before retry</param>
        /// <param name="requestsPerSecond">The rate limit (requests per second)</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException RateLimitExceeded(int retryAfterSeconds, int? requestsPerSecond = null)
        {
            var message = requestsPerSecond.HasValue 
                ? $"Rate limit exceeded. Limit: {requestsPerSecond} requests/second. Retry after {retryAfterSeconds} seconds."
                : $"Rate limit exceeded. Retry after {retryAfterSeconds} seconds.";
                
            return new ProtocolException(
                ProtocolErrorType.RateLimitExceeded,
                message,
                new { RetryAfterSeconds = retryAfterSeconds, RequestsPerSecond = requestsPerSecond }
            );
        }
        
        /// <summary>
        /// Creates an exception for transaction verification failures.
        /// </summary>
        /// <param name="transactionHash">The transaction hash</param>
        /// <param name="reason">The verification failure reason</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException TransactionVerificationFailed(string transactionHash, string reason)
        {
            return new ProtocolException(
                ProtocolErrorType.TransactionVerificationFailed,
                $"Transaction verification failed for {transactionHash}: {reason}",
                new { TransactionHash = transactionHash, Reason = reason }
            );
        }
        
        /// <summary>
        /// Creates an exception for insufficient network fees.
        /// </summary>
        /// <param name="required">Required network fee</param>
        /// <param name="provided">Provided network fee</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException InsufficientNetworkFee(long required, long provided)
        {
            return new ProtocolException(
                ProtocolErrorType.InsufficientNetworkFee,
                $"Insufficient network fee. Required: {required}, Provided: {provided}",
                new { Required = required, Provided = provided }
            );
        }
        
        /// <summary>
        /// Creates an exception for blockchain synchronization issues.
        /// </summary>
        /// <param name="currentHeight">Current block height</param>
        /// <param name="networkHeight">Network block height</param>
        /// <returns>A ProtocolException instance</returns>
        public static ProtocolException BlockchainSyncRequired(uint currentHeight, uint networkHeight)
        {
            return new ProtocolException(
                ProtocolErrorType.BlockchainSyncRequired,
                $"Blockchain synchronization required. Current: {currentHeight}, Network: {networkHeight}",
                new { CurrentHeight = currentHeight, NetworkHeight = networkHeight }
            );
        }
        
        #endregion
        
        #region Overrides
        
        public override string ToString()
        {
            var baseString = base.ToString();
            var details = new System.Text.StringBuilder();
            
            details.AppendLine($"Error Type: {ErrorType}");
            
            if (ErrorCode.HasValue)
                details.AppendLine($"Error Code: {ErrorCode.Value}");
                
            if (ErrorData != null)
                details.AppendLine($"Error Data: {ErrorData}");
            
            return $"{baseString}\n{details}";
        }
        
        #endregion
        
        #region Unity Logging Integration
        
        /// <summary>
        /// Logs this exception using Unity's logging system with appropriate log level.
        /// </summary>
        /// <param name="context">Optional Unity context object</param>
        public void LogToUnity(UnityEngine.Object context = null)
        {
            var logMessage = $"[ProtocolException] {ErrorType}: {Message}";
            
            switch (ErrorType)
            {
                case ProtocolErrorType.NetworkTimeout:
                case ProtocolErrorType.ClientConnection:
                case ProtocolErrorType.RateLimitExceeded:
                    Debug.LogWarning(logMessage, context);
                    break;
                    
                case ProtocolErrorType.InvocationFaultState:
                case ProtocolErrorType.StackItemCastError:
                case ProtocolErrorType.TransactionVerificationFailed:
                case ProtocolErrorType.InsufficientNetworkFee:
                    Debug.LogError(logMessage, context);
                    break;
                    
                default:
                    Debug.LogError(logMessage, context);
                    break;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Enumeration of protocol error types for detailed error categorization.
    /// </summary>
    public enum ProtocolErrorType
    {
        /// <summary>
        /// General protocol error.
        /// </summary>
        General,
        
        /// <summary>
        /// Neo node returned an RPC error.
        /// </summary>
        RpcResponseError,
        
        /// <summary>
        /// VM invocation resulted in FAULT state.
        /// </summary>
        InvocationFaultState,
        
        /// <summary>
        /// Client connection error.
        /// </summary>
        ClientConnection,
        
        /// <summary>
        /// Stack item casting error.
        /// </summary>
        StackItemCastError,
        
        /// <summary>
        /// Network operation timeout.
        /// </summary>
        NetworkTimeout,
        
        /// <summary>
        /// Invalid response format received.
        /// </summary>
        InvalidResponseFormat,
        
        /// <summary>
        /// Unsupported protocol version.
        /// </summary>
        UnsupportedProtocolVersion,
        
        /// <summary>
        /// Authentication failure.
        /// </summary>
        AuthenticationFailed,
        
        /// <summary>
        /// Rate limit exceeded.
        /// </summary>
        RateLimitExceeded,
        
        /// <summary>
        /// Transaction verification failed.
        /// </summary>
        TransactionVerificationFailed,
        
        /// <summary>
        /// Insufficient network fee for transaction.
        /// </summary>
        InsufficientNetworkFee,
        
        /// <summary>
        /// Blockchain synchronization required.
        /// </summary>
        BlockchainSyncRequired
    }
    
    /// <summary>
    /// Base exception class for Neo Unity SDK.
    /// Provides common functionality for all SDK exceptions.
    /// </summary>
    [System.Serializable]
    public class NeoUnityException : Exception
    {
        /// <summary>
        /// The timestamp when the exception occurred.
        /// </summary>
        public DateTime Timestamp { get; }
        
        /// <summary>
        /// Initializes a new instance of the NeoUnityException class.
        /// </summary>
        /// <param name="message">The error message</param>
        public NeoUnityException(string message) : base(message)
        {
            Timestamp = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Initializes a new instance of the NeoUnityException class with an inner exception.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public NeoUnityException(string message, Exception innerException) : base(message, innerException)
        {
            Timestamp = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Returns a string representation of this exception.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss} UTC] {base.ToString()}";
        }
    }
}