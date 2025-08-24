using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Response for NEO getunspents RPC method (legacy method for NEO 2.x)
    /// </summary>
    [Serializable]
    public class NeoGetUnspentsResponse : NeoResponse<NeoGetUnspentsResponse.Unspents>
    {
        /// <summary>
        /// Gets the unspents result
        /// </summary>
        public Unspents UnspentsData => Result;
        
        /// <summary>
        /// Represents unspent transaction outputs for an address
        /// </summary>
        [Serializable]
        public struct Unspents : IEquatable<Unspents>
        {
            [JsonProperty("balance")]
            [SerializeField] private List<Balance> _balances;
            
            [JsonProperty("address")]
            [SerializeField] private string _address;
            
            /// <summary>
            /// Gets the list of asset balances with unspent outputs
            /// </summary>
            public List<Balance> Balances => _balances ?? new List<Balance>();
            
            /// <summary>
            /// Gets the address for which unspents are listed
            /// </summary>
            public string Address => _address;
            
            /// <summary>
            /// Initializes a new instance of Unspents
            /// </summary>
            [JsonConstructor]
            public Unspents(List<Balance> balances, string address)
            {
                _balances = balances ?? new List<Balance>();
                _address = address ?? throw new ArgumentNullException(nameof(address));
            }
            
            /// <summary>
            /// Gets the total number of unspent outputs across all assets
            /// </summary>
            public int TotalUnspentCount => _balances?.Sum(b => b.UnspentTransactions.Count) ?? 0;
            
            /// <summary>
            /// Gets balance for a specific asset hash
            /// </summary>
            /// <param name="assetHash">Asset hash to find</param>
            /// <returns>Balance for the asset or null if not found</returns>
            public Balance? GetBalanceForAsset(string assetHash)
            {
                return _balances?.FirstOrDefault(b => b.AssetHash.Equals(assetHash, StringComparison.OrdinalIgnoreCase));
            }
            
            public bool Equals(Unspents other)
            {
                return _address == other._address && 
                       Balances.Count == other.Balances.Count;
            }
            
            public override bool Equals(object obj)
            {
                return obj is Unspents other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return HashCode.Combine(_address, _balances?.Count ?? 0);
            }
            
            public override string ToString()
            {
                return $"Unspents(Address: {_address}, Assets: {Balances.Count}, Total UTXOs: {TotalUnspentCount})";
            }
        }
        
        /// <summary>
        /// Represents the balance of a specific asset with its unspent outputs
        /// </summary>
        [Serializable]
        public struct Balance : IEquatable<Balance>
        {
            [JsonProperty("unspent")]
            [SerializeField] private List<UnspentTransaction> _unspentTransactions;
            
            [JsonProperty("assethash")]
            [SerializeField] private string _assetHash;
            
            [JsonProperty("asset")]
            [SerializeField] private string _assetName;
            
            [JsonProperty("asset_symbol")]
            [SerializeField] private string _assetSymbol;
            
            [JsonProperty("amount")]
            [SerializeField] private double _amount;
            
            /// <summary>
            /// Gets the list of unspent transaction outputs
            /// </summary>
            public List<UnspentTransaction> UnspentTransactions => _unspentTransactions ?? new List<UnspentTransaction>();
            
            /// <summary>
            /// Gets the asset hash
            /// </summary>
            public string AssetHash => _assetHash;
            
            /// <summary>
            /// Gets the asset name
            /// </summary>
            public string AssetName => _assetName;
            
            /// <summary>
            /// Gets the asset symbol
            /// </summary>
            public string AssetSymbol => _assetSymbol;
            
            /// <summary>
            /// Gets the total amount
            /// </summary>
            public double Amount => _amount;
            
            /// <summary>
            /// Gets the amount as decimal
            /// </summary>
            public decimal AmountDecimal => (decimal)_amount;
            
            /// <summary>
            /// Initializes a new instance of Balance
            /// </summary>
            [JsonConstructor]
            public Balance(
                List<UnspentTransaction> unspentTransactions, 
                string assetHash, 
                string assetName, 
                string assetSymbol, 
                double amount)
            {
                _unspentTransactions = unspentTransactions ?? new List<UnspentTransaction>();
                _assetHash = assetHash ?? throw new ArgumentNullException(nameof(assetHash));
                _assetName = assetName ?? "";
                _assetSymbol = assetSymbol ?? "";
                _amount = amount;
            }
            
            /// <summary>
            /// Gets whether this asset has any unspent outputs
            /// </summary>
            public bool HasUnspentOutputs => _unspentTransactions != null && _unspentTransactions.Count > 0;
            
            /// <summary>
            /// Gets the largest unspent output
            /// </summary>
            /// <returns>Largest unspent output or null if none</returns>
            public UnspentTransaction? GetLargestOutput()
            {
                if (!HasUnspentOutputs) return null;
                
                var maxOutput = _unspentTransactions[0];
                for (int i = 1; i < _unspentTransactions.Count; i++)
                {
                    if (_unspentTransactions[i].Value > maxOutput.Value)
                        maxOutput = _unspentTransactions[i];
                }
                return maxOutput;
            }
            
            public bool Equals(Balance other)
            {
                return _assetHash == other._assetHash && 
                       Math.Abs(_amount - other._amount) < 0.00000001 &&
                       UnspentTransactions.Count == other.UnspentTransactions.Count;
            }
            
            public override bool Equals(object obj)
            {
                return obj is Balance other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return HashCode.Combine(_assetHash, _amount, _unspentTransactions?.Count ?? 0);
            }
            
            public override string ToString()
            {
                return $"Balance({_assetSymbol}: {_amount}, UTXOs: {UnspentTransactions.Count})";
            }
        }
        
        /// <summary>
        /// Represents a single unspent transaction output
        /// </summary>
        [Serializable]
        public struct UnspentTransaction : IEquatable<UnspentTransaction>
        {
            [JsonProperty("txid")]
            [SerializeField] private string _txId;
            
            [JsonProperty("n")]
            [SerializeField] private int _index;
            
            [JsonProperty("value")]
            [SerializeField] private double _value;
            
            /// <summary>
            /// Gets the transaction ID
            /// </summary>
            public string TxId => _txId;
            
            /// <summary>
            /// Gets the output index
            /// </summary>
            public int Index => _index;
            
            /// <summary>
            /// Gets the output value
            /// </summary>
            public double Value => _value;
            
            /// <summary>
            /// Gets the value as decimal
            /// </summary>
            public decimal ValueDecimal => (decimal)_value;
            
            /// <summary>
            /// Initializes a new instance of UnspentTransaction
            /// </summary>
            [JsonConstructor]
            public UnspentTransaction(string txId, int index, double value)
            {
                _txId = txId ?? throw new ArgumentNullException(nameof(txId));
                _index = index;
                _value = value;
            }
            
            /// <summary>
            /// Gets the outpoint reference for this UTXO
            /// </summary>
            /// <returns>Formatted outpoint string</returns>
            public string GetOutPoint()
            {
                return $"{_txId}:{_index}";
            }
            
            public bool Equals(UnspentTransaction other)
            {
                return _txId == other._txId && 
                       _index == other._index && 
                       Math.Abs(_value - other._value) < 0.00000001;
            }
            
            public override bool Equals(object obj)
            {
                return obj is UnspentTransaction other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return HashCode.Combine(_txId, _index, _value);
            }
            
            public override string ToString()
            {
                return $"UnspentTx({GetOutPoint()}, {_value})";
            }
        }
    }
}