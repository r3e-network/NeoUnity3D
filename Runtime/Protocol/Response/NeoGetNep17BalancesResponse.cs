using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Response for getting NEP-17 token balances for an address.
    /// Contains all NEP-17 token balances and metadata for a specific address.
    /// </summary>
    [System.Serializable]
    public class NeoGetNep17BalancesResponse : NeoResponse<Nep17Balances>
    {
        /// <summary>
        /// Gets the NEP-17 balances from the response.
        /// </summary>
        /// <returns>NEP-17 balances or null if response failed</returns>
        public Nep17Balances GetBalances()
        {
            return IsSuccess ? Result : null;
        }
        
        /// <summary>
        /// Gets the NEP-17 balances or throws if the response failed.
        /// </summary>
        /// <returns>NEP-17 balances</returns>
        /// <exception cref="NeoRpcException">If the response contains an error</exception>
        public Nep17Balances GetBalancesOrThrow()
        {
            return GetResult();
        }
    }
    
    /// <summary>
    /// Represents all NEP-17 token balances for a specific address.
    /// </summary>
    [System.Serializable]
    public class Nep17Balances
    {
        #region Properties
        
        /// <summary>The address these balances belong to</summary>
        [JsonProperty("address")]
        public string Address { get; set; }
        
        /// <summary>List of NEP-17 token balances</summary>
        [JsonProperty("balance")]
        public List<Nep17Balance> Balances { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public Nep17Balances()
        {
            Balances = new List<Nep17Balance>();
        }
        
        /// <summary>
        /// Creates new NEP-17 balances.
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="balances">The token balances</param>
        public Nep17Balances(string address, List<Nep17Balance> balances = null)
        {
            Address = address;
            Balances = balances ?? new List<Nep17Balance>();
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether there are any balances</summary>
        [JsonIgnore]
        public bool HasBalances => Balances != null && Balances.Count > 0;
        
        /// <summary>Number of different tokens held</summary>
        [JsonIgnore]
        public int TokenCount => Balances?.Count ?? 0;
        
        /// <summary>Number of tokens with non-zero balance</summary>
        [JsonIgnore]
        public int NonZeroTokenCount => Balances?.Count(b => b.HasNonZeroBalance) ?? 0;
        
        #endregion
        
        #region Balance Operations
        
        /// <summary>
        /// Gets the balance for a specific token.
        /// </summary>
        /// <param name="assetHash">The token contract hash</param>
        /// <returns>The balance or null if not found</returns>
        public Nep17Balance GetBalance(Hash160 assetHash)
        {
            if (assetHash == null || !HasBalances)
                return null;
            
            return Balances.FirstOrDefault(b => b.AssetHash?.Equals(assetHash) == true);
        }
        
        /// <summary>
        /// Gets the balance for a specific token.
        /// </summary>
        /// <param name="assetHashString">The token contract hash as string</param>
        /// <returns>The balance or null if not found</returns>
        public Nep17Balance GetBalance(string assetHashString)
        {
            if (string.IsNullOrEmpty(assetHashString) || !HasBalances)
                return null;
            
            return Balances.FirstOrDefault(b => b.AssetHash?.ToString() == assetHashString);
        }
        
        /// <summary>
        /// Gets the balance for a token by symbol.
        /// </summary>
        /// <param name="symbol">The token symbol</param>
        /// <returns>The balance or null if not found</returns>
        public Nep17Balance GetBalanceBySymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol) || !HasBalances)
                return null;
            
            return Balances.FirstOrDefault(b => b.Symbol?.Equals(symbol, StringComparison.OrdinalIgnoreCase) == true);
        }
        
        /// <summary>
        /// Gets the balance for a token by name.
        /// </summary>
        /// <param name="name">The token name</param>
        /// <returns>The balance or null if not found</returns>
        public Nep17Balance GetBalanceByName(string name)
        {
            if (string.IsNullOrEmpty(name) || !HasBalances)
                return null;
            
            return Balances.FirstOrDefault(b => b.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
        }
        
        /// <summary>
        /// Gets all tokens with non-zero balances.
        /// </summary>
        /// <returns>List of non-zero balances</returns>
        public List<Nep17Balance> GetNonZeroBalances()
        {
            if (!HasBalances)
                return new List<Nep17Balance>();
            
            return Balances.Where(b => b.HasNonZeroBalance).ToList();
        }
        
        /// <summary>
        /// Gets all tokens with zero balances.
        /// </summary>
        /// <returns>List of zero balances</returns>
        public List<Nep17Balance> GetZeroBalances()
        {
            if (!HasBalances)
                return new List<Nep17Balance>();
            
            return Balances.Where(b => !b.HasNonZeroBalance).ToList();
        }
        
        /// <summary>
        /// Gets all unique token symbols.
        /// </summary>
        /// <returns>List of token symbols</returns>
        public List<string> GetTokenSymbols()
        {
            if (!HasBalances)
                return new List<string>();
            
            return Balances.Where(b => !string.IsNullOrEmpty(b.Symbol))
                          .Select(b => b.Symbol)
                          .Distinct()
                          .ToList();
        }
        
        /// <summary>
        /// Gets balances sorted by amount (descending).
        /// </summary>
        /// <returns>Balances sorted by amount</returns>
        public List<Nep17Balance> GetBalancesSortedByAmount()
        {
            if (!HasBalances)
                return new List<Nep17Balance>();
            
            return Balances.OrderByDescending(b => b.GetAmountDecimal()).ToList();
        }
        
        /// <summary>
        /// Gets balances sorted by last update (most recent first).
        /// </summary>
        /// <returns>Balances sorted by last update</returns>
        public List<Nep17Balance> GetBalancesSortedByLastUpdate()
        {
            if (!HasBalances)
                return new List<Nep17Balance>();
            
            return Balances.OrderByDescending(b => b.LastUpdatedBlock).ToList();
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates these NEP-17 balances.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Address))
                throw new InvalidOperationException("NEP-17 balances address cannot be null or empty.");
            
            if (Balances != null)
            {
                foreach (var balance in Balances)
                {
                    balance.Validate();
                }
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of these balances.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"NEP-17 Balances for {Address}:\n";
            result += $"  Total Tokens: {TokenCount}\n";
            result += $"  Non-Zero Tokens: {NonZeroTokenCount}\n\n";
            
            if (HasBalances)
            {
                var nonZeroBalances = GetNonZeroBalances();
                foreach (var balance in nonZeroBalances)
                {
                    result += $"  {balance.Symbol ?? balance.Name ?? "Unknown"}: {balance.GetFormattedAmount()}\n";
                }
                
                var zeroCount = TokenCount - nonZeroBalances.Count;
                if (zeroCount > 0)
                {
                    result += $"  ... and {zeroCount} tokens with zero balance\n";
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of these balances.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"Nep17Balances(Address: {Address}, Tokens: {TokenCount}, NonZero: {NonZeroTokenCount})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a single NEP-17 token balance.
    /// </summary>
    [System.Serializable]
    public class Nep17Balance
    {
        #region Properties
        
        /// <summary>The name of the token</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>The symbol of the token</summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        
        /// <summary>The number of decimals the token uses</summary>
        [JsonProperty("decimals")]
        public string Decimals { get; set; }
        
        /// <summary>The balance amount as a string</summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }
        
        /// <summary>The block number when this balance was last updated</summary>
        [JsonProperty("lastupdatedblock")]
        public long LastUpdatedBlock { get; set; }
        
        /// <summary>The hash of the token contract</summary>
        [JsonProperty("assethash")]
        public Hash160 AssetHash { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public Nep17Balance()
        {
        }
        
        /// <summary>
        /// Creates a new NEP-17 balance.
        /// </summary>
        /// <param name="name">Token name</param>
        /// <param name="symbol">Token symbol</param>
        /// <param name="decimals">Token decimals</param>
        /// <param name="amount">Balance amount</param>
        /// <param name="lastUpdatedBlock">Last updated block</param>
        /// <param name="assetHash">Asset hash</param>
        public Nep17Balance(string name, string symbol, string decimals, string amount, long lastUpdatedBlock, Hash160 assetHash)
        {
            Name = name;
            Symbol = symbol;
            Decimals = decimals;
            Amount = amount;
            LastUpdatedBlock = lastUpdatedBlock;
            AssetHash = assetHash;
        }
        
        #endregion
        
        #region Properties (Computed)
        
        /// <summary>Whether this balance has a non-zero amount</summary>
        [JsonIgnore]
        public bool HasNonZeroBalance => GetAmountDecimal() > 0;
        
        /// <summary>Whether this balance has token metadata</summary>
        [JsonIgnore]
        public bool HasMetadata => !string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Symbol);
        
        /// <summary>The display name of this token</summary>
        [JsonIgnore]
        public string DisplayName => Symbol ?? Name ?? $"Token_{AssetHash?.ToString()?.Substring(0, 8)}";
        
        #endregion
        
        #region Amount Methods
        
        /// <summary>
        /// Gets the balance amount as a decimal.
        /// </summary>
        /// <returns>Balance amount as decimal</returns>
        public decimal GetAmountDecimal()
        {
            if (string.IsNullOrEmpty(Amount))
                return 0;
            
            if (decimal.TryParse(Amount, out var amount))
                return amount;
            
            return 0;
        }
        
        /// <summary>
        /// Gets the number of decimals as an integer.
        /// </summary>
        /// <returns>Number of decimals</returns>
        public int GetDecimalsInt()
        {
            if (string.IsNullOrEmpty(Decimals))
                return 0;
            
            if (int.TryParse(Decimals, out var decimals))
                return decimals;
            
            return 0;
        }
        
        /// <summary>
        /// Gets the balance amount formatted with appropriate decimals.
        /// </summary>
        /// <returns>Formatted balance amount</returns>
        public string GetFormattedAmount()
        {
            var amount = GetAmountDecimal();
            var decimals = GetDecimalsInt();
            
            if (decimals > 0)
            {
                var divisor = (decimal)Math.Pow(10, decimals);
                var formattedAmount = amount / divisor;
                return formattedAmount.ToString($"F{Math.Min(decimals, 8)}");
            }
            
            return amount.ToString();
        }
        
        /// <summary>
        /// Gets the balance amount as the smallest unit (wei/satoshi equivalent).
        /// </summary>
        /// <returns>Balance in smallest unit</returns>
        public decimal GetAmountInSmallestUnit()
        {
            return GetAmountDecimal();
        }
        
        /// <summary>
        /// Gets the balance amount as the standard unit (considering decimals).
        /// </summary>
        /// <returns>Balance in standard unit</returns>
        public decimal GetAmountInStandardUnit()
        {
            var amount = GetAmountDecimal();
            var decimals = GetDecimalsInt();
            
            if (decimals > 0)
            {
                var divisor = (decimal)Math.Pow(10, decimals);
                return amount / divisor;
            }
            
            return amount;
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Validates this NEP-17 balance.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (AssetHash == null)
                throw new InvalidOperationException("NEP-17 balance asset hash cannot be null.");
            
            if (string.IsNullOrEmpty(Amount))
                throw new InvalidOperationException("NEP-17 balance amount cannot be null or empty.");
            
            if (LastUpdatedBlock < 0)
                throw new InvalidOperationException("NEP-17 balance last updated block cannot be negative.");
            
            // Validate amount is a valid number
            if (!decimal.TryParse(Amount, out _))
                throw new InvalidOperationException($"NEP-17 balance amount is not a valid number: {Amount}");
            
            // Validate decimals if provided
            if (!string.IsNullOrEmpty(Decimals) && !int.TryParse(Decimals, out var decimals))
                throw new InvalidOperationException($"NEP-17 balance decimals is not a valid integer: {Decimals}");
        }
        
        #endregion
        
        #region Comparison Methods
        
        /// <summary>
        /// Compares this balance to another balance by amount.
        /// </summary>
        /// <param name="other">The other balance</param>
        /// <returns>Comparison result</returns>
        public int CompareAmountTo(Nep17Balance other)
        {
            if (other == null)
                return 1;
            
            return GetAmountDecimal().CompareTo(other.GetAmountDecimal());
        }
        
        /// <summary>
        /// Compares this balance to another balance by last update.
        /// </summary>
        /// <param name="other">The other balance</param>
        /// <returns>Comparison result</returns>
        public int CompareLastUpdateTo(Nep17Balance other)
        {
            if (other == null)
                return 1;
            
            return LastUpdatedBlock.CompareTo(other.LastUpdatedBlock);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this balance.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"NEP-17 Balance:\n";
            result += $"  Token: {DisplayName}\n";
            result += $"  Asset Hash: {AssetHash}\n";
            result += $"  Amount: {GetFormattedAmount()}\n";
            result += $"  Decimals: {Decimals ?? "0"}\n";
            result += $"  Last Updated: Block #{LastUpdatedBlock}\n";
            
            if (!string.IsNullOrEmpty(Name))
            {
                result += $"  Name: {Name}\n";
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this balance.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var formattedAmount = GetFormattedAmount();
            return $"Nep17Balance({DisplayName}: {formattedAmount})";
        }
        
        #endregion
    }
}