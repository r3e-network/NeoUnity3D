using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the invokecontractverify RPC call.
    /// Returns the invocation result from verifying a contract.
    /// This is essentially the same as a regular invoke response but specifically for contract verification.
    /// </summary>
    [System.Serializable]
    public class NeoInvokeContractVerifyResponse : NeoResponse<InvocationResult>
    {
        /// <summary>
        /// Gets the invocation result from the response.
        /// </summary>
        public InvocationResult InvocationResult => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoInvokeContractVerifyResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful invoke contract verify response.
        /// </summary>
        /// <param name="invocationResult">The invocation result</param>
        /// <param name="id">The request ID</param>
        public NeoInvokeContractVerifyResponse(InvocationResult invocationResult, int id = 1) : base(invocationResult, id)
        {
        }

        /// <summary>
        /// Creates an error invoke contract verify response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoInvokeContractVerifyResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Checks if the contract verification was successful.
        /// This checks both that there was no RPC error and that the invocation state is HALT.
        /// </summary>
        [JsonIgnore]
        public bool IsVerificationSuccessful
        {
            get
            {
                if (HasError || InvocationResult == null)
                    return false;

                return InvocationResult.State.Equals("HALT", StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Checks if the contract verification failed.
        /// </summary>
        [JsonIgnore]
        public bool DidVerificationFail => !IsVerificationSuccessful;

        /// <summary>
        /// Gets the verification state from the invocation result.
        /// </summary>
        [JsonIgnore]
        public string VerificationState => InvocationResult?.State ?? "UNKNOWN";

        /// <summary>
        /// Gets the GAS consumed by the contract verification.
        /// </summary>
        [JsonIgnore]
        public string GasConsumed => InvocationResult?.GasConsumed ?? "0";

        /// <summary>
        /// Gets the exception message if the verification failed.
        /// </summary>
        [JsonIgnore]
        public string VerificationException => InvocationResult?.Exception;

        /// <summary>
        /// Checks if there was an exception during verification.
        /// </summary>
        [JsonIgnore]
        public bool HasVerificationException => !string.IsNullOrEmpty(VerificationException);

        /// <summary>
        /// Gets the verification result as a descriptive string.
        /// </summary>
        [JsonIgnore]
        public string VerificationStatus
        {
            get
            {
                if (HasError)
                    return $"RPC Error: {Error.Message}";

                if (InvocationResult == null)
                    return "No invocation result";

                if (IsVerificationSuccessful)
                    return "Verification successful (HALT)";

                var status = $"Verification failed ({VerificationState})";
                if (HasVerificationException)
                    status += $": {VerificationException}";

                return status;
            }
        }

        /// <summary>
        /// Gets the verification result stack (return values).
        /// </summary>
        [JsonIgnore]
        public object[] VerificationStack => InvocationResult?.Stack;

        /// <summary>
        /// Gets the number of items in the verification result stack.
        /// </summary>
        [JsonIgnore]
        public int StackCount => VerificationStack?.Length ?? 0;

        /// <summary>
        /// Checks if the verification returned any stack results.
        /// </summary>
        [JsonIgnore]
        public bool HasStackResults => StackCount > 0;

        /// <summary>
        /// Gets the first stack item as a specific type.
        /// </summary>
        /// <typeparam name="T">The expected type</typeparam>
        /// <returns>The first stack item cast to type T, or default(T) if not available</returns>
        public T GetFirstStackItemAs<T>()
        {
            if (!HasStackResults)
                return default(T);

            var firstItem = VerificationStack[0];
            if (firstItem is T)
                return (T)firstItem;

            // Try to convert string representations
            if (typeof(T) == typeof(string) && firstItem != null)
                return (T)(object)firstItem.ToString();

            // Try to parse common types from strings
            if (firstItem is string str && !string.IsNullOrEmpty(str))
            {
                if (typeof(T) == typeof(bool))
                {
                    // In Neo, verification methods typically return boolean values
                    if (str == "1" || str.Equals("true", StringComparison.OrdinalIgnoreCase))
                        return (T)(object)true;
                    if (str == "0" || str.Equals("false", StringComparison.OrdinalIgnoreCase))
                        return (T)(object)false;
                }

                if (typeof(T) == typeof(int) && int.TryParse(str, out int intVal))
                    return (T)(object)intVal;

                if (typeof(T) == typeof(long) && long.TryParse(str, out long longVal))
                    return (T)(object)longVal;

                if (typeof(T) == typeof(decimal) && decimal.TryParse(str, out decimal decVal))
                    return (T)(object)decVal;
            }

            return default(T);
        }

        /// <summary>
        /// Gets the verification result as a boolean (common for verify methods).
        /// Neo contract verify methods typically return true/false.
        /// </summary>
        [JsonIgnore]
        public bool? VerificationResultBoolean
        {
            get
            {
                if (!IsVerificationSuccessful || !HasStackResults)
                    return null;

                return GetFirstStackItemAs<bool?>();
            }
        }

        /// <summary>
        /// Returns a string representation of the invoke contract verify response.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (HasError)
                return base.ToString();

            var gasInfo = !string.IsNullOrEmpty(GasConsumed) ? $", GAS: {GasConsumed}" : "";
            var stackInfo = HasStackResults ? $", Stack Items: {StackCount}" : "";

            return $"NeoInvokeContractVerifyResponse(State: {VerificationState}{gasInfo}{stackInfo})";
        }

        /// <summary>
        /// Gets detailed information about the contract verification.
        /// </summary>
        /// <returns>Detailed description</returns>
        public string GetDetailedInfo()
        {
            var info = "Contract Verification Information:\n" +
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
            else if (InvocationResult != null)
            {
                info += $"  Verification State: {VerificationState}\n" +
                       $"  Is Successful: {IsVerificationSuccessful}\n" +
                       $"  GAS Consumed: {GasConsumed}\n" +
                       $"  Stack Items: {StackCount}\n";

                if (HasVerificationException)
                    info += $"  Exception: {VerificationException}\n";

                if (VerificationResultBoolean.HasValue)
                    info += $"  Result Boolean: {VerificationResultBoolean.Value}\n";

                if (HasStackResults && StackCount <= 5)
                {
                    info += "  Stack Contents:\n";
                    for (int i = 0; i < StackCount; i++)
                    {
                        var item = VerificationStack[i];
                        var itemStr = item?.ToString() ?? "null";
                        if (itemStr.Length > 50)
                            itemStr = itemStr.Substring(0, 50) + "...";
                        
                        info += $"    [{i}] {item?.GetType().Name ?? "null"}: {itemStr}\n";
                    }
                }

                info += $"  Status: {VerificationStatus}\n";
            }
            else
            {
                info += "  No invocation result available\n";
            }

            return info.TrimEnd('\n');
        }

        /// <summary>
        /// Validates the contract verification response.
        /// </summary>
        /// <exception cref="ArgumentException">If the response indicates verification failure</exception>
        public void ValidateVerification()
        {
            if (HasError)
            {
                throw new ArgumentException($"Contract verification failed with RPC error: {Error.Message}");
            }

            if (InvocationResult == null)
            {
                throw new ArgumentException("Contract verification failed: No invocation result");
            }

            if (!IsVerificationSuccessful)
            {
                var message = $"Contract verification failed with state: {VerificationState}";
                if (HasVerificationException)
                    message += $" - {VerificationException}";
                
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Creates a copy of this response with a new invocation result.
        /// </summary>
        /// <param name="newResult">The new invocation result</param>
        /// <returns>New NeoInvokeContractVerifyResponse instance</returns>
        public NeoInvokeContractVerifyResponse WithResult(InvocationResult newResult)
        {
            return new NeoInvokeContractVerifyResponse(newResult, Id);
        }

        /// <summary>
        /// Checks if the verification consumed more GAS than a specified limit.
        /// </summary>
        /// <param name="gasLimit">The GAS limit to check against</param>
        /// <returns>True if GAS consumption exceeded the limit</returns>
        public bool ExceededGasLimit(decimal gasLimit)
        {
            if (string.IsNullOrEmpty(GasConsumed))
                return false;

            if (decimal.TryParse(GasConsumed, out decimal consumed))
            {
                return consumed > gasLimit;
            }

            return false;
        }

        /// <summary>
        /// Gets a user-friendly success message for the verification.
        /// </summary>
        [JsonIgnore]
        public string SuccessMessage
        {
            get
            {
                if (HasError || !IsVerificationSuccessful)
                    return null;

                var message = "Contract verification completed successfully.";
                
                if (VerificationResultBoolean.HasValue)
                {
                    message += $" Result: {(VerificationResultBoolean.Value ? "VALID" : "INVALID")}";
                }

                if (!string.IsNullOrEmpty(GasConsumed) && GasConsumed != "0")
                {
                    message += $" (GAS consumed: {GasConsumed})";
                }

                return message;
            }
        }

        /// <summary>
        /// Compares this verification result with another.
        /// </summary>
        /// <param name="other">The other verification response to compare with</param>
        /// <returns>True if both have the same verification outcome</returns>
        public bool VerificationEquals(NeoInvokeContractVerifyResponse other)
        {
            if (other == null)
                return false;

            if (HasError != other.HasError)
                return false;

            if (HasError && other.HasError)
            {
                return Error.Code == other.Error.Code &&
                       string.Equals(Error.Message, other.Error.Message, StringComparison.OrdinalIgnoreCase);
            }

            if (InvocationResult == null && other.InvocationResult == null)
                return true;

            if (InvocationResult == null || other.InvocationResult == null)
                return false;

            return string.Equals(VerificationState, other.VerificationState, StringComparison.OrdinalIgnoreCase) &&
                   VerificationResultBoolean == other.VerificationResultBoolean;
        }
    }
}