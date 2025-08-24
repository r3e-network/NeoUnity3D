using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the getproof RPC call.
    /// Returns a proof for a specific key in the state tree.
    /// </summary>
    [System.Serializable]
    public class NeoGetProofResponse : NeoResponse<string>
    {
        /// <summary>
        /// Gets the proof string from the response.
        /// </summary>
        public string Proof => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoGetProofResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful get proof response.
        /// </summary>
        /// <param name="proof">The proof string</param>
        /// <param name="id">The request ID</param>
        public NeoGetProofResponse(string proof, int id = 1) : base(proof, id)
        {
        }

        /// <summary>
        /// Creates an error get proof response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoGetProofResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Checks if the response contains a valid proof.
        /// </summary>
        [JsonIgnore]
        public bool HasValidProof => !string.IsNullOrEmpty(Proof);

        /// <summary>
        /// Gets the proof in uppercase format.
        /// </summary>
        [JsonIgnore]
        public string ProofUppercase => Proof?.ToUpperInvariant() ?? string.Empty;

        /// <summary>
        /// Gets the proof in lowercase format.
        /// </summary>
        [JsonIgnore]
        public string ProofLowercase => Proof?.ToLowerInvariant() ?? string.Empty;

        /// <summary>
        /// Gets the proof with '0x' prefix if it doesn't already have one.
        /// </summary>
        [JsonIgnore]
        public string ProofWithPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(Proof))
                    return string.Empty;

                return Proof.StartsWith("0x") ? Proof : $"0x{Proof}";
            }
        }

        /// <summary>
        /// Gets the proof without '0x' prefix if it has one.
        /// </summary>
        [JsonIgnore]
        public string ProofWithoutPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(Proof))
                    return string.Empty;

                return Proof.StartsWith("0x") ? Proof.Substring(2) : Proof;
            }
        }

        /// <summary>
        /// Gets the length of the proof string (without prefix).
        /// </summary>
        [JsonIgnore]
        public int ProofLength => ProofWithoutPrefix.Length;

        /// <summary>
        /// Validates that the proof has a valid hexadecimal format.
        /// </summary>
        /// <returns>True if the proof format is valid</returns>
        public bool IsValidProofFormat()
        {
            if (string.IsNullOrEmpty(Proof))
                return false;

            var proofToCheck = ProofWithoutPrefix;

            // Check if all characters are valid hexadecimal
            foreach (char c in proofToCheck)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a shortened version of the proof for display purposes.
        /// </summary>
        /// <param name="prefixLength">Length of prefix to show (default: 16)</param>
        /// <param name="suffixLength">Length of suffix to show (default: 16)</param>
        /// <returns>Shortened proof string</returns>
        public string GetShortProof(int prefixLength = 16, int suffixLength = 16)
        {
            if (string.IsNullOrEmpty(Proof))
                return string.Empty;

            var proofToUse = ProofWithoutPrefix;

            if (proofToUse.Length <= prefixLength + suffixLength)
                return proofToUse;

            if (prefixLength <= 0 && suffixLength <= 0)
                return "...";

            if (prefixLength <= 0)
                return $"...{proofToUse.Substring(proofToUse.Length - suffixLength)}";

            if (suffixLength <= 0)
                return $"{proofToUse.Substring(0, prefixLength)}...";

            return $"{proofToUse.Substring(0, prefixLength)}...{proofToUse.Substring(proofToUse.Length - suffixLength)}";
        }

        /// <summary>
        /// Attempts to decode the proof as a Base64 string.
        /// </summary>
        /// <returns>Decoded bytes if successful, null otherwise</returns>
        public byte[] TryDecodeAsBase64()
        {
            try
            {
                if (string.IsNullOrEmpty(Proof))
                    return null;

                return Convert.FromBase64String(Proof);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to decode the proof as a hexadecimal string.
        /// </summary>
        /// <returns>Decoded bytes if successful, null otherwise</returns>
        public byte[] TryDecodeAsHex()
        {
            try
            {
                if (string.IsNullOrEmpty(Proof))
                    return null;

                var hexString = ProofWithoutPrefix;
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
        /// Gets the estimated size of the proof in bytes.
        /// </summary>
        [JsonIgnore]
        public int EstimatedSizeBytes
        {
            get
            {
                if (string.IsNullOrEmpty(Proof))
                    return 0;

                // Try hex first (most common for blockchain proofs)
                if (IsValidProofFormat())
                {
                    return ProofLength / 2; // 2 hex characters = 1 byte
                }

                // Fallback to character count (might be Base64 or other encoding)
                return Proof.Length;
            }
        }

        /// <summary>
        /// Returns a string representation of the get proof response.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (HasError)
                return base.ToString();

            return $"NeoGetProofResponse(Proof: {GetShortProof()}, Size: {EstimatedSizeBytes} bytes)";
        }

        /// <summary>
        /// Gets detailed information about the proof.
        /// </summary>
        /// <returns>Detailed description</returns>
        public string GetDetailedInfo()
        {
            if (!HasValidProof)
                return "No proof available";

            return $"Proof Information:\n" +
                   $"  Length: {ProofLength} characters\n" +
                   $"  Estimated Size: {EstimatedSizeBytes} bytes\n" +
                   $"  Valid Hex Format: {IsValidProofFormat()}\n" +
                   $"  Preview: {GetShortProof()}";
        }

        /// <summary>
        /// Validates the proof information.
        /// </summary>
        /// <exception cref="ArgumentException">If the proof is invalid</exception>
        public void ValidateProof()
        {
            if (string.IsNullOrEmpty(Proof))
                throw new ArgumentException("Proof cannot be null or empty");

            // Additional validation can be added here based on specific proof requirements
        }

        /// <summary>
        /// Compares this proof with another.
        /// </summary>
        /// <param name="other">The other proof response to compare with</param>
        /// <returns>True if the proofs are equal</returns>
        public bool ProofEquals(NeoGetProofResponse other)
        {
            if (other == null)
                return false;

            if (Proof == null && other.Proof == null)
                return true;

            if (Proof == null || other.Proof == null)
                return false;

            return string.Equals(ProofWithoutPrefix, other.ProofWithoutPrefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a copy of this response with a new proof.
        /// </summary>
        /// <param name="newProof">The new proof string</param>
        /// <returns>New NeoGetProofResponse instance</returns>
        public NeoGetProofResponse WithProof(string newProof)
        {
            return new NeoGetProofResponse(newProof, Id);
        }
    }
}