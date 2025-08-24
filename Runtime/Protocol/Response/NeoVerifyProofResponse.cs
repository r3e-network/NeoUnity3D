using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the verifyproof RPC call.
    /// Returns the verification result for a proof, typically containing the proven value.
    /// </summary>
    [System.Serializable]
    public class NeoVerifyProofResponse : NeoResponse<string>
    {
        /// <summary>
        /// Gets the verification result from the response.
        /// </summary>
        public string VerificationResult => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoVerifyProofResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful verify proof response.
        /// </summary>
        /// <param name="verificationResult">The verification result</param>
        /// <param name="id">The request ID</param>
        public NeoVerifyProofResponse(string verificationResult, int id = 1) : base(verificationResult, id)
        {
        }

        /// <summary>
        /// Creates an error verify proof response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoVerifyProofResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Checks if the response contains a valid verification result.
        /// </summary>
        [JsonIgnore]
        public bool HasVerificationResult => !string.IsNullOrEmpty(VerificationResult);

        /// <summary>
        /// Checks if the proof verification was successful.
        /// An empty or null result typically indicates the proof was invalid.
        /// </summary>
        [JsonIgnore]
        public bool IsProofValid => HasVerificationResult;

        /// <summary>
        /// Gets the verification result in uppercase format.
        /// </summary>
        [JsonIgnore]
        public string VerificationResultUppercase => VerificationResult?.ToUpperInvariant() ?? string.Empty;

        /// <summary>
        /// Gets the verification result in lowercase format.
        /// </summary>
        [JsonIgnore]
        public string VerificationResultLowercase => VerificationResult?.ToLowerInvariant() ?? string.Empty;

        /// <summary>
        /// Gets the verification result with '0x' prefix if it doesn't already have one.
        /// </summary>
        [JsonIgnore]
        public string VerificationResultWithPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(VerificationResult))
                    return string.Empty;

                return VerificationResult.StartsWith("0x") ? VerificationResult : $"0x{VerificationResult}";
            }
        }

        /// <summary>
        /// Gets the verification result without '0x' prefix if it has one.
        /// </summary>
        [JsonIgnore]
        public string VerificationResultWithoutPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(VerificationResult))
                    return string.Empty;

                return VerificationResult.StartsWith("0x") ? VerificationResult.Substring(2) : VerificationResult;
            }
        }

        /// <summary>
        /// Gets the length of the verification result string (without prefix).
        /// </summary>
        [JsonIgnore]
        public int VerificationResultLength => VerificationResultWithoutPrefix.Length;

        /// <summary>
        /// Validates that the verification result has a valid hexadecimal format.
        /// </summary>
        /// <returns>True if the verification result format is valid hex</returns>
        public bool IsValidHexFormat()
        {
            if (string.IsNullOrEmpty(VerificationResult))
                return false;

            var resultToCheck = VerificationResultWithoutPrefix;

            // Check if all characters are valid hexadecimal
            foreach (char c in resultToCheck)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a shortened version of the verification result for display purposes.
        /// </summary>
        /// <param name="prefixLength">Length of prefix to show (default: 16)</param>
        /// <param name="suffixLength">Length of suffix to show (default: 16)</param>
        /// <returns>Shortened verification result string</returns>
        public string GetShortVerificationResult(int prefixLength = 16, int suffixLength = 16)
        {
            if (string.IsNullOrEmpty(VerificationResult))
                return string.Empty;

            var resultToUse = VerificationResultWithoutPrefix;

            if (resultToUse.Length <= prefixLength + suffixLength)
                return resultToUse;

            if (prefixLength <= 0 && suffixLength <= 0)
                return "...";

            if (prefixLength <= 0)
                return $"...{resultToUse.Substring(resultToUse.Length - suffixLength)}";

            if (suffixLength <= 0)
                return $"{resultToUse.Substring(0, prefixLength)}...";

            return $"{resultToUse.Substring(0, prefixLength)}...{resultToUse.Substring(resultToUse.Length - suffixLength)}";
        }

        /// <summary>
        /// Attempts to decode the verification result as a Base64 string.
        /// </summary>
        /// <returns>Decoded bytes if successful, null otherwise</returns>
        public byte[] TryDecodeAsBase64()
        {
            try
            {
                if (string.IsNullOrEmpty(VerificationResult))
                    return null;

                return Convert.FromBase64String(VerificationResult);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to decode the verification result as a hexadecimal string.
        /// </summary>
        /// <returns>Decoded bytes if successful, null otherwise</returns>
        public byte[] TryDecodeAsHex()
        {
            try
            {
                if (string.IsNullOrEmpty(VerificationResult))
                    return null;

                var hexString = VerificationResultWithoutPrefix;
                if (hexString.Length % 2 != 0)
                    return null;

                byte[] bytes = new byte[hexString.Length / 2];
                for (int i = 0; i < hexString.Length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                }

                return bytes;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to decode the verification result as a UTF-8 string.
        /// </summary>
        /// <returns>Decoded string if successful, null otherwise</returns>
        public string TryDecodeAsUtf8String()
        {
            try
            {
                var bytes = TryDecodeAsHex();
                if (bytes == null)
                    return null;

                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the estimated size of the verification result in bytes.
        /// </summary>
        [JsonIgnore]
        public int EstimatedSizeBytes
        {
            get
            {
                if (string.IsNullOrEmpty(VerificationResult))
                    return 0;

                // Try hex first (most common for blockchain data)
                if (IsValidHexFormat())
                {
                    return VerificationResultLength / 2; // 2 hex characters = 1 byte
                }

                // Fallback to character count
                return VerificationResult.Length;
            }
        }

        /// <summary>
        /// Returns a string representation of the verify proof response.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (HasError)
                return base.ToString();

            if (!IsProofValid)
                return "NeoVerifyProofResponse(Proof Invalid)";

            return $"NeoVerifyProofResponse(Valid: {IsProofValid}, Result: {GetShortVerificationResult()}, Size: {EstimatedSizeBytes} bytes)";
        }

        /// <summary>
        /// Gets detailed information about the verification result.
        /// </summary>
        /// <returns>Detailed description</returns>
        public string GetDetailedInfo()
        {
            if (!HasVerificationResult)
                return "No verification result available (proof may be invalid)";

            var decodedUtf8 = TryDecodeAsUtf8String();
            var decodedInfo = !string.IsNullOrEmpty(decodedUtf8) ? $"\n  Decoded UTF-8: {decodedUtf8}" : "";

            return $"Verification Result Information:\n" +
                   $"  Is Valid: {IsProofValid}\n" +
                   $"  Length: {VerificationResultLength} characters\n" +
                   $"  Estimated Size: {EstimatedSizeBytes} bytes\n" +
                   $"  Valid Hex Format: {IsValidHexFormat()}\n" +
                   $"  Preview: {GetShortVerificationResult()}" +
                   decodedInfo;
        }

        /// <summary>
        /// Validates the verification result.
        /// </summary>
        /// <exception cref="ArgumentException">If the verification result format is invalid</exception>
        public void ValidateVerificationResult()
        {
            // Note: An empty verification result is valid (indicates invalid proof)
            // Additional validation can be added here based on specific requirements
        }

        /// <summary>
        /// Compares this verification result with another.
        /// </summary>
        /// <param name="other">The other verify proof response to compare with</param>
        /// <returns>True if the verification results are equal</returns>
        public bool VerificationEquals(NeoVerifyProofResponse other)
        {
            if (other == null)
                return false;

            if (VerificationResult == null && other.VerificationResult == null)
                return true;

            if (VerificationResult == null || other.VerificationResult == null)
                return false;

            return string.Equals(VerificationResultWithoutPrefix, other.VerificationResultWithoutPrefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a copy of this response with a new verification result.
        /// </summary>
        /// <param name="newVerificationResult">The new verification result</param>
        /// <returns>New NeoVerifyProofResponse instance</returns>
        public NeoVerifyProofResponse WithVerificationResult(string newVerificationResult)
        {
            return new NeoVerifyProofResponse(newVerificationResult, Id);
        }

        /// <summary>
        /// Gets the verification status as a user-friendly string.
        /// </summary>
        [JsonIgnore]
        public string VerificationStatus
        {
            get
            {
                if (HasError)
                    return "Error";

                if (!HasVerificationResult)
                    return "Invalid Proof";

                return "Valid Proof";
            }
        }
    }
}