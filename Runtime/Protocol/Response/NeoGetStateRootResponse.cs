using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the getstateroot RPC call.
    /// Returns the state root information for a specific block.
    /// </summary>
    [System.Serializable]
    public class NeoGetStateRootResponse : NeoResponse<NeoGetStateRootResponse.StateRoot>
    {
        /// <summary>
        /// Gets the state root information from the response.
        /// </summary>
        public StateRoot StateRoot => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoGetStateRootResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful state root response.
        /// </summary>
        /// <param name="stateRoot">The state root information</param>
        /// <param name="id">The request ID</param>
        public NeoGetStateRootResponse(StateRoot stateRoot, int id = 1) : base(stateRoot, id)
        {
        }

        /// <summary>
        /// Creates an error state root response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoGetStateRootResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Contains state root information for a specific block.
        /// </summary>
        [System.Serializable]
        public class StateRoot
        {
            /// <summary>The version of the state root format</summary>
            [JsonProperty("version")]
            public int Version { get; set; }

            /// <summary>The block index (height) for this state root</summary>
            [JsonProperty("index")]
            public int Index { get; set; }

            /// <summary>The Merkle root hash of the state tree</summary>
            [JsonProperty("roothash")]
            public string RootHash { get; set; }

            /// <summary>The list of witnesses that validated this state root</summary>
            [JsonProperty("witnesses")]
            public List<NeoWitness> Witnesses { get; set; } = new List<NeoWitness>();

            /// <summary>
            /// Default constructor for JSON deserialization.
            /// </summary>
            public StateRoot()
            {
            }

            /// <summary>
            /// Creates a new state root information object.
            /// </summary>
            /// <param name="version">The state root version</param>
            /// <param name="index">The block index</param>
            /// <param name="rootHash">The root hash</param>
            /// <param name="witnesses">The list of witnesses</param>
            public StateRoot(int version, int index, string rootHash, List<NeoWitness> witnesses)
            {
                Version = version;
                Index = index;
                RootHash = rootHash;
                Witnesses = witnesses ?? new List<NeoWitness>();
            }

            /// <summary>
            /// Gets the block height (same as Index).
            /// </summary>
            [JsonIgnore]
            public int BlockHeight => Index;

            /// <summary>
            /// Gets the number of witnesses that validated this state root.
            /// </summary>
            [JsonIgnore]
            public int WitnessCount => Witnesses?.Count ?? 0;

            /// <summary>
            /// Checks if the state root has any witnesses.
            /// </summary>
            [JsonIgnore]
            public bool HasWitnesses => WitnessCount > 0;

            /// <summary>
            /// Gets the root hash in uppercase format.
            /// </summary>
            [JsonIgnore]
            public string RootHashUppercase => RootHash?.ToUpperInvariant() ?? string.Empty;

            /// <summary>
            /// Gets the root hash in lowercase format.
            /// </summary>
            [JsonIgnore]
            public string RootHashLowercase => RootHash?.ToLowerInvariant() ?? string.Empty;

            /// <summary>
            /// Gets the root hash with '0x' prefix.
            /// </summary>
            [JsonIgnore]
            public string RootHashWithPrefix
            {
                get
                {
                    if (string.IsNullOrEmpty(RootHash))
                        return string.Empty;

                    return RootHash.StartsWith("0x") ? RootHash : $"0x{RootHash}";
                }
            }

            /// <summary>
            /// Gets the root hash without '0x' prefix.
            /// </summary>
            [JsonIgnore]
            public string RootHashWithoutPrefix
            {
                get
                {
                    if (string.IsNullOrEmpty(RootHash))
                        return string.Empty;

                    return RootHash.StartsWith("0x") ? RootHash.Substring(2) : RootHash;
                }
            }

            /// <summary>
            /// Validates that the root hash has the correct format.
            /// </summary>
            /// <returns>True if the root hash format is valid</returns>
            public bool IsValidRootHashFormat()
            {
                if (string.IsNullOrEmpty(RootHash))
                    return false;

                var hashToCheck = RootHashWithoutPrefix;

                // Root hashes should be exactly 64 characters (256 bits in hex)
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
            /// Gets a shortened version of the root hash for display purposes.
            /// </summary>
            /// <param name="prefixLength">Length of prefix to show (default: 8)</param>
            /// <param name="suffixLength">Length of suffix to show (default: 8)</param>
            /// <returns>Shortened hash string</returns>
            public string GetShortRootHash(int prefixLength = 8, int suffixLength = 8)
            {
                if (string.IsNullOrEmpty(RootHash))
                    return string.Empty;

                var hashToUse = RootHashWithoutPrefix;

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
            /// Checks if this state root is for the genesis block.
            /// </summary>
            [JsonIgnore]
            public bool IsGenesisBlock => Index == 0;

            /// <summary>
            /// Gets the witness signatures as a list of strings.
            /// </summary>
            [JsonIgnore]
            public List<string> WitnessSignatures
            {
                get
                {
                    var signatures = new List<string>();
                    if (Witnesses != null)
                    {
                        foreach (var witness in Witnesses)
                        {
                            if (!string.IsNullOrEmpty(witness.InvocationScript))
                                signatures.Add(witness.InvocationScript);
                        }
                    }
                    return signatures;
                }
            }

            /// <summary>
            /// Returns a string representation of the state root.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"StateRoot(Version: {Version}, Index: {Index}, RootHash: {GetShortRootHash()}, Witnesses: {WitnessCount})";
            }

            /// <summary>
            /// Gets a detailed description of the state root.
            /// </summary>
            /// <returns>Detailed description</returns>
            public string GetDetailedInfo()
            {
                return $"State Root Information:\n" +
                       $"  Version: {Version}\n" +
                       $"  Block Index: {Index}\n" +
                       $"  Root Hash: {RootHash}\n" +
                       $"  Witnesses: {WitnessCount}\n" +
                       $"  Is Genesis: {IsGenesisBlock}";
            }

            /// <summary>
            /// Validates the state root information.
            /// </summary>
            /// <exception cref="ArgumentException">If the state root information is invalid</exception>
            public void Validate()
            {
                if (Version < 0)
                    throw new ArgumentException("Version cannot be negative");

                if (Index < 0)
                    throw new ArgumentException("Index cannot be negative");

                if (string.IsNullOrEmpty(RootHash))
                    throw new ArgumentException("Root hash cannot be null or empty");

                if (!IsValidRootHashFormat())
                    throw new ArgumentException($"Invalid root hash format: {RootHash}");

                if (Witnesses == null)
                    throw new ArgumentException("Witnesses list cannot be null");

                // Validate each witness
                foreach (var witness in Witnesses)
                {
                    witness?.Validate();
                }
            }

            /// <summary>
            /// Compares this state root with another based on index and root hash.
            /// </summary>
            /// <param name="other">The other state root to compare with</param>
            /// <returns>True if they represent the same state</returns>
            public bool Equals(StateRoot other)
            {
                if (other == null)
                    return false;

                return Index == other.Index && 
                       string.Equals(RootHashWithoutPrefix, other.RootHashWithoutPrefix, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Gets the hash code based on index and root hash.
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
            {
                var hashCode = Index.GetHashCode();
                if (!string.IsNullOrEmpty(RootHash))
                    hashCode ^= RootHashWithoutPrefix.ToLowerInvariant().GetHashCode();
                return hashCode;
            }
        }
    }
}