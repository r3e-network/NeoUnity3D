using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the findstates RPC call.
    /// Returns a list of state key-value pairs matching the search criteria.
    /// </summary>
    [System.Serializable]
    public class NeoFindStatesResponse : NeoResponse<NeoFindStatesResponse.States>
    {
        /// <summary>
        /// Gets the states information from the response.
        /// </summary>
        public States StatesInfo => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoFindStatesResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful find states response.
        /// </summary>
        /// <param name="states">The states information</param>
        /// <param name="id">The request ID</param>
        public NeoFindStatesResponse(States states, int id = 1) : base(states, id)
        {
        }

        /// <summary>
        /// Creates an error find states response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoFindStatesResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Contains state search results with pagination information.
        /// </summary>
        [System.Serializable]
        public class States
        {
            /// <summary>Proof for the first result (used for pagination)</summary>
            [JsonProperty("firstproof")]
            public string FirstProof { get; set; }

            /// <summary>Proof for the last result (used for pagination)</summary>
            [JsonProperty("lastproof")]
            public string LastProof { get; set; }

            /// <summary>Whether the results were truncated due to limits</summary>
            [JsonProperty("truncated")]
            public bool Truncated { get; set; }

            /// <summary>List of state key-value pairs found</summary>
            [JsonProperty("results")]
            public List<StateResult> Results { get; set; } = new List<StateResult>();

            /// <summary>
            /// Default constructor for JSON deserialization.
            /// </summary>
            public States()
            {
            }

            /// <summary>
            /// Creates new states search results.
            /// </summary>
            /// <param name="firstProof">The first proof</param>
            /// <param name="lastProof">The last proof</param>
            /// <param name="truncated">Whether results were truncated</param>
            /// <param name="results">The list of results</param>
            public States(string firstProof, string lastProof, bool truncated, List<StateResult> results)
            {
                FirstProof = firstProof;
                LastProof = lastProof;
                Truncated = truncated;
                Results = results ?? new List<StateResult>();
            }

            /// <summary>
            /// Gets the number of state results found.
            /// </summary>
            [JsonIgnore]
            public int ResultCount => Results?.Count ?? 0;

            /// <summary>
            /// Checks if any results were found.
            /// </summary>
            [JsonIgnore]
            public bool HasResults => ResultCount > 0;

            /// <summary>
            /// Checks if pagination proofs are available.
            /// </summary>
            [JsonIgnore]
            public bool HasPaginationProofs => !string.IsNullOrEmpty(FirstProof) || !string.IsNullOrEmpty(LastProof);

            /// <summary>
            /// Checks if there are potentially more results available (pagination needed).
            /// </summary>
            [JsonIgnore]
            public bool HasMoreResults => Truncated;

            /// <summary>
            /// Gets all the keys from the results.
            /// </summary>
            [JsonIgnore]
            public List<string> Keys
            {
                get
                {
                    return Results?.Select(r => r.Key).Where(k => !string.IsNullOrEmpty(k)).ToList() ?? new List<string>();
                }
            }

            /// <summary>
            /// Gets all the values from the results.
            /// </summary>
            [JsonIgnore]
            public List<string> Values
            {
                get
                {
                    return Results?.Select(r => r.Value).Where(v => !string.IsNullOrEmpty(v)).ToList() ?? new List<string>();
                }
            }

            /// <summary>
            /// Finds a result by key.
            /// </summary>
            /// <param name="key">The key to search for</param>
            /// <returns>The state result if found, null otherwise</returns>
            public StateResult FindByKey(string key)
            {
                if (string.IsNullOrEmpty(key) || Results == null)
                    return null;

                return Results.FirstOrDefault(r => 
                    string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase));
            }

            /// <summary>
            /// Gets results containing a specific key pattern.
            /// </summary>
            /// <param name="keyPattern">The pattern to search for in keys</param>
            /// <returns>List of matching results</returns>
            public List<StateResult> FindByKeyPattern(string keyPattern)
            {
                if (string.IsNullOrEmpty(keyPattern) || Results == null)
                    return new List<StateResult>();

                return Results.Where(r => 
                    !string.IsNullOrEmpty(r.Key) && 
                    r.Key.ToLowerInvariant().Contains(keyPattern.ToLowerInvariant())).ToList();
            }

            /// <summary>
            /// Returns a string representation of the states.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"States(Results: {ResultCount}, Truncated: {Truncated}, HasProofs: {HasPaginationProofs})";
            }

            /// <summary>
            /// Gets detailed information about the states search results.
            /// </summary>
            /// <returns>Detailed description</returns>
            public string GetDetailedInfo()
            {
                var firstProofInfo = !string.IsNullOrEmpty(FirstProof) ? $"Yes ({FirstProof.Substring(0, Math.Min(16, FirstProof.Length))}...)" : "No";
                var lastProofInfo = !string.IsNullOrEmpty(LastProof) ? $"Yes ({LastProof.Substring(0, Math.Min(16, LastProof.Length))}...)" : "No";

                return $"Find States Results:\n" +
                       $"  Total Results: {ResultCount}\n" +
                       $"  Truncated: {Truncated}\n" +
                       $"  First Proof: {firstProofInfo}\n" +
                       $"  Last Proof: {lastProofInfo}\n" +
                       $"  Has More Results: {HasMoreResults}";
            }

            /// <summary>
            /// Validates the states information.
            /// </summary>
            /// <exception cref="ArgumentException">If the states information is invalid</exception>
            public void Validate()
            {
                if (Results == null)
                    throw new ArgumentException("Results list cannot be null");

                // Validate each result
                foreach (var result in Results)
                {
                    result?.Validate();
                }
            }
        }

        /// <summary>
        /// Represents a single state key-value pair result.
        /// </summary>
        [System.Serializable]
        public class StateResult
        {
            /// <summary>The state key</summary>
            [JsonProperty("key")]
            public string Key { get; set; }

            /// <summary>The state value</summary>
            [JsonProperty("value")]
            public string Value { get; set; }

            /// <summary>
            /// Default constructor for JSON deserialization.
            /// </summary>
            public StateResult()
            {
            }

            /// <summary>
            /// Creates a new state result.
            /// </summary>
            /// <param name="key">The state key</param>
            /// <param name="value">The state value</param>
            public StateResult(string key, string value)
            {
                Key = key;
                Value = value;
            }

            /// <summary>
            /// Checks if this result has valid data.
            /// </summary>
            [JsonIgnore]
            public bool IsValid => !string.IsNullOrEmpty(Key);

            /// <summary>
            /// Checks if this result has a value.
            /// </summary>
            [JsonIgnore]
            public bool HasValue => !string.IsNullOrEmpty(Value);

            /// <summary>
            /// Gets the key without '0x' prefix if it has one.
            /// </summary>
            [JsonIgnore]
            public string KeyWithoutPrefix
            {
                get
                {
                    if (string.IsNullOrEmpty(Key))
                        return string.Empty;

                    return Key.StartsWith("0x") ? Key.Substring(2) : Key;
                }
            }

            /// <summary>
            /// Gets the value without '0x' prefix if it has one.
            /// </summary>
            [JsonIgnore]
            public string ValueWithoutPrefix
            {
                get
                {
                    if (string.IsNullOrEmpty(Value))
                        return string.Empty;

                    return Value.StartsWith("0x") ? Value.Substring(2) : Value;
                }
            }

            /// <summary>
            /// Gets a shortened version of the key for display purposes.
            /// </summary>
            /// <param name="maxLength">Maximum length to display (default: 32)</param>
            /// <returns>Shortened key string</returns>
            public string GetShortKey(int maxLength = 32)
            {
                if (string.IsNullOrEmpty(Key))
                    return string.Empty;

                var keyToUse = KeyWithoutPrefix;

                if (keyToUse.Length <= maxLength)
                    return keyToUse;

                var prefixLength = maxLength / 2 - 2;
                var suffixLength = maxLength - prefixLength - 3; // 3 for "..."

                return $"{keyToUse.Substring(0, prefixLength)}...{keyToUse.Substring(keyToUse.Length - suffixLength)}";
            }

            /// <summary>
            /// Gets a shortened version of the value for display purposes.
            /// </summary>
            /// <param name="maxLength">Maximum length to display (default: 32)</param>
            /// <returns>Shortened value string</returns>
            public string GetShortValue(int maxLength = 32)
            {
                if (string.IsNullOrEmpty(Value))
                    return string.Empty;

                var valueToUse = ValueWithoutPrefix;

                if (valueToUse.Length <= maxLength)
                    return valueToUse;

                var prefixLength = maxLength / 2 - 2;
                var suffixLength = maxLength - prefixLength - 3; // 3 for "..."

                return $"{valueToUse.Substring(0, prefixLength)}...{valueToUse.Substring(valueToUse.Length - suffixLength)}";
            }

            /// <summary>
            /// Attempts to decode the key as hexadecimal.
            /// </summary>
            /// <returns>Decoded bytes if successful, null otherwise</returns>
            public byte[] TryDecodeKeyAsHex()
            {
                try
                {
                    if (string.IsNullOrEmpty(Key))
                        return null;

                    var hexString = KeyWithoutPrefix;
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
            /// Attempts to decode the value as hexadecimal.
            /// </summary>
            /// <returns>Decoded bytes if successful, null otherwise</returns>
            public byte[] TryDecodeValueAsHex()
            {
                try
                {
                    if (string.IsNullOrEmpty(Value))
                        return null;

                    var hexString = ValueWithoutPrefix;
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
            /// Attempts to decode the value as a UTF-8 string.
            /// </summary>
            /// <returns>Decoded string if successful, null otherwise</returns>
            public string TryDecodeValueAsUtf8String()
            {
                try
                {
                    var bytes = TryDecodeValueAsHex();
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
            /// Returns a string representation of the state result.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"StateResult(Key: {GetShortKey()}, Value: {GetShortValue()})";
            }

            /// <summary>
            /// Validates the state result.
            /// </summary>
            /// <exception cref="ArgumentException">If the result is invalid</exception>
            public void Validate()
            {
                if (string.IsNullOrEmpty(Key))
                    throw new ArgumentException("State key cannot be null or empty");

                // Value can be null or empty (indicates no value for the key)
            }

            /// <summary>
            /// Compares this result with another based on key.
            /// </summary>
            /// <param name="other">The other result to compare with</param>
            /// <returns>True if the keys are equal</returns>
            public bool KeyEquals(StateResult other)
            {
                if (other == null)
                    return false;

                return string.Equals(KeyWithoutPrefix, other.KeyWithoutPrefix, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Gets the hash code based on the key.
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
            {
                return KeyWithoutPrefix?.ToLowerInvariant().GetHashCode() ?? 0;
            }
        }

        /// <summary>
        /// Convenience property to get the results list directly.
        /// </summary>
        [JsonIgnore]
        public List<StateResult> Results => StatesInfo?.Results;

        /// <summary>
        /// Convenience property to check if results were truncated.
        /// </summary>
        [JsonIgnore]
        public bool WereResultsTruncated => StatesInfo?.Truncated == true;

        /// <summary>
        /// Convenience property to get the result count.
        /// </summary>
        [JsonIgnore]
        public int ResultCount => StatesInfo?.ResultCount ?? 0;
    }
}