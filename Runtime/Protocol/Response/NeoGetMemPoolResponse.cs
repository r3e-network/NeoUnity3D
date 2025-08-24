using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for the getmempool RPC call.
    /// Returns information about the node's memory pool including verified and unverified transactions.
    /// </summary>
    [System.Serializable]
    public class NeoGetMemPoolResponse : NeoResponse<NeoGetMemPoolResponse.MemPoolDetails>
    {
        /// <summary>
        /// Gets the memory pool details from the response.
        /// </summary>
        public MemPoolDetails MemPoolDetails => Result;

        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NeoGetMemPoolResponse() : base()
        {
        }

        /// <summary>
        /// Creates a successful memory pool response.
        /// </summary>
        /// <param name="memPoolDetails">The memory pool details</param>
        /// <param name="id">The request ID</param>
        public NeoGetMemPoolResponse(MemPoolDetails memPoolDetails, int id = 1) : base(memPoolDetails, id)
        {
        }

        /// <summary>
        /// Creates an error memory pool response.
        /// </summary>
        /// <param name="error">The error information</param>
        /// <param name="id">The request ID</param>
        public NeoGetMemPoolResponse(ResponseError error, int id = 1) : base(error, id)
        {
        }

        /// <summary>
        /// Contains detailed information about the memory pool state.
        /// </summary>
        [System.Serializable]
        public class MemPoolDetails
        {
            /// <summary>The current block height of the node</summary>
            [JsonProperty("height")]
            public int Height { get; set; }

            /// <summary>List of verified transaction hashes in the memory pool</summary>
            [JsonProperty("verified")]
            public List<string> Verified { get; set; } = new List<string>();

            /// <summary>List of unverified transaction hashes in the memory pool</summary>
            [JsonProperty("unverified")]
            public List<string> Unverified { get; set; } = new List<string>();

            /// <summary>
            /// Default constructor for JSON deserialization.
            /// </summary>
            public MemPoolDetails()
            {
            }

            /// <summary>
            /// Creates new memory pool details.
            /// </summary>
            /// <param name="height">The current block height</param>
            /// <param name="verified">List of verified transaction hashes</param>
            /// <param name="unverified">List of unverified transaction hashes</param>
            public MemPoolDetails(int height, List<string> verified, List<string> unverified)
            {
                Height = height;
                Verified = verified ?? new List<string>();
                Unverified = unverified ?? new List<string>();
            }

            /// <summary>
            /// Gets the total number of transactions in the memory pool.
            /// </summary>
            [JsonIgnore]
            public int TotalCount => (Verified?.Count ?? 0) + (Unverified?.Count ?? 0);

            /// <summary>
            /// Gets the number of verified transactions.
            /// </summary>
            [JsonIgnore]
            public int VerifiedCount => Verified?.Count ?? 0;

            /// <summary>
            /// Gets the number of unverified transactions.
            /// </summary>
            [JsonIgnore]
            public int UnverifiedCount => Unverified?.Count ?? 0;

            /// <summary>
            /// Checks if the memory pool contains a specific transaction hash.
            /// </summary>
            /// <param name="transactionHash">The transaction hash to search for</param>
            /// <returns>True if the transaction is in the memory pool</returns>
            public bool ContainsTransaction(string transactionHash)
            {
                if (string.IsNullOrEmpty(transactionHash))
                    return false;

                return (Verified?.Contains(transactionHash) == true) || 
                       (Unverified?.Contains(transactionHash) == true);
            }

            /// <summary>
            /// Checks if a transaction is verified in the memory pool.
            /// </summary>
            /// <param name="transactionHash">The transaction hash to check</param>
            /// <returns>True if the transaction is verified</returns>
            public bool IsTransactionVerified(string transactionHash)
            {
                if (string.IsNullOrEmpty(transactionHash))
                    return false;

                return Verified?.Contains(transactionHash) == true;
            }

            /// <summary>
            /// Returns a string representation of the memory pool details.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                return $"MemPoolDetails(Height: {Height}, Verified: {VerifiedCount}, Unverified: {UnverifiedCount}, Total: {TotalCount})";
            }

            /// <summary>
            /// Validates the memory pool details.
            /// </summary>
            /// <exception cref="ArgumentException">If the details are invalid</exception>
            public void Validate()
            {
                if (Height < 0)
                    throw new ArgumentException("Height cannot be negative");

                if (Verified == null)
                    throw new ArgumentException("Verified list cannot be null");

                if (Unverified == null)
                    throw new ArgumentException("Unverified list cannot be null");

                // Validate transaction hash format (should be 64-character hex strings)
                foreach (var hash in Verified)
                {
                    if (string.IsNullOrEmpty(hash) || hash.Length != 64)
                        throw new ArgumentException($"Invalid verified transaction hash format: {hash}");
                }

                foreach (var hash in Unverified)
                {
                    if (string.IsNullOrEmpty(hash) || hash.Length != 64)
                        throw new ArgumentException($"Invalid unverified transaction hash format: {hash}");
                }
            }
        }
    }
}