using System;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Contracts
{
    /// <summary>
    /// Represents a token wrapper class that contains shared methods for both
    /// fungible NEP-17 and non-fungible NEP-11 token standards.
    /// Provides common token functionality like symbol, decimals, and total supply.
    /// </summary>
    [System.Serializable]
    public abstract class Token : SmartContract
    {
        #region Constants
        
        private const string TOTAL_SUPPLY = "totalSupply";
        private const string SYMBOL = "symbol";
        private const string DECIMALS = "decimals";
        
        #endregion
        
        #region Cached Properties
        
        [SerializeField]
        private int? totalSupply = null;
        
        [SerializeField]
        private int? decimals = null;
        
        [SerializeField]
        private string symbol = null;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Constructs a Token instance representing the token contract with the given script hash.
        /// </summary>
        /// <param name="scriptHash">The token contract's script hash</param>
        /// <param name="neoUnity">The NeoUnity instance to use for invocations</param>
        protected Token(Hash160 scriptHash, NeoUnity neoUnity) : base(scriptHash, neoUnity)
        {
        }
        
        /// <summary>
        /// Constructs a Token instance using the singleton NeoUnity instance.
        /// </summary>
        /// <param name="scriptHash">The token contract's script hash</param>
        protected Token(Hash160 scriptHash) : base(scriptHash)
        {
        }
        
        #endregion
        
        #region Token Metadata Methods
        
        /// <summary>
        /// Gets the total supply of this token in fractions.
        /// The return value is retrieved from the neo-node only once and then cached.
        /// </summary>
        /// <returns>The total supply in token fractions</returns>
        public async Task<int> GetTotalSupply()
        {
            if (totalSupply == null)
            {
                totalSupply = await CallFunctionReturningInt(TOTAL_SUPPLY);
                
                if (NeoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Token] Cached total supply for {ScriptHash}: {totalSupply}");
                }
            }
            
            return totalSupply.Value;
        }
        
        /// <summary>
        /// Gets the number of fractions that one unit of this token can be divided into.
        /// The return value is retrieved from the neo-node only once and then cached.
        /// </summary>
        /// <returns>The number of decimal places</returns>
        public async Task<int> GetDecimals()
        {
            if (decimals == null)
            {
                decimals = await CallFunctionReturningInt(DECIMALS);
                
                if (NeoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Token] Cached decimals for {ScriptHash}: {decimals}");
                }
            }
            
            return decimals.Value;
        }
        
        /// <summary>
        /// Gets the symbol of this token.
        /// The return value is retrieved from the neo-node only once and then cached.
        /// </summary>
        /// <returns>The token symbol</returns>
        public async Task<string> GetSymbol()
        {
            if (string.IsNullOrEmpty(symbol))
            {
                symbol = await CallFunctionReturningString(SYMBOL);
                
                if (NeoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Token] Cached symbol for {ScriptHash}: {symbol}");
                }
            }
            
            return symbol;
        }
        
        #endregion
        
        #region Token Conversion Methods
        
        /// <summary>
        /// Converts the token amount from a decimal point number to the amount in token fractions
        /// according to this token's number of decimals.
        /// Use this method to convert e.g. 1.5 GAS to its fraction value 150_000_000.
        /// </summary>
        /// <param name="amount">The token amount in decimals</param>
        /// <returns>The token amount in fractions</returns>
        public async Task<long> ToFractions(decimal amount)
        {
            var tokenDecimals = await GetDecimals();
            return ToFractions(amount, tokenDecimals);
        }
        
        /// <summary>
        /// Converts the token amount from a decimal point number to the amount in token fractions
        /// according to the specified number of decimals.
        /// Use this method to convert e.g. a token amount of 25.5 for a token with 4 decimals to 255_000.
        /// </summary>
        /// <param name="amount">The token amount in decimals</param>
        /// <param name="decimals">The number of decimals</param>
        /// <returns>The token amount in fractions</returns>
        public static long ToFractions(decimal amount, int decimals)
        {
            if (decimals < 0)
            {
                throw new ArgumentException("Decimals cannot be negative.", nameof(decimals));
            }
            
            // Check if the provided amount has too many decimal places
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(amount)[3])[2];
            if (decimalPlaces > decimals)
            {
                throw new ArgumentException($"The provided amount has too many decimal places. Make sure the decimals of the provided amount ({decimalPlaces}) do not exceed the supported token decimals ({decimals}).");
            }
            
            // Calculate the multiplier (10^decimals)
            var multiplier = (decimal)Math.Pow(10, decimals);
            var result = amount * multiplier;
            
            // Convert to long, checking for overflow
            if (result > long.MaxValue || result < long.MinValue)
            {
                throw new OverflowException("The calculated token fraction value exceeds the maximum supported range.");
            }
            
            return (long)result;
        }
        
        /// <summary>
        /// Converts the token amount from token fractions to its decimal point value
        /// according to this token's number of decimals.
        /// Use this method to convert e.g. 600_000 GAS fractions to its decimal value 0.006.
        /// </summary>
        /// <param name="amount">The token amount in fractions</param>
        /// <returns>The token amount in decimals</returns>
        public async Task<decimal> ToDecimals(long amount)
        {
            var tokenDecimals = await GetDecimals();
            return ToDecimals(amount, tokenDecimals);
        }
        
        /// <summary>
        /// Converts the token amount from token fractions to its decimal point value
        /// according to the specified number of decimals.
        /// Use this method to convert e.g. 600_000 token fractions to its decimal value 0.006 (with 8 decimals).
        /// </summary>
        /// <param name="amount">The token amount in fractions</param>
        /// <param name="decimals">The number of decimals</param>
        /// <returns>The token amount in decimals</returns>
        public static decimal ToDecimals(long amount, int decimals)
        {
            if (decimals < 0)
            {
                throw new ArgumentException("Decimals cannot be negative.", nameof(decimals));
            }
            
            if (decimals == 0)
            {
                return amount;
            }
            
            // Calculate the divisor (10^decimals)
            var divisor = (decimal)Math.Pow(10, decimals);
            return amount / divisor;
        }
        
        #endregion
        
        #region Neo Name Service (NNS) Support
        
        /// <summary>
        /// Resolves a Neo Name Service (NNS) text record to a Hash160 address.
        /// Used internally for NNS-based token operations.
        /// </summary>
        /// <param name="name">The NNS name to resolve</param>
        /// <returns>The resolved Hash160 address</returns>
        protected async Task<Hash160> ResolveNNSTextRecord(NNSName name)
        {
            try
            {
                var nnsService = new NeoNameService(NeoUnity);
                var resolvedAddress = await nnsService.Resolve(name, RecordType.TXT);
                return Hash160.FromAddress(resolvedAddress);
            }
            catch (Exception ex)
            {
                throw new ContractException($"Failed to resolve NNS name '{name}': {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Unity Serialization Support
        
        /// <summary>
        /// Clears cached token metadata. Useful when the token contract has been updated.
        /// </summary>
        [ContextMenu("Clear Token Cache")]
        public void ClearCache()
        {
            totalSupply = null;
            decimals = null;
            symbol = null;
            
            if (NeoUnity.Config.EnableDebugLogging)
            {
                Debug.Log($"[Token] Cleared cache for {ScriptHash}");
            }
        }
        
        /// <summary>
        /// Preloads token metadata for faster subsequent access.
        /// Useful to call during game initialization.
        /// </summary>
        public async Task PreloadMetadata()
        {
            try
            {
                await Task.WhenAll(
                    GetSymbol(),
                    GetDecimals(),
                    GetTotalSupply()
                );
                
                if (NeoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Token] Preloaded metadata for {ScriptHash}: {symbol}, {decimals} decimals, {totalSupply} total supply");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Token] Failed to preload metadata for {ScriptHash}: {ex.Message}");
                throw;
            }
        }
        
        #endregion
        
        #region Display Helpers
        
        /// <summary>
        /// Formats a token amount for display with proper decimal places and symbol.
        /// </summary>
        /// <param name="fractionAmount">The amount in token fractions</param>
        /// <param name="includeSymbol">Whether to include the token symbol</param>
        /// <returns>The formatted amount string</returns>
        public async Task<string> FormatAmount(long fractionAmount, bool includeSymbol = true)
        {
            var decimalAmount = await ToDecimals(fractionAmount);
            var tokenSymbol = includeSymbol ? await GetSymbol() : "";
            
            // Format with appropriate decimal places
            var tokenDecimals = await GetDecimals();
            var formatString = tokenDecimals > 0 ? $"F{Math.Min(tokenDecimals, 8)}" : "F0"; // Limit to 8 decimal places for display
            
            var formattedAmount = decimalAmount.ToString(formatString);
            
            return includeSymbol ? $"{formattedAmount} {tokenSymbol}" : formattedAmount;
        }
        
        /// <summary>
        /// Gets a string representation of this token for debugging.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"Token(ScriptHash: {ScriptHash}, Symbol: {symbol ?? "Unknown"}, Decimals: {decimals?.ToString() ?? "Unknown"})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Represents a Neo Name Service (NNS) name for use in token operations.
    /// </summary>
    [System.Serializable]
    public class NNSName
    {
        /// <summary>The NNS domain name</summary>
        [SerializeField]
        public string Name { get; set; }
        
        /// <summary>
        /// Creates a new NNS name.
        /// </summary>
        /// <param name="name">The domain name</param>
        public NNSName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("NNS name cannot be null or empty.", nameof(name));
            }
            
            Name = name;
        }
        
        /// <summary>
        /// Implicit conversion from string to NNSName.
        /// </summary>
        /// <param name="name">The domain name</param>
        public static implicit operator NNSName(string name)
        {
            return new NNSName(name);
        }
        
        /// <summary>
        /// String representation of the NNS name.
        /// </summary>
        /// <returns>The domain name</returns>
        public override string ToString()
        {
            return Name;
        }
    }
    
    /// <summary>
    /// DNS record types supported by Neo Name Service.
    /// </summary>
    public enum RecordType
    {
        /// <summary>IPv4 address record</summary>
        A = 1,
        
        /// <summary>Canonical name record</summary>
        CNAME = 5,
        
        /// <summary>Text record</summary>
        TXT = 16,
        
        /// <summary>IPv6 address record</summary>
        AAAA = 28
    }
}