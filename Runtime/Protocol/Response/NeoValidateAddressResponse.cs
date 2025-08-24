using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for validating a Neo address format.
    /// Contains validation result and address information.
    /// </summary>
    [System.Serializable]
    public class NeoValidateAddressResponse : NeoResponse<AddressValidationResult>
    {
        /// <summary>
        /// Gets the validation result from the response.
        /// </summary>
        /// <returns>Validation result or null if response failed</returns>
        public AddressValidationResult GetValidation()
        {
            return IsSuccess ? Result : null;
        }
        
        /// <summary>
        /// Gets the validation result or throws if the response failed.
        /// </summary>
        /// <returns>Validation result</returns>
        /// <exception cref="NeoRpcException">If the response contains an error</exception>
        public AddressValidationResult GetValidationOrThrow()
        {
            return GetResult();
        }
    }
    
    /// <summary>
    /// Represents the result of Neo address validation.
    /// Contains the address and whether it's valid according to Neo address format rules.
    /// </summary>
    [System.Serializable]
    public class AddressValidationResult
    {
        #region Properties
        
        /// <summary>The address that was validated</summary>
        [JsonProperty("address")]
        public string Address { get; set; }
        
        /// <summary>Whether the address is valid</summary>
        [JsonProperty("isvalid")]
        public bool IsValid { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public AddressValidationResult()
        {
        }
        
        /// <summary>
        /// Creates a new address validation result.
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="isValid">Whether it's valid</param>
        public AddressValidationResult(string address, bool isValid)
        {
            Address = address;
            IsValid = isValid;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether the address is invalid</summary>
        [JsonIgnore]
        public bool IsInvalid => !IsValid;
        
        /// <summary>Whether the address appears to be a script hash</summary>
        [JsonIgnore]
        public bool LooksLikeScriptHash => Address?.StartsWith("0x") == true && Address.Length == 42;
        
        /// <summary>Whether the address appears to be a Neo address</summary>
        [JsonIgnore]
        public bool LooksLikeNeoAddress => !string.IsNullOrEmpty(Address) && Address.StartsWith("N") && Address.Length > 30;
        
        #endregion
        
        #region Validation Analysis
        
        /// <summary>
        /// Gets the address format type.
        /// </summary>
        /// <returns>Address format type</returns>
        public AddressFormatType GetAddressFormat()
        {
            if (string.IsNullOrEmpty(Address))
                return AddressFormatType.Empty;
            
            if (LooksLikeScriptHash)
                return AddressFormatType.ScriptHash;
            
            if (LooksLikeNeoAddress)
                return AddressFormatType.NeoAddress;
            
            return AddressFormatType.Unknown;
        }
        
        /// <summary>
        /// Gets a description of why the address might be invalid.
        /// </summary>
        /// <returns>Validation error description</returns>
        public string GetValidationError()
        {
            if (IsValid)
                return "Address is valid";
            
            if (string.IsNullOrEmpty(Address))
                return "Address is null or empty";
            
            var format = GetAddressFormat();
            return format switch
            {
                AddressFormatType.Empty => "Address is empty",
                AddressFormatType.ScriptHash => "Script hash format but invalid checksum or encoding",
                AddressFormatType.NeoAddress => "Neo address format but invalid checksum or encoding",
                AddressFormatType.Unknown => "Unknown address format",
                _ => "Invalid address format"
            };
        }
        
        /// <summary>
        /// Provides suggestions for fixing the address.
        /// </summary>
        /// <returns>List of suggestions</returns>
        public string GetSuggestion()
        {
            if (IsValid)
                return "Address is valid";
            
            if (string.IsNullOrEmpty(Address))
                return "Please provide an address";
            
            var format = GetAddressFormat();
            return format switch
            {
                AddressFormatType.ScriptHash => "Ensure the script hash starts with '0x' and is exactly 40 hex characters",
                AddressFormatType.NeoAddress => "Check that the Neo address uses the correct Base58 encoding and checksum",
                AddressFormatType.Unknown => "Use either Neo address format (starting with 'N') or script hash format (starting with '0x')",
                _ => "Please check the address format and try again"
            };
        }
        
        #endregion
        
        #region Static Validation Methods
        
        /// <summary>
        /// Creates a validation result for a valid address.
        /// </summary>
        /// <param name="address">The valid address</param>
        /// <returns>Valid address result</returns>
        public static AddressValidationResult CreateValid(string address)
        {
            return new AddressValidationResult(address, true);
        }
        
        /// <summary>
        /// Creates a validation result for an invalid address.
        /// </summary>
        /// <param name="address">The invalid address</param>
        /// <returns>Invalid address result</returns>
        public static AddressValidationResult CreateInvalid(string address)
        {
            return new AddressValidationResult(address, false);
        }
        
        /// <summary>
        /// Performs basic client-side validation without contacting a node.
        /// Note: This is a simplified check and may not catch all invalid addresses.
        /// </summary>
        /// <param name="address">The address to validate</param>
        /// <returns>Basic validation result</returns>
        public static AddressValidationResult ValidateBasic(string address)
        {
            if (string.IsNullOrEmpty(address))
                return CreateInvalid(address);
            
            // Basic Neo address format check (starts with 'N' and reasonable length)
            if (address.StartsWith("N") && address.Length >= 25 && address.Length <= 35)
            {
                // Could add more sophisticated Base58 validation here
                return CreateValid(address);
            }
            
            // Basic script hash format check (starts with '0x' and 40 hex chars)
            if (address.StartsWith("0x") && address.Length == 42)
            {
                // Check if remaining characters are valid hex
                var hexPart = address.Substring(2);
                if (IsValidHex(hexPart))
                    return CreateValid(address);
            }
            
            return CreateInvalid(address);
        }
        
        /// <summary>
        /// Checks if a string contains only valid hexadecimal characters.
        /// </summary>
        /// <param name="hex">The hex string to validate</param>
        /// <returns>True if valid hex</returns>
        private static bool IsValidHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return false;
            
            foreach (char c in hex)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this address validation result.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            // Address can be null or empty for invalid results, so no strict validation needed
            // The IsValid flag is the authoritative validation result
        }
        
        /// <summary>
        /// Throws an exception if the address is invalid.
        /// </summary>
        /// <exception cref="ArgumentException">If the address is invalid</exception>
        public void ThrowIfInvalid()
        {
            if (IsInvalid)
            {
                throw new ArgumentException($"Invalid Neo address: {Address}. {GetValidationError()}");
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this validation result.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"Address Validation Result:\n";
            result += $"  Address: {Address}\n";
            result += $"  Valid: {IsValid}\n";
            result += $"  Format: {GetAddressFormat()}\n";
            
            if (IsInvalid)
            {
                result += $"  Error: {GetValidationError()}\n";
                result += $"  Suggestion: {GetSuggestion()}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this validation result.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var status = IsValid ? "Valid" : "Invalid";
            var preview = Address?.Length > 10 ? Address.Substring(0, 10) + "..." : Address;
            return $"AddressValidation({preview}: {status})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Enumeration of address format types.
    /// </summary>
    public enum AddressFormatType
    {
        /// <summary>Empty or null address</summary>
        Empty,
        
        /// <summary>Neo address format (Base58)</summary>
        NeoAddress,
        
        /// <summary>Script hash format (0x...)</summary>
        ScriptHash,
        
        /// <summary>Unknown or unrecognized format</summary>
        Unknown
    }
}