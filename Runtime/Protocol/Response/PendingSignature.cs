using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Contains information about pending signatures for multi-signature transactions.
    /// Used when a transaction requires additional signatures before it can be submitted.
    /// </summary>
    [System.Serializable]
    public class PendingSignature
    {
        #region Properties
        
        /// <summary>The transaction type</summary>
        [JsonProperty("type")]
        public string Type { get; set; }
        
        /// <summary>The network where this transaction will be submitted</summary>
        [JsonProperty("network")]
        public long Network { get; set; }
        
        /// <summary>List of items that need to be signed</summary>
        [JsonProperty("items")]
        public List<PendingSignatureItem> Items { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public PendingSignature()
        {
            Items = new List<PendingSignatureItem>();
        }
        
        /// <summary>
        /// Creates a new pending signature.
        /// </summary>
        /// <param name="type">The transaction type</param>
        /// <param name="network">The network ID</param>
        /// <param name="items">The signature items</param>
        public PendingSignature(string type, long network, List<PendingSignatureItem> items = null)
        {
            Type = type;
            Network = network;
            Items = items ?? new List<PendingSignatureItem>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether there are items that need signing</summary>
        [JsonIgnore]
        public bool HasItems => Items != null && Items.Count > 0;
        
        /// <summary>Number of signature items</summary>
        [JsonIgnore]
        public int ItemCount => Items?.Count ?? 0;
        
        /// <summary>Whether this is for mainnet</summary>
        [JsonIgnore]
        public bool IsMainNet => Network == 860833102; // Neo mainnet magic number
        
        /// <summary>Whether this is for testnet</summary>
        [JsonIgnore]
        public bool IsTestNet => Network == 894710606; // Neo testnet magic number
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Gets the signature item at the specified index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The signature item</returns>
        /// <exception cref="ArgumentOutOfRangeException">If index is out of range</exception>
        public PendingSignatureItem GetItem(int index)
        {
            if (Items == null || index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Item index {index} is out of range. Items has {Items?.Count ?? 0} items.");
            }
            
            return Items[index];
        }
        
        /// <summary>
        /// Tries to get the signature item at the specified index.
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="item">The signature item if found</param>
        /// <returns>True if successful, false if index is out of range</returns>
        public bool TryGetItem(int index, out PendingSignatureItem item)
        {
            if (Items != null && index >= 0 && index < Items.Count)
            {
                item = Items[index];
                return true;
            }
            
            item = null;
            return false;
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this pending signature.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var networkName = IsMainNet ? "MainNet" : IsTestNet ? "TestNet" : Network.ToString();
            return $"PendingSignature(Type: {Type}, Network: {networkName}, Items: {ItemCount})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents an individual item that needs to be signed in a multi-signature transaction.
    /// </summary>
    [System.Serializable]
    public class PendingSignatureItem
    {
        #region Properties
        
        /// <summary>The script hash that needs to provide a signature</summary>
        [JsonProperty("script")]
        public Hash160 Script { get; set; }
        
        /// <summary>The list of parameters for this signature item</summary>
        [JsonProperty("parameters")]
        public List<ContractParameter> Parameters { get; set; }
        
        /// <summary>Existing signatures for this item</summary>
        [JsonProperty("signatures")]
        public Dictionary<string, string> Signatures { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public PendingSignatureItem()
        {
            Parameters = new List<ContractParameter>();
            Signatures = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// Creates a new pending signature item.
        /// </summary>
        /// <param name="script">The script hash</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="signatures">Existing signatures</param>
        public PendingSignatureItem(Hash160 script, List<ContractParameter> parameters = null, Dictionary<string, string> signatures = null)
        {
            Script = script;
            Parameters = parameters ?? new List<ContractParameter>();
            Signatures = signatures ?? new Dictionary<string, string>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this item has parameters</summary>
        [JsonIgnore]
        public bool HasParameters => Parameters != null && Parameters.Count > 0;
        
        /// <summary>Whether this item has existing signatures</summary>
        [JsonIgnore]
        public bool HasSignatures => Signatures != null && Signatures.Count > 0;
        
        /// <summary>Number of parameters</summary>
        [JsonIgnore]
        public int ParameterCount => Parameters?.Count ?? 0;
        
        /// <summary>Number of existing signatures</summary>
        [JsonIgnore]
        public int SignatureCount => Signatures?.Count ?? 0;
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Adds a signature for the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key</param>
        /// <param name="signature">The signature</param>
        public void AddSignature(string publicKey, string signature)
        {
            if (Signatures == null)
                Signatures = new Dictionary<string, string>();
            
            Signatures[publicKey] = signature;
        }
        
        /// <summary>
        /// Checks if a signature exists for the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key to check</param>
        /// <returns>True if a signature exists, false otherwise</returns>
        public bool HasSignatureFor(string publicKey)
        {
            return Signatures != null && Signatures.ContainsKey(publicKey);
        }
        
        /// <summary>
        /// Gets the signature for the specified public key.
        /// </summary>
        /// <param name="publicKey">The public key</param>
        /// <returns>The signature or null if not found</returns>
        public string GetSignature(string publicKey)
        {
            return Signatures?.ContainsKey(publicKey) == true ? Signatures[publicKey] : null;
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this signature item.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"PendingSignatureItem(Script: {Script}, Parameters: {ParameterCount}, Signatures: {SignatureCount})";
        }
        
        #endregion
    }
}