using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;
using Neo.Unity.SDK.Transaction;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents a signer in a Neo transaction.
    /// Contains account information and witness scope restrictions.
    /// </summary>
    [System.Serializable]
    public class TransactionSigner
    {
        #region Properties
        
        /// <summary>The account hash that will provide the signature</summary>
        [JsonProperty("account")]
        public Hash160 Account { get; set; }
        
        /// <summary>The witness scope restrictions for this signer</summary>
        [JsonProperty("scopes")]
        public List<WitnessScope> Scopes { get; set; }
        
        /// <summary>List of contracts this signer is allowed to interact with (if CustomContracts scope)</summary>
        [JsonProperty("allowedcontracts")]
        public List<string> AllowedContracts { get; set; }
        
        /// <summary>List of allowed groups this signer can interact with (if CustomGroups scope)</summary>
        [JsonProperty("allowedgroups")]
        public List<string> AllowedGroups { get; set; }
        
        /// <summary>List of witness rules for this signer (if Rules scope)</summary>
        [JsonProperty("rules")]
        public List<WitnessRule> Rules { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public TransactionSigner()
        {
            Scopes = new List<WitnessScope>();
            AllowedContracts = new List<string>();
            AllowedGroups = new List<string>();
            Rules = new List<WitnessRule>();
        }
        
        /// <summary>
        /// Creates a new transaction signer with basic scope.
        /// </summary>
        /// <param name="account">The account hash</param>
        /// <param name="scopes">The witness scopes</param>
        public TransactionSigner(Hash160 account, params WitnessScope[] scopes)
        {
            Account = account;
            Scopes = new List<WitnessScope>(scopes);
            AllowedContracts = new List<string>();
            AllowedGroups = new List<string>();
            Rules = new List<WitnessRule>();
        }
        
        /// <summary>
        /// Creates a new transaction signer with full configuration.
        /// </summary>
        /// <param name="account">The account hash</param>
        /// <param name="scopes">The witness scopes</param>
        /// <param name="allowedContracts">Allowed contracts</param>
        /// <param name="allowedGroups">Allowed groups</param>
        /// <param name="rules">Witness rules</param>
        public TransactionSigner(Hash160 account, List<WitnessScope> scopes, 
                               List<string> allowedContracts = null, List<string> allowedGroups = null, 
                               List<WitnessRule> rules = null)
        {
            Account = account;
            Scopes = scopes ?? new List<WitnessScope>();
            AllowedContracts = allowedContracts ?? new List<string>();
            AllowedGroups = allowedGroups ?? new List<string>();
            Rules = rules ?? new List<WitnessRule>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this signer has any scopes defined</summary>
        [JsonIgnore]
        public bool HasScopes => Scopes != null && Scopes.Count > 0;
        
        /// <summary>Whether this signer has allowed contracts</summary>
        [JsonIgnore]
        public bool HasAllowedContracts => AllowedContracts != null && AllowedContracts.Count > 0;
        
        /// <summary>Whether this signer has allowed groups</summary>
        [JsonIgnore]
        public bool HasAllowedGroups => AllowedGroups != null && AllowedGroups.Count > 0;
        
        /// <summary>Whether this signer has witness rules</summary>
        [JsonIgnore]
        public bool HasRules => Rules != null && Rules.Count > 0;
        
        /// <summary>Whether this signer has global scope</summary>
        [JsonIgnore]
        public bool HasGlobalScope => HasScopes && Scopes.Contains(WitnessScope.Global);
        
        /// <summary>Whether this signer has caller-only scope</summary>
        [JsonIgnore]
        public bool HasCallerOnlyScope => HasScopes && Scopes.Contains(WitnessScope.CalledByEntry);
        
        /// <summary>Whether this signer has custom contracts scope</summary>
        [JsonIgnore]
        public bool HasCustomContractsScope => HasScopes && Scopes.Contains(WitnessScope.CustomContracts);
        
        /// <summary>Whether this signer has custom groups scope</summary>
        [JsonIgnore]
        public bool HasCustomGroupsScope => HasScopes && Scopes.Contains(WitnessScope.CustomGroups);
        
        /// <summary>Whether this signer has witness rules scope</summary>
        [JsonIgnore]
        public bool HasRulesScope => HasScopes && Scopes.Contains(WitnessScope.WitnessRules);
        
        /// <summary>Number of scopes</summary>
        [JsonIgnore]
        public int ScopeCount => Scopes?.Count ?? 0;
        
        #endregion
        
        #region Scope Methods
        
        /// <summary>
        /// Adds a witness scope to this signer.
        /// </summary>
        /// <param name="scope">The scope to add</param>
        public void AddScope(WitnessScope scope)
        {
            if (Scopes == null)
                Scopes = new List<WitnessScope>();
            
            if (!Scopes.Contains(scope))
                Scopes.Add(scope);
        }
        
        /// <summary>
        /// Removes a witness scope from this signer.
        /// </summary>
        /// <param name="scope">The scope to remove</param>
        /// <returns>True if the scope was removed</returns>
        public bool RemoveScope(WitnessScope scope)
        {
            return Scopes?.Remove(scope) ?? false;
        }
        
        /// <summary>
        /// Checks if this signer has a specific scope.
        /// </summary>
        /// <param name="scope">The scope to check</param>
        /// <returns>True if the signer has this scope</returns>
        public bool HasScope(WitnessScope scope)
        {
            return Scopes?.Contains(scope) ?? false;
        }
        
        /// <summary>
        /// Gets the scopes as a comma-separated string.
        /// </summary>
        /// <returns>Scopes as string</returns>
        public string GetScopesString()
        {
            if (!HasScopes)
                return "None";
            
            return string.Join(", ", Scopes);
        }
        
        #endregion
        
        #region Contract and Group Methods
        
        /// <summary>
        /// Adds an allowed contract hash.
        /// </summary>
        /// <param name="contractHash">The contract hash to add</param>
        public void AddAllowedContract(string contractHash)
        {
            if (AllowedContracts == null)
                AllowedContracts = new List<string>();
            
            if (!string.IsNullOrEmpty(contractHash) && !AllowedContracts.Contains(contractHash))
            {
                AllowedContracts.Add(contractHash);
                
                // Ensure CustomContracts scope is included
                AddScope(WitnessScope.CustomContracts);
            }
        }
        
        /// <summary>
        /// Removes an allowed contract hash.
        /// </summary>
        /// <param name="contractHash">The contract hash to remove</param>
        /// <returns>True if the contract was removed</returns>
        public bool RemoveAllowedContract(string contractHash)
        {
            return AllowedContracts?.Remove(contractHash) ?? false;
        }
        
        /// <summary>
        /// Checks if a contract is allowed.
        /// </summary>
        /// <param name="contractHash">The contract hash to check</param>
        /// <returns>True if the contract is allowed</returns>
        public bool IsContractAllowed(string contractHash)
        {
            if (HasGlobalScope)
                return true;
            
            return AllowedContracts?.Contains(contractHash) ?? false;
        }
        
        /// <summary>
        /// Adds an allowed group.
        /// </summary>
        /// <param name="groupKey">The group public key to add</param>
        public void AddAllowedGroup(string groupKey)
        {
            if (AllowedGroups == null)
                AllowedGroups = new List<string>();
            
            if (!string.IsNullOrEmpty(groupKey) && !AllowedGroups.Contains(groupKey))
            {
                AllowedGroups.Add(groupKey);
                
                // Ensure CustomGroups scope is included
                AddScope(WitnessScope.CustomGroups);
            }
        }
        
        /// <summary>
        /// Removes an allowed group.
        /// </summary>
        /// <param name="groupKey">The group public key to remove</param>
        /// <returns>True if the group was removed</returns>
        public bool RemoveAllowedGroup(string groupKey)
        {
            return AllowedGroups?.Remove(groupKey) ?? false;
        }
        
        /// <summary>
        /// Checks if a group is allowed.
        /// </summary>
        /// <param name="groupKey">The group public key to check</param>
        /// <returns>True if the group is allowed</returns>
        public bool IsGroupAllowed(string groupKey)
        {
            if (HasGlobalScope)
                return true;
            
            return AllowedGroups?.Contains(groupKey) ?? false;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this transaction signer.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (Account == null)
                throw new InvalidOperationException("Transaction signer account cannot be null.");
            
            if (!HasScopes)
                throw new InvalidOperationException("Transaction signer must have at least one witness scope.");
            
            // Validate scope-specific requirements
            if (HasCustomContractsScope && !HasAllowedContracts)
                throw new InvalidOperationException("CustomContracts scope requires at least one allowed contract.");
            
            if (HasCustomGroupsScope && !HasAllowedGroups)
                throw new InvalidOperationException("CustomGroups scope requires at least one allowed group.");
            
            if (HasRulesScope && !HasRules)
                throw new InvalidOperationException("WitnessRules scope requires at least one witness rule.");
        }
        
        /// <summary>
        /// Checks if this signer can authorize an operation in the given context.
        /// </summary>
        /// <param name="callingContract">The calling contract hash</param>
        /// <param name="targetContract">The target contract hash</param>
        /// <returns>True if authorized</returns>
        public bool CanAuthorize(string callingContract, string targetContract)
        {
            if (HasGlobalScope)
                return true;
            
            if (HasCallerOnlyScope && callingContract == targetContract)
                return true;
            
            if (HasCustomContractsScope && IsContractAllowed(targetContract))
                return true;
            
            // Additional rule-based checks would go here
            
            return false;
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this transaction signer.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var scopesStr = GetScopesString();
            return $"TransactionSigner(Account: {Account}, Scopes: {scopesStr})";
        }
        
        /// <summary>
        /// Returns a detailed string representation of this transaction signer.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"TransactionSigner:\n";
            result += $"  Account: {Account}\n";
            result += $"  Scopes: {GetScopesString()}\n";
            
            if (HasAllowedContracts)
            {
                result += $"  Allowed Contracts ({AllowedContracts.Count}):\n";
                foreach (var contract in AllowedContracts)
                {
                    result += $"    - {contract}\n";
                }
            }
            
            if (HasAllowedGroups)
            {
                result += $"  Allowed Groups ({AllowedGroups.Count}):\n";
                foreach (var group in AllowedGroups)
                {
                    result += $"    - {group}\n";
                }
            }
            
            if (HasRules)
            {
                result += $"  Witness Rules: {Rules.Count}\n";
            }
            
            return result;
        }
        
        #endregion
    }
}