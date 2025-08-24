using System;
using System.Collections.Generic;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Serialization;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// Represents a transaction signer for JSON-RPC serialization.
    /// Used specifically for RPC method parameters when calling Neo node methods.
    /// </summary>
    [System.Serializable]
    public class TransactionSigner
    {
        #region Properties
        
        /// <summary>The script hash of the signer account</summary>
        [JsonProperty("account")]
        public string Account { get; set; }
        
        /// <summary>The witness scopes as a combined value</summary>
        [JsonProperty("scopes")]
        public string Scopes { get; set; }
        
        /// <summary>The allowed contracts for CustomContracts scope</summary>
        [JsonProperty("allowedcontracts", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AllowedContracts { get; set; }
        
        /// <summary>The allowed groups for CustomGroups scope</summary>
        [JsonProperty("allowedgroups", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AllowedGroups { get; set; }
        
        /// <summary>The witness rules for Rules scope</summary>
        [JsonProperty("rules", NullValueHandling = NullValueHandling.Ignore)]
        public List<WitnessRule> Rules { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a transaction signer from a Signer object.
        /// </summary>
        /// <param name="signer">The signer to convert</param>
        public TransactionSigner(Signer signer)
        {
            if (signer == null)
                throw new ArgumentNullException(nameof(signer));
            
            Account = signer.SignerHash.ToString();
            Scopes = ConvertScopesToString(signer.Scopes);
            
            // Add allowed contracts if CustomContracts scope is present
            if (signer.Scopes.Contains(WitnessScope.CustomContracts) && signer.AllowedContracts.Count > 0)
            {
                AllowedContracts = new List<string>();
                foreach (var contract in signer.AllowedContracts)
                {
                    AllowedContracts.Add(contract.ToString());
                }
            }
            
            // Add allowed groups if CustomGroups scope is present
            if (signer.Scopes.Contains(WitnessScope.CustomGroups) && signer.AllowedGroups.Count > 0)
            {
                AllowedGroups = new List<string>();
                foreach (var group in signer.AllowedGroups)
                {
                    AllowedGroups.Add(Convert.ToHexString(group.GetEncoded(true)));
                }
            }
            
            // Add rules if WitnessRules scope is present
            if (signer.Scopes.Contains(WitnessScope.WitnessRules) && signer.Rules.Count > 0)
            {
                Rules = new List<WitnessRule>(signer.Rules);
            }
        }
        
        /// <summary>
        /// Creates a transaction signer with specified parameters.
        /// </summary>
        /// <param name="account">The account script hash</param>
        /// <param name="scopes">The witness scopes</param>
        /// <param name="allowedContracts">Allowed contracts (optional)</param>
        /// <param name="allowedGroups">Allowed groups (optional)</param>
        /// <param name="rules">Witness rules (optional)</param>
        public TransactionSigner(string account, string scopes, List<string> allowedContracts = null, 
                                List<string> allowedGroups = null, List<WitnessRule> rules = null)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            Scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
            AllowedContracts = allowedContracts;
            AllowedGroups = allowedGroups;
            Rules = rules;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public TransactionSigner()
        {
        }
        
        #endregion
        
        #region Scope Conversion
        
        /// <summary>
        /// Converts a list of WitnessScope enums to their JSON string representation.
        /// </summary>
        /// <param name="scopes">The witness scopes</param>
        /// <returns>JSON-compatible scope string</returns>
        private static string ConvertScopesToString(List<WitnessScope> scopes)
        {
            if (scopes == null || scopes.Count == 0)
                return WitnessScope.None.ToString();
            
            if (scopes.Count == 1)
                return scopes[0].ToString();
            
            // For multiple scopes, combine them
            var scopeStrings = new List<string>();
            foreach (var scope in scopes)
            {
                scopeStrings.Add(scope.ToString());
            }
            
            return string.Join(",", scopeStrings);
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates that this transaction signer is properly configured.
        /// </summary>
        /// <exception cref="InvalidOperationException">If configuration is invalid</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Account))
                throw new InvalidOperationException("Transaction signer must have a valid account hash.");
            
            if (string.IsNullOrEmpty(Scopes))
                throw new InvalidOperationException("Transaction signer must have valid witness scopes.");
            
            // Validate account hash format
            try
            {
                var hash = new Hash160(Account);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid account hash format: {Account}", ex);
            }
            
            // Validate allowed contracts if present
            if (AllowedContracts != null)
            {
                foreach (var contract in AllowedContracts)
                {
                    try
                    {
                        var contractHash = new Hash160(contract);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Invalid allowed contract hash format: {contract}", ex);
                    }
                }
            }
            
            // Validate allowed groups if present
            if (AllowedGroups != null)
            {
                foreach (var group in AllowedGroups)
                {
                    if (string.IsNullOrEmpty(group) || !group.IsValidHex())
                    {
                        throw new InvalidOperationException($"Invalid allowed group format: {group}");
                    }
                }
            }
        }
        
        #endregion
        
        #region Equality and Hashing
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is TransactionSigner other)
            {
                return Account == other.Account &&
                       Scopes == other.Scopes &&
                       ((AllowedContracts == null && other.AllowedContracts == null) ||
                        (AllowedContracts != null && other.AllowedContracts != null && AllowedContracts.SequenceEqual(other.AllowedContracts))) &&
                       ((AllowedGroups == null && other.AllowedGroups == null) ||
                        (AllowedGroups != null && other.AllowedGroups != null && AllowedGroups.SequenceEqual(other.AllowedGroups))) &&
                       ((Rules == null && other.Rules == null) ||
                        (Rules != null && other.Rules != null && Rules.SequenceEqual(other.Rules)));
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = HashCode.Combine(Account, Scopes);
            
            if (AllowedContracts != null)
            {
                foreach (var contract in AllowedContracts)
                {
                    hashCode = HashCode.Combine(hashCode, contract);
                }
            }
            
            if (AllowedGroups != null)
            {
                foreach (var group in AllowedGroups)
                {
                    hashCode = HashCode.Combine(hashCode, group);
                }
            }
            
            return hashCode;
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this transaction signer.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var result = $"TransactionSigner(Account: {Account}, Scopes: {Scopes}";
            
            if (AllowedContracts != null && AllowedContracts.Count > 0)
            {
                result += $", AllowedContracts: {AllowedContracts.Count}";
            }
            
            if (AllowedGroups != null && AllowedGroups.Count > 0)
            {
                result += $", AllowedGroups: {AllowedGroups.Count}";
            }
            
            if (Rules != null && Rules.Count > 0)
            {
                result += $", Rules: {Rules.Count}";
            }
            
            result += ")";
            return result;
        }
        
        #endregion
    }
}