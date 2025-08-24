using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// Represents a context for contract parameter signing, containing all the information
    /// needed to collect signatures for a transaction from multiple signers.
    /// </summary>
    [Serializable]
    public class ContractParametersContext
    {
        /// <summary>
        /// The type identifier for Neo transactions.
        /// </summary>
        public const string TRANSACTION_TYPE = "Neo.Network.P2P.Payloads.Transaction";

        [SerializeField]
        private string _type;

        [SerializeField]
        private string _hash;

        [SerializeField]
        private string _data;

        [SerializeField]
        private Dictionary<string, ContextItem> _items;

        [SerializeField]
        private int _network;

        /// <summary>
        /// Gets the type of the context (always transaction type for this implementation).
        /// </summary>
        public string Type => _type;

        /// <summary>
        /// Gets the hash of the transaction being signed.
        /// </summary>
        public string Hash => _hash;

        /// <summary>
        /// Gets the serialized transaction data.
        /// </summary>
        public string Data => _data;

        /// <summary>
        /// Gets the context items containing signatures and parameters for each signer.
        /// </summary>
        public IReadOnlyDictionary<string, ContextItem> Items => _items?.AsReadOnly() ?? new Dictionary<string, ContextItem>();

        /// <summary>
        /// Gets the network identifier.
        /// </summary>
        public int Network => _network;

        /// <summary>
        /// Gets a value indicating whether all required signatures have been collected.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return _items != null && _items.Values.All(item => item.IsComplete);
            }
        }

        /// <summary>
        /// Gets the number of signers in this context.
        /// </summary>
        public int SignerCount => _items?.Count ?? 0;

        /// <summary>
        /// Initializes a new instance of the ContractParametersContext class.
        /// </summary>
        /// <param name="hash">The hash of the transaction.</param>
        /// <param name="data">The serialized transaction data.</param>
        /// <param name="items">The context items for signers.</param>
        /// <param name="network">The network identifier.</param>
        public ContractParametersContext(string hash, string data, Dictionary<string, ContextItem> items = null, int network = 0)
        {
            _type = TRANSACTION_TYPE;
            _hash = hash ?? throw new ArgumentNullException(nameof(hash));
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _items = items != null ? new Dictionary<string, ContextItem>(items) : new Dictionary<string, ContextItem>();
            _network = network;
        }

        /// <summary>
        /// Creates a new ContractParametersContext from a transaction.
        /// </summary>
        /// <param name="transaction">The transaction to create context for.</param>
        /// <param name="network">The network identifier.</param>
        /// <returns>A new ContractParametersContext instance.</returns>
        public static ContractParametersContext FromTransaction(NeoTransaction transaction, int network = 0)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var hash = transaction.GetHash().ToString();
            var data = Convert.ToBase64String(transaction.Serialize());
            var items = new Dictionary<string, ContextItem>();

            // Initialize context items for each signer
            foreach (var signer in transaction.Signers)
            {
                var scriptHash = signer.Account.ToString();
                items[scriptHash] = new ContextItem(
                    script: signer.GetVerificationScript()?.ToString() ?? "",
                    parameters: null,
                    signatures: new Dictionary<string, string>()
                );
            }

            return new ContractParametersContext(hash, data, items, network);
        }

        /// <summary>
        /// Adds or updates a context item for a specific script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash of the signer.</param>
        /// <param name="item">The context item to add or update.</param>
        public void SetContextItem(string scriptHash, ContextItem item)
        {
            if (string.IsNullOrEmpty(scriptHash))
                throw new ArgumentException("Script hash cannot be null or empty", nameof(scriptHash));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _items ??= new Dictionary<string, ContextItem>();
            _items[scriptHash] = item;
        }

        /// <summary>
        /// Gets a context item for a specific script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash of the signer.</param>
        /// <returns>The context item if found, null otherwise.</returns>
        public ContextItem GetContextItem(string scriptHash)
        {
            return _items?.GetValueOrDefault(scriptHash);
        }

        /// <summary>
        /// Adds a signature for a specific script hash and public key.
        /// </summary>
        /// <param name="scriptHash">The script hash of the signer.</param>
        /// <param name="publicKey">The public key used for signing.</param>
        /// <param name="signature">The signature data.</param>
        public void AddSignature(string scriptHash, string publicKey, string signature)
        {
            if (string.IsNullOrEmpty(scriptHash))
                throw new ArgumentException("Script hash cannot be null or empty", nameof(scriptHash));
            if (string.IsNullOrEmpty(publicKey))
                throw new ArgumentException("Public key cannot be null or empty", nameof(publicKey));
            if (string.IsNullOrEmpty(signature))
                throw new ArgumentException("Signature cannot be null or empty", nameof(signature));

            _items ??= new Dictionary<string, ContextItem>();
            
            if (!_items.TryGetValue(scriptHash, out var item))
            {
                item = new ContextItem("", null, new Dictionary<string, string>());
                _items[scriptHash] = item;
            }

            item.AddSignature(publicKey, signature);
        }

        /// <summary>
        /// Removes a signature for a specific script hash and public key.
        /// </summary>
        /// <param name="scriptHash">The script hash of the signer.</param>
        /// <param name="publicKey">The public key to remove signature for.</param>
        /// <returns>True if the signature was removed, false if not found.</returns>
        public bool RemoveSignature(string scriptHash, string publicKey)
        {
            if (_items?.TryGetValue(scriptHash, out var item) == true)
            {
                return item.RemoveSignature(publicKey);
            }
            return false;
        }

        /// <summary>
        /// Gets all signatures for a specific script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash of the signer.</param>
        /// <returns>A dictionary of public keys to signatures.</returns>
        public IReadOnlyDictionary<string, string> GetSignatures(string scriptHash)
        {
            var item = GetContextItem(scriptHash);
            return item?.Signatures ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Sets contract parameters for a specific script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash of the contract.</param>
        /// <param name="parameters">The contract parameters.</param>
        public void SetContractParameters(string scriptHash, ContractParameter[] parameters)
        {
            if (string.IsNullOrEmpty(scriptHash))
                throw new ArgumentException("Script hash cannot be null or empty", nameof(scriptHash));

            _items ??= new Dictionary<string, ContextItem>();
            
            if (!_items.TryGetValue(scriptHash, out var item))
            {
                item = new ContextItem("", null, new Dictionary<string, string>());
                _items[scriptHash] = item;
            }

            item.SetParameters(parameters);
        }

        /// <summary>
        /// Gets contract parameters for a specific script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash of the contract.</param>
        /// <returns>The contract parameters if found, null otherwise.</returns>
        public ContractParameter[] GetContractParameters(string scriptHash)
        {
            var item = GetContextItem(scriptHash);
            return item?.Parameters;
        }

        /// <summary>
        /// Validates that the context is properly formed and complete.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(_hash) || string.IsNullOrEmpty(_data))
                return false;

            if (_items == null || _items.Count == 0)
                return false;

            return _items.Values.All(item => item.IsValid());
        }

        /// <summary>
        /// Gets a summary of the signing progress.
        /// </summary>
        /// <returns>A string describing the current signing status.</returns>
        public string GetSigningProgress()
        {
            if (_items == null || _items.Count == 0)
                return "No signers";

            var completed = _items.Values.Count(item => item.IsComplete);
            var total = _items.Count;
            
            return $"{completed}/{total} signers completed";
        }

        /// <summary>
        /// Creates a deep copy of this context.
        /// </summary>
        /// <returns>A new ContractParametersContext instance.</returns>
        public ContractParametersContext Clone()
        {
            var clonedItems = _items?.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Clone()
            );

            return new ContractParametersContext(_hash, _data, clonedItems, _network);
        }

        public override string ToString()
        {
            return $"ContractParametersContext: {GetSigningProgress()}, Network: {_network}";
        }
    }

    /// <summary>
    /// Represents a context item containing signing information for a specific signer.
    /// </summary>
    [Serializable]
    public class ContextItem
    {
        [SerializeField]
        private string _script;

        [SerializeField]
        private ContractParameter[] _parameters;

        [SerializeField]
        private Dictionary<string, string> _signatures;

        /// <summary>
        /// Gets the verification script for this signer.
        /// </summary>
        public string Script => _script;

        /// <summary>
        /// Gets the contract parameters if this is a contract signer.
        /// </summary>
        public ContractParameter[] Parameters => _parameters;

        /// <summary>
        /// Gets the signatures collected for this signer.
        /// </summary>
        public IReadOnlyDictionary<string, string> Signatures => _signatures?.AsReadOnly() ?? new Dictionary<string, string>();

        /// <summary>
        /// Gets a value indicating whether this context item has all required signatures.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                // For contract signers, completion depends on parameters
                if (_parameters != null && _parameters.Length > 0)
                    return _parameters.All(p => p != null);

                // For signature-based signers, we need at least one signature
                return _signatures != null && _signatures.Count > 0;
            }
        }

        /// <summary>
        /// Gets the number of signatures collected.
        /// </summary>
        public int SignatureCount => _signatures?.Count ?? 0;

        /// <summary>
        /// Initializes a new instance of the ContextItem class.
        /// </summary>
        /// <param name="script">The verification script.</param>
        /// <param name="parameters">The contract parameters.</param>
        /// <param name="signatures">The signatures dictionary.</param>
        public ContextItem(string script, ContractParameter[] parameters = null, Dictionary<string, string> signatures = null)
        {
            _script = script ?? "";
            _parameters = parameters;
            _signatures = signatures != null ? new Dictionary<string, string>(signatures) : new Dictionary<string, string>();
        }

        /// <summary>
        /// Adds a signature for a specific public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="signature">The signature.</param>
        public void AddSignature(string publicKey, string signature)
        {
            if (string.IsNullOrEmpty(publicKey))
                throw new ArgumentException("Public key cannot be null or empty", nameof(publicKey));
            if (string.IsNullOrEmpty(signature))
                throw new ArgumentException("Signature cannot be null or empty", nameof(signature));

            _signatures ??= new Dictionary<string, string>();
            _signatures[publicKey] = signature;
        }

        /// <summary>
        /// Removes a signature for a specific public key.
        /// </summary>
        /// <param name="publicKey">The public key to remove signature for.</param>
        /// <returns>True if removed, false if not found.</returns>
        public bool RemoveSignature(string publicKey)
        {
            return _signatures?.Remove(publicKey) == true;
        }

        /// <summary>
        /// Sets the contract parameters for this context item.
        /// </summary>
        /// <param name="parameters">The parameters to set.</param>
        public void SetParameters(ContractParameter[] parameters)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Validates that this context item is properly formed.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public bool IsValid()
        {
            // Must have either signatures or parameters
            var hasSignatures = _signatures != null && _signatures.Count > 0;
            var hasParameters = _parameters != null && _parameters.Length > 0;
            
            return hasSignatures || hasParameters;
        }

        /// <summary>
        /// Creates a deep copy of this context item.
        /// </summary>
        /// <returns>A new ContextItem instance.</returns>
        public ContextItem Clone()
        {
            var clonedSignatures = _signatures != null ? new Dictionary<string, string>(_signatures) : null;
            var clonedParameters = _parameters?.ToArray(); // ContractParameter should be immutable

            return new ContextItem(_script, clonedParameters, clonedSignatures);
        }

        public override string ToString()
        {
            var type = _parameters != null ? "Contract" : "Signature";
            var count = _parameters?.Length ?? _signatures?.Count ?? 0;
            return $"ContextItem ({type}): {count} items, Complete: {IsComplete}";
        }
    }
}