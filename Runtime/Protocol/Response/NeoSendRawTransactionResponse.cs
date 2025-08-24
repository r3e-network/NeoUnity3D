using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the sendrawtransaction RPC call.
    /// Returns the transaction hash if the transaction was successfully submitted to the network.
    /// </summary>
    [System.Serializable]
    public class NeoSendRawTransactionResponse : NeoResponse<NeoSendRawTransactionResponse.RawTransactionResult>
    {
        /// <summary>
        /// Gets the raw transaction result from the response.
        /// </summary>
        public RawTransactionResult RawTransactionResult => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoSendRawTransactionResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful send raw transaction response.
        /// </summary>
        /// <param name="rawTransactionResult">The raw transaction result</param>
        /// <param name="id">The request ID</param>
        public NeoSendRawTransactionResponse(RawTransactionResult rawTransactionResult, int id = 1) : base(rawTransactionResult, id)
        {
        }

        /// <summary>
        /// Creates an error send raw transaction response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoSendRawTransactionResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Contains the result of sending a raw transaction to the network.
        /// </summary>
        [System.Serializable]
        public class RawTransactionResult
        {
            /// <summary>The hash of the submitted transaction</summary>
            [JsonProperty("hash")]
            public string Hash { get; set; }

            /// <summary>
            /// Default constructor for JSON deserialization.
            /// </summary>
            public RawTransactionResult()
            {
            }

            /// <summary>
            /// Creates a new raw transaction result.
            /// </summary>
            /// <param name="hash">The transaction hash</param>
            public RawTransactionResult(string hash)
            {
                Hash = hash;
            }

            /// <summary>
            /// Checks if the transaction hash is valid (not null or empty).
            /// </summary>
            [JsonIgnore]
            public bool HasValidHash => !string.IsNullOrEmpty(Hash);

            /// <summary>
            /// Gets the transaction hash in uppercase format.
            /// </summary>
            [JsonIgnore]
            public string HashUppercase => Hash?.ToUpperInvariant() ?? string.Empty;

            /// <summary>
            /// Gets the transaction hash in lowercase format.
            /// </summary>
            [JsonIgnore]
            public string HashLowercase => Hash?.ToLowerInvariant() ?? string.Empty;

            /// <summary>
            /// Gets the transaction hash with '0x' prefix.
            /// </summary>
            [JsonIgnore]
            public string HashWithPrefix
            {
                get
                {
                    if (string.IsNullOrEmpty(Hash))
                        return string.Empty;

                    return Hash.StartsWith("0x") ? Hash : $"0x{Hash}";
                }
            }

            /// <summary>
            /// Gets the transaction hash without '0x' prefix.
            /// </summary>
            [JsonIgnore]
            public string HashWithoutPrefix
            {
                get
                {
                    if (string.IsNullOrEmpty(Hash))
                        return string.Empty;

                    return Hash.StartsWith("0x") ? Hash.Substring(2) : Hash;
                }
            }

            /// <summary>
            /// Validates that the transaction hash has the correct format.
            /// Neo transaction hashes are 256-bit values represented as 64-character hexadecimal strings.
            /// </summary>
            /// <returns>True if the hash format is valid</returns>
            public bool IsValidHashFormat()
            {
                if (string.IsNullOrEmpty(Hash))
                    return false;

                var hashToCheck = HashWithoutPrefix;

                // Transaction hashes should be exactly 64 characters (256 bits in hex)
                if (hashToCheck.Length != 64)
                    return false;

                // Check if all characters are valid hexadecimal
                foreach (char c in hashToCheck)
                {
                    if (!Uri.IsHexDigit(c))
                        return false;
                }

                return true;
            }

            /// <summary>
            /// Gets a shortened version of the hash for display purposes.
            /// </summary>
            /// <param name="prefixLength">Length of prefix to show (default: 8)</param>
            /// <param name="suffixLength">Length of suffix to show (default: 8)</param>
            /// <returns>Shortened hash string</returns>
            public string GetShortHash(int prefixLength = 8, int suffixLength = 8)
            {
                if (string.IsNullOrEmpty(Hash))
                    return string.Empty;

                var hashToUse = HashWithoutPrefix;

                if (hashToUse.Length <= prefixLength + suffixLength)
                    return hashToUse;

                if (prefixLength <= 0 && suffixLength <= 0)
                    return "...";

                if (prefixLength <= 0)
                    return $"...{hashToUse.Substring(hashToUse.Length - suffixLength)}";

                if (suffixLength <= 0)
                    return $"{hashToUse.Substring(0, prefixLength)}...";

                return $"{hashToUse.Substring(0, prefixLength)}...{hashToUse.Substring(hashToUse.Length - suffixLength)}";
            }

            /// <summary>
            /// Returns a string representation of the raw transaction result.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"RawTransactionResult(Hash: {GetShortHash()})";
            }

            /// <summary>
            /// Validates the raw transaction result.
            /// </summary>
            /// <exception cref="ArgumentException">If the result is invalid</exception>
            public void Validate()
            {
                if (string.IsNullOrEmpty(Hash))
                    throw new ArgumentException("Transaction hash cannot be null or empty");

                if (!IsValidHashFormat())
                    throw new ArgumentException($"Invalid transaction hash format: {Hash}");
            }

            /// <summary>
            /// Compares this transaction result with another based on hash.
            /// </summary>
            /// <param name="other">The other result to compare with</param>
            /// <returns>True if the hashes are equal</returns>
            public bool Equals(RawTransactionResult other)
            {
                if (other == null)
                    return false;

                return string.Equals(HashWithoutPrefix, other.HashWithoutPrefix, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Gets the hash code based on the transaction hash.
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
            {
                return HashWithoutPrefix?.ToLowerInvariant().GetHashCode() ?? 0;
            }

            /// <summary>
            /// Creates a copy of this result with a new hash.
            /// </summary>
            /// <param name="newHash">The new transaction hash</param>
            /// <returns>New RawTransactionResult instance</returns>
            public RawTransactionResult WithHash(string newHash)
            {
                return new RawTransactionResult(newHash);
            }
        }

        /// <summary>
        /// Convenience property to get the transaction hash directly.
        /// </summary>
        [JsonIgnore]
        public string TransactionHash => RawTransactionResult?.Hash;

        /// <summary>
        /// Checks if the response contains a valid transaction hash.
        /// </summary>
        [JsonIgnore]
        public bool HasValidTransactionHash => RawTransactionResult?.HasValidHash == true && RawTransactionResult.IsValidHashFormat();
    }
}