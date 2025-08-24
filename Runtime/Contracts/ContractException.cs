using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Unity.SDK.Contracts
{
    /// <summary>
    /// Exception types specific to Neo smart contract operations.
    /// Provides detailed error information for various contract-related failures.
    /// </summary>
    public class ContractException : Exception
    {
        /// <summary>
        /// The type of contract error.
        /// </summary>
        public ContractErrorType ErrorType { get; }
        
        /// <summary>
        /// Additional error context data.
        /// </summary>
        public object ErrorData { get; }
        
        /// <summary>
        /// Initializes a new instance of the ContractException class.
        /// </summary>
        /// <param name="errorType">The type of contract error</param>
        /// <param name="message">The error message</param>
        /// <param name="errorData">Additional error context data</param>
        /// <param name="innerException">The inner exception</param>
        public ContractException(ContractErrorType errorType, string message, object errorData = null, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorType = errorType;
            ErrorData = errorData;
        }
        
        /// <summary>
        /// Initializes a new instance of the ContractException class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ContractException(string message, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorType = ContractErrorType.General;
        }
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates an exception for invalid Neo Name Service (NNS) names.
        /// </summary>
        /// <param name="name">The invalid NNS name</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException InvalidNeoName(string name)
        {
            return new ContractException(
                ContractErrorType.InvalidNeoName,
                $"'{name}' is not a valid NNS name.",
                name
            );
        }
        
        /// <summary>
        /// Creates an exception for invalid Neo Name Service roots.
        /// </summary>
        /// <param name="root">The invalid NNS root</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException InvalidNeoNameServiceRoot(string root)
        {
            return new ContractException(
                ContractErrorType.InvalidNeoNameServiceRoot,
                $"'{root}' is not a valid NNS root.",
                root
            );
        }
        
        /// <summary>
        /// Creates an exception for unexpected return types from contract invocations.
        /// </summary>
        /// <param name="actualType">The actual return type received</param>
        /// <param name="expectedTypes">The expected return types (optional)</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException UnexpectedReturnType(string actualType, params string[] expectedTypes)
        {
            var message = expectedTypes != null && expectedTypes.Length > 0
                ? $"Got stack item of type {actualType} but expected {string.Join(", ", expectedTypes)}."
                : actualType;
                
            return new ContractException(
                ContractErrorType.UnexpectedReturnType,
                message,
                new { ActualType = actualType, ExpectedTypes = expectedTypes }
            );
        }
        
        /// <summary>
        /// Creates an exception for unresolvable domain names.
        /// </summary>
        /// <param name="domainName">The unresolvable domain name</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException UnresolvableDomainName(string domainName)
        {
            return new ContractException(
                ContractErrorType.UnresolvableDomainName,
                $"The provided domain name '{domainName}' could not be resolved.",
                domainName
            );
        }
        
        /// <summary>
        /// Creates an exception for invalid contract addresses.
        /// </summary>
        /// <param name="address">The invalid contract address</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException InvalidContractAddress(string address)
        {
            return new ContractException(
                ContractErrorType.InvalidContractAddress,
                $"The provided contract address '{address}' is invalid.",
                address
            );
        }
        
        /// <summary>
        /// Creates an exception for contract invocation failures.
        /// </summary>
        /// <param name="contractHash">The contract hash</param>
        /// <param name="method">The method name</param>
        /// <param name="error">The underlying error</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException InvocationFailed(string contractHash, string method, string error)
        {
            return new ContractException(
                ContractErrorType.InvocationFailed,
                $"Contract invocation failed for {contractHash}::{method}: {error}",
                new { ContractHash = contractHash, Method = method, Error = error }
            );
        }
        
        /// <summary>
        /// Creates an exception for insufficient contract balance.
        /// </summary>
        /// <param name="tokenHash">The token contract hash</param>
        /// <param name="required">The required amount</param>
        /// <param name="available">The available amount</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException InsufficientBalance(string tokenHash, decimal required, decimal available)
        {
            return new ContractException(
                ContractErrorType.InsufficientBalance,
                $"Insufficient balance for token {tokenHash}. Required: {required}, Available: {available}",
                new { TokenHash = tokenHash, Required = required, Available = available }
            );
        }
        
        /// <summary>
        /// Creates an exception for contract deployment failures.
        /// </summary>
        /// <param name="error">The deployment error</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException DeploymentFailed(string error)
        {
            return new ContractException(
                ContractErrorType.DeploymentFailed,
                $"Contract deployment failed: {error}",
                error
            );
        }
        
        /// <summary>
        /// Creates an exception for contract update failures.
        /// </summary>
        /// <param name="contractHash">The contract hash</param>
        /// <param name="error">The update error</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException UpdateFailed(string contractHash, string error)
        {
            return new ContractException(
                ContractErrorType.UpdateFailed,
                $"Contract update failed for {contractHash}: {error}",
                new { ContractHash = contractHash, Error = error }
            );
        }
        
        /// <summary>
        /// Creates an exception for invalid method parameters.
        /// </summary>
        /// <param name="method">The method name</param>
        /// <param name="parameterError">The parameter error description</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException InvalidParameters(string method, string parameterError)
        {
            return new ContractException(
                ContractErrorType.InvalidParameters,
                $"Invalid parameters for method '{method}': {parameterError}",
                new { Method = method, ParameterError = parameterError }
            );
        }
        
        /// <summary>
        /// Creates an exception for contract manifest validation failures.
        /// </summary>
        /// <param name="validationError">The validation error</param>
        /// <returns>A ContractException instance</returns>
        public static ContractException ManifestValidationFailed(string validationError)
        {
            return new ContractException(
                ContractErrorType.ManifestValidationFailed,
                $"Contract manifest validation failed: {validationError}",
                validationError
            );
        }
        
        #endregion
        
        #region Overrides
        
        public override string ToString()
        {
            var baseString = base.ToString();
            if (ErrorData != null)
            {
                return $"{baseString}\nError Type: {ErrorType}\nError Data: {ErrorData}";
            }
            return $"{baseString}\nError Type: {ErrorType}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Enumeration of contract error types.
    /// </summary>
    public enum ContractErrorType
    {
        /// <summary>
        /// General contract error.
        /// </summary>
        General,
        
        /// <summary>
        /// Invalid Neo Name Service (NNS) name.
        /// </summary>
        InvalidNeoName,
        
        /// <summary>
        /// Invalid Neo Name Service root.
        /// </summary>
        InvalidNeoNameServiceRoot,
        
        /// <summary>
        /// Unexpected return type from contract invocation.
        /// </summary>
        UnexpectedReturnType,
        
        /// <summary>
        /// Unresolvable domain name.
        /// </summary>
        UnresolvableDomainName,
        
        /// <summary>
        /// Invalid contract address.
        /// </summary>
        InvalidContractAddress,
        
        /// <summary>
        /// Contract invocation failed.
        /// </summary>
        InvocationFailed,
        
        /// <summary>
        /// Insufficient balance for operation.
        /// </summary>
        InsufficientBalance,
        
        /// <summary>
        /// Contract deployment failed.
        /// </summary>
        DeploymentFailed,
        
        /// <summary>
        /// Contract update failed.
        /// </summary>
        UpdateFailed,
        
        /// <summary>
        /// Invalid method parameters.
        /// </summary>
        InvalidParameters,
        
        /// <summary>
        /// Contract manifest validation failed.
        /// </summary>
        ManifestValidationFailed
    }
}