using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Response for getting token balances for an address
    /// </summary>
    [Serializable]
    public class NeoGetTokenBalancesResponse : NeoResponse<NeoGetTokenBalancesResponse.TokenBalances>
    {
        /// <summary>
        /// Gets the token balances result
        /// </summary>
        public TokenBalances TokenBalancesData => Result;
        
        /// <summary>
        /// Represents token balances for an address
        /// </summary>
        [Serializable]
        public struct TokenBalances : IEquatable<TokenBalances>
        {
            [JsonProperty("address")]
            [SerializeField] private string _address;
            
            [JsonProperty("balance")]
            [SerializeField] private List<TokenBalance> _balances;
            
            /// <summary>
            /// Gets the address for which balances are listed
            /// </summary>
            public string Address => _address;
            
            /// <summary>
            /// Gets the list of token balances
            /// </summary>
            public List<TokenBalance> Balances => _balances ?? new List<TokenBalance>();
            
            /// <summary>
            /// Initializes a new instance of TokenBalances
            /// </summary>
            [JsonConstructor]
            public TokenBalances(string address, List<TokenBalance> balances)
            {
                _address = address ?? throw new ArgumentNullException(nameof(address));
                _balances = balances ?? new List<TokenBalance>();
            }
            
            /// <summary>
            /// Gets the number of different tokens this address holds
            /// </summary>
            public int TokenCount => _balances?.Count ?? 0;
            
            /// <summary>
            /// Gets whether this address has any token balances
            /// </summary>
            public bool HasTokens => _balances != null && _balances.Count > 0;
            
            /// <summary>
            /// Gets balance for a specific token contract
            /// </summary>
            /// <param name="assetHash">Asset hash to find</param>
            /// <returns>Token balance or null if not found</returns>
            public TokenBalance? GetBalanceForToken(string assetHash)
            {
                return _balances?.FirstOrDefault(b => b.AssetHashString.Equals(assetHash, StringComparison.OrdinalIgnoreCase));
            }
            
            /// <summary>
            /// Gets balance for a specific token contract
            /// </summary>
            /// <param name="assetHash">Asset hash to find</param>
            /// <returns>Token balance or null if not found</returns>
            public TokenBalance? GetBalanceForToken(Hash160 assetHash)
            {
                return GetBalanceForToken(assetHash.ToString());
            }
            
            /// <summary>
            /// Gets all tokens with non-zero balances
            /// </summary>
            /// <returns>List of tokens with positive balances</returns>
            public List<TokenBalance> GetNonZeroBalances()
            {
                if (!HasTokens) return new List<TokenBalance>();
                return _balances.Where(b => b.Amount > 0).ToList();
            }
            
            public bool Equals(TokenBalances other)
            {
                return _address == other._address && 
                       TokenCount == other.TokenCount;
            }
            
            public override bool Equals(object obj)
            {
                return obj is TokenBalances other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return HashCode.Combine(_address, TokenCount);
            }
            
            public override string ToString()
            {
                return $"TokenBalances(Address: {_address}, Tokens: {TokenCount})";
            }
        }
        
        /// <summary>
        /// Represents a balance for a specific token
        /// </summary>
        [Serializable]
        public struct TokenBalance : IEquatable<TokenBalance>
        {
            [JsonProperty("assethash")]
            [SerializeField] private string _assetHash;
            
            [JsonProperty("amount")]
            [SerializeField] private string _amount;
            
            [JsonProperty("lastupdatedblock")]
            [SerializeField] private int _lastUpdatedBlock;
            
            /// <summary>
            /// Gets the asset hash as string
            /// </summary>
            public string AssetHashString => _assetHash;
            
            /// <summary>
            /// Gets the asset hash as Hash160
            /// </summary>
            public Hash160 AssetHash => new Hash160(_assetHash);
            
            /// <summary>
            /// Gets the amount as string (in smallest token units)
            /// </summary>
            public string AmountString => _amount;
            
            /// <summary>
            /// Gets the amount as long (in smallest token units)
            /// </summary>
            public long Amount => long.TryParse(_amount, out var result) ? result : 0;
            
            /// <summary>
            /// Gets the last updated block height
            /// </summary>
            public int LastUpdatedBlock => _lastUpdatedBlock;
            
            /// <summary>
            /// Gets the amount as decimal (assuming 8 decimal places)
            /// </summary>
            public decimal AmountDecimal => Amount / 100_000_000m;
            
            /// <summary>
            /// Initializes a new instance of TokenBalance
            /// </summary>
            [JsonConstructor]
            public TokenBalance(string assetHash, string amount, int lastUpdatedBlock)
            {
                _assetHash = assetHash ?? throw new ArgumentNullException(nameof(assetHash));
                _amount = amount ?? "0";
                _lastUpdatedBlock = lastUpdatedBlock;
            }
            
            /// <summary>
            /// Gets the amount with specified decimal places
            /// </summary>
            /// <param name="decimals">Number of decimal places</param>
            /// <returns>Amount as decimal</returns>
            public decimal GetAmountWithDecimals(int decimals)
            {
                var divisor = (decimal)Math.Pow(10, decimals);
                return Amount / divisor;
            }
            
            /// <summary>
            /// Gets whether this balance is positive
            /// </summary>
            public bool HasPositiveBalance => Amount > 0;
            
            /// <summary>
            /// Gets whether this balance was recently updated
            /// </summary>
            /// <param name="currentBlock">Current block height</param>
            /// <param name="threshold">Threshold in blocks</param>
            /// <returns>True if updated within threshold</returns>
            public bool IsRecentlyUpdated(int currentBlock, int threshold = 100)
            {
                return currentBlock - _lastUpdatedBlock <= threshold;
            }
            
            public bool Equals(TokenBalance other)
            {
                return _assetHash == other._assetHash && 
                       _amount == other._amount && 
                       _lastUpdatedBlock == other._lastUpdatedBlock;
            }
            
            public override bool Equals(object obj)
            {
                return obj is TokenBalance other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return HashCode.Combine(_assetHash, _amount, _lastUpdatedBlock);
            }
            
            public override string ToString()
            {
                return $"TokenBalance({_assetHash}: {AmountDecimal}, Block: {_lastUpdatedBlock})";
            }
        }
    }
}