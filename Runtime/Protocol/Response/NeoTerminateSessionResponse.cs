using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the terminatesession RPC call.
    /// Returns whether the session was successfully terminated.
    /// </summary>
    [System.Serializable]
    public class NeoTerminateSessionResponse : NeoResponse<bool>
    {
        /// <summary>
        /// Gets the termination result from the response.
        /// </summary>
        public bool TerminationResult => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoTerminateSessionResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful terminate session response.
        /// </summary>
        /// <param name="terminationResult">The termination result</param>
        /// <param name="id">The request ID</param>
        public NeoTerminateSessionResponse(bool terminationResult, int id = 1) : base(terminationResult, id)
        {
        }

        /// <summary>
        /// Creates an error terminate session response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoTerminateSessionResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Checks if the session was successfully terminated.
        /// </summary>
        [JsonIgnore]
        public bool WasSessionTerminated => IsSuccess && TerminationResult;

        /// <summary>
        /// Checks if the session termination failed.
        /// </summary>
        [JsonIgnore]
        public bool DidTerminationFail => HasError || !TerminationResult;

        /// <summary>
        /// Gets the termination status as a descriptive string.
        /// </summary>
        [JsonIgnore]
        public string TerminationStatus
        {
            get
            {
                if (HasError)
                    return $"Error: {Error.Message}";

                if (TerminationResult)
                    return "Session successfully terminated";

                return "Session termination failed";
            }
        }

        /// <summary>
        /// Returns a string representation of the terminate session response.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (HasError)
                return base.ToString();

            return $"NeoTerminateSessionResponse(Success: {WasSessionTerminated}, Status: {TerminationStatus})";
        }

        /// <summary>
        /// Gets detailed information about the session termination.
        /// </summary>
        /// <returns>Detailed description</returns>
        public string GetDetailedInfo()
        {
            var info = "Session Termination Information:\n" +
                      $"  Request ID: {Id}\n" +
                      $"  JSON-RPC: {JsonRpc}\n" +
                      $"  Has Error: {HasError}\n";

            if (HasError)
            {
                info += $"  Error Code: {Error.Code}\n" +
                       $"  Error Message: {Error.Message}\n";
                if (!string.IsNullOrEmpty(Error.Data))
                    info += $"  Error Data: {Error.Data}\n";
            }
            else
            {
                info += $"  Termination Result: {TerminationResult}\n" +
                       $"  Was Successful: {WasSessionTerminated}\n" +
                       $"  Status: {TerminationStatus}\n";
            }

            return info.TrimEnd('\n');
        }

        /// <summary>
        /// Validates the session termination response.
        /// </summary>
        /// <exception cref="ArgumentException">If the response is invalid</exception>
        public void ValidateTermination()
        {
            if (HasError)
            {
                throw new ArgumentException($"Session termination failed with error: {Error.Message}");
            }

            // Note: A false result is valid (indicates the session couldn't be terminated for some reason)
        }

        /// <summary>
        /// Creates a copy of this response with a new termination result.
        /// </summary>
        /// <param name="newResult">The new termination result</param>
        /// <returns>New NeoTerminateSessionResponse instance</returns>
        public NeoTerminateSessionResponse WithResult(bool newResult)
        {
            return new NeoTerminateSessionResponse(newResult, Id);
        }

        /// <summary>
        /// Convenience method to check if the operation completed successfully.
        /// This checks both that there was no error AND that the termination was successful.
        /// </summary>
        /// <returns>True if the session was successfully terminated without errors</returns>
        public bool IsCompleteSuccess()
        {
            return IsSuccess && TerminationResult;
        }

        /// <summary>
        /// Gets the reason for termination failure, if applicable.
        /// </summary>
        [JsonIgnore]
        public string FailureReason
        {
            get
            {
                if (HasError)
                    return Error.Message ?? "Unknown error occurred";

                if (!TerminationResult)
                    return "Termination returned false (session may not exist or already terminated)";

                return null; // No failure
            }
        }

        /// <summary>
        /// Checks if the termination failed due to a specific error code.
        /// </summary>
        /// <param name="errorCode">The error code to check for</param>
        /// <returns>True if the termination failed with the specified error code</returns>
        public bool FailedWithErrorCode(int errorCode)
        {
            return HasError && Error.Code == errorCode;
        }

        /// <summary>
        /// Checks if the termination failed due to a session not found error.
        /// This is typically indicated by specific error codes or messages.
        /// </summary>
        [JsonIgnore]
        public bool FailedDueToSessionNotFound
        {
            get
            {
                if (!HasError)
                    return false;

                // Common error codes for "not found" scenarios
                var notFoundCodes = new[] { -32601, -32602, -1 }; // Method not found, Invalid params, Generic error
                
                if (notFoundCodes.Contains(Error.Code))
                    return true;

                // Check error message for session-related terms
                var message = Error.Message?.ToLowerInvariant() ?? "";
                return message.Contains("session") && 
                       (message.Contains("not found") || message.Contains("invalid") || message.Contains("expired"));
            }
        }

        /// <summary>
        /// Gets a user-friendly success message.
        /// </summary>
        [JsonIgnore]
        public string SuccessMessage
        {
            get
            {
                if (HasError)
                    return null;

                if (TerminationResult)
                    return "Session has been successfully terminated.";

                return "Session termination was processed but returned false. The session may have already been terminated or may not exist.";
            }
        }

        /// <summary>
        /// Compares this response with another terminate session response.
        /// </summary>
        /// <param name="other">The other response to compare with</param>
        /// <returns>True if both responses have the same termination result</returns>
        public bool ResultEquals(NeoTerminateSessionResponse other)
        {
            if (other == null)
                return false;

            // Compare error states
            if (HasError != other.HasError)
                return false;

            if (HasError && other.HasError)
            {
                return Error.Code == other.Error.Code &&
                       string.Equals(Error.Message, other.Error.Message, StringComparison.OrdinalIgnoreCase);
            }

            // Compare results
            return TerminationResult == other.TerminationResult;
        }

        /// <summary>
        /// Gets hash code based on the termination result and error state.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            if (HasError)
                return Error.Code.GetHashCode() ^ (Error.Message?.GetHashCode() ?? 0);

            return TerminationResult.GetHashCode();
        }
    }
}