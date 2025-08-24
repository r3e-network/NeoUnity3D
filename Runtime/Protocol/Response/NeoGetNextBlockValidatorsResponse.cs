using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Crypto;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for getNextBlockValidators RPC method.
    /// Returns information about validators who will validate the next block.
    /// </summary>
    [System.Serializable]
    public class NeoGetNextBlockValidatorsResponse : NeoResponse<List<NeoGetNextBlockValidatorsResponse.Validator>>
    {
        /// <summary>
        /// Information about a validator for the next block.
        /// </summary>
        [System.Serializable]
        public class Validator
        {
            /// <summary>The validator's public key in hex format</summary>
            [JsonProperty("publickey")]
            public string PublicKey { get; set; }
            
            /// <summary>The number of votes this validator has</summary>
            [JsonProperty("votes")]
            public string Votes { get; set; }
            
            /// <summary>Whether this validator is active</summary>
            [JsonProperty("active")]
            public bool Active { get; set; }
            
            /// <summary>
            /// Gets the public key as ECPublicKey object.
            /// </summary>
            /// <returns>ECPublicKey object or null if invalid</returns>
            public ECPublicKey GetECPublicKey()
            {
                if (string.IsNullOrEmpty(PublicKey))
                    return null;
                
                try
                {
                    return new ECPublicKey(PublicKey);
                }
                catch
                {
                    return null;
                }
            }
            
            /// <summary>
            /// Gets the validator's Neo address.
            /// </summary>
            /// <returns>Neo address string</returns>
            public string GetAddress()
            {
                var pubKey = GetECPublicKey();
                if (pubKey == null)
                    return null;
                
                try
                {
                    return pubKey.GetAddress();
                }
                catch
                {
                    return null;
                }
            }
            
            /// <summary>
            /// Gets the validator's script hash.
            /// </summary>
            /// <returns>Script hash or null if invalid</returns>
            public Hash160 GetScriptHash()
            {
                var address = GetAddress();
                if (string.IsNullOrEmpty(address))
                    return null;
                
                try
                {
                    return Hash160.FromAddress(address);
                }
                catch
                {
                    return null;
                }
            }
            
            /// <summary>
            /// Gets the vote count as a numeric value.
            /// </summary>
            /// <returns>Vote count or 0 if invalid</returns>
            public long GetVoteCount()
            {
                if (string.IsNullOrEmpty(Votes))
                    return 0;
                
                if (long.TryParse(Votes, out var voteCount))
                    return voteCount;
                
                return 0;
            }
            
            /// <summary>
            /// Formats the vote count for display.
            /// </summary>
            /// <returns>Formatted vote count string</returns>
            public string GetFormattedVotes()
            {
                var voteCount = GetVoteCount();
                
                if (voteCount >= 1_000_000_000)
                    return $"{voteCount / 1_000_000_000.0:F1}B";
                
                if (voteCount >= 1_000_000)
                    return $"{voteCount / 1_000_000.0:F1}M";
                
                if (voteCount >= 1_000)
                    return $"{voteCount / 1_000.0:F1}K";
                
                return voteCount.ToString();
            }
            
            /// <summary>
            /// Validates that this validator has valid data.
            /// </summary>
            /// <returns>True if validator data is valid</returns>
            public bool IsValid()
            {
                if (string.IsNullOrEmpty(PublicKey))
                    return false;
                
                if (!PublicKey.IsValidHex())
                    return false;
                
                var pubKey = GetECPublicKey();
                if (pubKey == null)
                    return false;
                
                return pubKey.IsValid();
            }
            
            /// <summary>
            /// String representation of validator.
            /// </summary>
            /// <returns>String representation</returns>
            public override string ToString()
            {
                var activeStatus = Active ? "Active" : "Inactive";
                var voteDisplay = GetFormattedVotes();
                var address = GetAddress();
                
                return $"Validator(Address: {address ?? "Invalid"}, Votes: {voteDisplay}, Status: {activeStatus})";
            }
        }
        
        /// <summary>
        /// Extension methods for validator collections.
        /// </summary>
        public static class ValidatorExtensions
        {
            /// <summary>
            /// Gets only active validators from the list.
            /// </summary>
            /// <param name="validators">The validator list</param>
            /// <returns>Active validators only</returns>
            public static List<Validator> GetActiveValidators(this List<Validator> validators)
            {
                if (validators == null)
                    return new List<Validator>();
                
                return validators.Where(v => v.Active && v.IsValid()).ToList();
            }
            
            /// <summary>
            /// Gets validators sorted by vote count (descending).
            /// </summary>
            /// <param name="validators">The validator list</param>
            /// <returns>Validators sorted by votes</returns>
            public static List<Validator> SortByVotes(this List<Validator> validators)
            {
                if (validators == null)
                    return new List<Validator>();
                
                return validators.OrderByDescending(v => v.GetVoteCount()).ToList();
            }
            
            /// <summary>
            /// Gets the total vote count across all validators.
            /// </summary>
            /// <param name="validators">The validator list</param>
            /// <returns>Total votes</returns>
            public static long GetTotalVotes(this List<Validator> validators)
            {
                if (validators == null)
                    return 0;
                
                return validators.Sum(v => v.GetVoteCount());
            }
            
            /// <summary>
            /// Finds a validator by public key.
            /// </summary>
            /// <param name="validators">The validator list</param>
            /// <param name="publicKey">The public key to search for</param>
            /// <returns>Validator or null if not found</returns>
            public static Validator FindByPublicKey(this List<Validator> validators, string publicKey)
            {
                if (validators == null || string.IsNullOrEmpty(publicKey))
                    return null;
                
                return validators.Find(v => v.PublicKey?.Equals(publicKey, StringComparison.OrdinalIgnoreCase) == true);
            }
            
            /// <summary>
            /// Finds a validator by address.
            /// </summary>
            /// <param name="validators">The validator list</param>
            /// <param name="address">The address to search for</param>
            /// <returns>Validator or null if not found</returns>
            public static Validator FindByAddress(this List<Validator> validators, string address)
            {
                if (validators == null || string.IsNullOrEmpty(address))
                    return null;
                
                return validators.Find(v => v.GetAddress()?.Equals(address, StringComparison.OrdinalIgnoreCase) == true);
            }
        }
    }
}