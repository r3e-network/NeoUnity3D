using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Types;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Wallet
{
    /// <summary>
    /// Represents a NEP-6 wallet file format for standard compatibility.
    /// NEP-6 is the standard format for Neo wallet files used across the ecosystem.
    /// </summary>
    [System.Serializable]
    public class NEP6Wallet
    {
        #region Properties
        
        /// <summary>The name of the wallet</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>The version of the wallet format</summary>
        [JsonProperty("version")]
        public string Version { get; set; }
        
        /// <summary>The scrypt parameters for encryption</summary>
        [JsonProperty("scrypt")]
        public ScryptParams Scrypt { get; set; }
        
        /// <summary>The accounts in this wallet</summary>
        [JsonProperty("accounts")]
        public List<NEP6Account> Accounts { get; set; }
        
        /// <summary>Additional metadata (optional)</summary>
        [JsonProperty("extra")]
        public Dictionary<string, object> Extra { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new NEP-6 wallet.
        /// </summary>
        /// <param name="name">The wallet name</param>
        /// <param name="version">The wallet version</param>
        /// <param name="scrypt">The scrypt parameters</param>
        /// <param name="accounts">The accounts</param>
        /// <param name="extra">Additional metadata</param>
        public NEP6Wallet(string name, string version, ScryptParams scrypt, List<NEP6Account> accounts, Dictionary<string, object> extra)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Scrypt = scrypt ?? throw new ArgumentNullException(nameof(scrypt));
            Accounts = accounts ?? new List<NEP6Account>();
            Extra = extra;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NEP6Wallet()
        {
            Accounts = new List<NEP6Account>();
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is NEP6Wallet other)
            {
                return Name == other.Name &&
                       Version == other.Version &&
                       Scrypt.Equals(other.Scrypt) &&
                       Accounts.Count == other.Accounts.Count &&
                       Accounts.All(acc => other.Accounts.Contains(acc)) &&
                       ((Extra == null && other.Extra == null) ||
                        (Extra != null && other.Extra != null && Extra.Count == other.Extra.Count));
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = HashCode.Combine(Name, Version, Scrypt);
            
            foreach (var account in Accounts)
            {
                hashCode = HashCode.Combine(hashCode, account.GetHashCode());
            }
            
            return hashCode;
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this NEP-6 wallet.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var defaultAccount = Accounts.FirstOrDefault(acc => acc.IsDefault);
            return $"NEP6Wallet(Name: {Name}, Version: {Version}, Accounts: {Accounts.Count}, Default: {defaultAccount?.Address ?? "None"})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a NEP-6 account within a wallet file.
    /// </summary>
    [System.Serializable]
    public class NEP6Account
    {
        #region Properties
        
        /// <summary>The Neo address of this account</summary>
        [JsonProperty("address")]
        public string Address { get; set; }
        
        /// <summary>The label/name for this account</summary>
        [JsonProperty("label")]
        public string Label { get; set; }
        
        /// <summary>Whether this is the default account in the wallet</summary>
        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }
        
        /// <summary>Whether this account is locked</summary>
        [JsonProperty("lock")]
        public bool Lock { get; set; }
        
        /// <summary>The encrypted private key (NEP-2 format)</summary>
        [JsonProperty("key")]
        public string Key { get; set; }
        
        /// <summary>The contract information for this account</summary>
        [JsonProperty("contract")]
        public NEP6Contract Contract { get; set; }
        
        /// <summary>Additional metadata</summary>
        [JsonProperty("extra")]
        public Dictionary<string, object> Extra { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new NEP-6 account.
        /// </summary>
        /// <param name="address">The Neo address</param>
        /// <param name="label">The account label</param>
        /// <param name="isDefault">Whether this is the default account</param>
        /// <param name="lock">Whether the account is locked</param>
        /// <param name="key">The encrypted private key</param>
        /// <param name="contract">The contract information</param>
        /// <param name="extra">Additional metadata</param>
        public NEP6Account(string address, string label, bool isDefault, bool lock, string key, NEP6Contract contract, Dictionary<string, object> extra)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Label = label ?? address;
            IsDefault = isDefault;
            Lock = lock;
            Key = key;
            Contract = contract;
            Extra = extra;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NEP6Account()
        {
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is NEP6Account other)
            {
                return Address == other.Address &&
                       Label == other.Label &&
                       IsDefault == other.IsDefault &&
                       Lock == other.Lock &&
                       Key == other.Key &&
                       ((Contract == null && other.Contract == null) ||
                        (Contract != null && Contract.Equals(other.Contract)));
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Address, Label, IsDefault, Lock, Key, Contract);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents contract information in a NEP-6 account.
    /// </summary>
    [System.Serializable]
    public class NEP6Contract
    {
        #region Properties
        
        /// <summary>The verification script in base64 format</summary>
        [JsonProperty("script")]
        public string Script { get; set; }
        
        /// <summary>The parameters for the contract</summary>
        [JsonProperty("parameters")]
        public List<NEP6Parameter> Parameters { get; set; }
        
        /// <summary>Whether the contract is deployed on the blockchain</summary>
        [JsonProperty("deployed")]
        public bool Deployed { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new NEP-6 contract.
        /// </summary>
        /// <param name="script">The verification script in base64</param>
        /// <param name="parameters">The contract parameters</param>
        /// <param name="deployed">Whether the contract is deployed</param>
        public NEP6Contract(string script, List<NEP6Parameter> parameters, bool deployed)
        {
            Script = script;
            Parameters = parameters ?? new List<NEP6Parameter>();
            Deployed = deployed;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NEP6Contract()
        {
            Parameters = new List<NEP6Parameter>();
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is NEP6Contract other)
            {
                return Script == other.Script &&
                       Deployed == other.Deployed &&
                       Parameters.Count == other.Parameters.Count &&
                       Parameters.All(param => other.Parameters.Contains(param));
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hashCode = HashCode.Combine(Script, Deployed);
            
            foreach (var param in Parameters)
            {
                hashCode = HashCode.Combine(hashCode, param.GetHashCode());
            }
            
            return hashCode;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a parameter in a NEP-6 contract.
    /// </summary>
    [System.Serializable]
    public class NEP6Parameter
    {
        #region Properties
        
        /// <summary>The parameter name</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>The parameter type</summary>
        [JsonProperty("type")]
        public ContractParameterType Type { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new NEP-6 parameter.
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="type">The parameter type</param>
        public NEP6Parameter(string name, ContractParameterType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NEP6Parameter()
        {
        }
        
        #endregion
        
        #region Equality
        
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is NEP6Parameter other)
            {
                return Name == other.Name && Type == other.Type;
            }
            return false;
        }
        
        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type);
        }
        
        #endregion
    }
}