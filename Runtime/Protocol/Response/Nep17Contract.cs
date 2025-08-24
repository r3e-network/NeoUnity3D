using System;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents a NEP-17 token contract with metadata
    /// </summary>
    [Serializable]
    public struct Nep17Contract : IEquatable<Nep17Contract>
    {
        [JsonProperty("scripthash")]
        [SerializeField] private string _scriptHash;
        
        [JsonProperty("symbol")]
        [SerializeField] private string _symbol;
        
        [JsonProperty("decimals")]
        [SerializeField] private int _decimals;
        
        /// <summary>
        /// Gets the contract script hash as string
        /// </summary>
        public string ScriptHashString => _scriptHash;
        
        /// <summary>
        /// Gets the contract script hash as Hash160
        /// </summary>
        public Hash160 ScriptHash => new Hash160(_scriptHash);
        
        /// <summary>
        /// Gets the token symbol
        /// </summary>
        public string Symbol => _symbol;
        
        /// <summary>
        /// Gets the number of decimal places for this token
        /// </summary>
        public int Decimals => _decimals;
        
        /// <summary>
        /// Gets the smallest unit for this token
        /// </summary>
        public decimal SmallestUnit => 1m / (decimal)Math.Pow(10, _decimals);
        
        /// <summary>
        /// Initializes a new instance of Nep17Contract
        /// </summary>
        /// <param name="scriptHash">Contract script hash as string</param>
        /// <param name="symbol">Token symbol</param>
        /// <param name="decimals">Number of decimal places</param>
        [JsonConstructor]
        public Nep17Contract(string scriptHash, string symbol, int decimals)
        {
            _scriptHash = scriptHash ?? throw new ArgumentNullException(nameof(scriptHash));
            _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            _decimals = decimals;
        }
        
        /// <summary>
        /// Initializes a new instance of Nep17Contract with Hash160
        /// </summary>
        /// <param name="scriptHash">Contract script hash</param>
        /// <param name="symbol">Token symbol</param>
        /// <param name="decimals">Number of decimal places</param>
        public Nep17Contract(Hash160 scriptHash, string symbol, int decimals)
        {
            _scriptHash = scriptHash?.ToString() ?? throw new ArgumentNullException(nameof(scriptHash));
            _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            _decimals = decimals;
        }
        
        /// <summary>
        /// Converts a raw token amount to decimal representation
        /// </summary>
        /// <param name="rawAmount">Raw amount in smallest units</param>
        /// <returns>Decimal amount</returns>
        public decimal FromRawAmount(long rawAmount)
        {
            var divisor = (decimal)Math.Pow(10, _decimals);
            return rawAmount / divisor;
        }
        
        /// <summary>
        /// Converts a decimal amount to raw token units
        /// </summary>
        /// <param name="decimalAmount">Decimal amount</param>
        /// <returns>Raw amount in smallest units</returns>
        public long ToRawAmount(decimal decimalAmount)
        {
            var multiplier = (decimal)Math.Pow(10, _decimals);
            return (long)(decimalAmount * multiplier);
        }
        
        /// <summary>
        /// Validates if the script hash format is correct
        /// </summary>
        /// <returns>True if script hash format is valid</returns>
        public bool IsValidScriptHash()
        {
            if (string.IsNullOrEmpty(_scriptHash))
                return false;
                
            try
            {
                var hash = new Hash160(_scriptHash);
                return !hash.IsZero();
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Validates if the decimals value is reasonable for a token
        /// </summary>
        /// <returns>True if decimals value is valid</returns>
        public bool IsValidDecimals()
        {
            return _decimals >= 0 && _decimals <= 18; // Common range for token decimals
        }
        
        /// <summary>
        /// Gets whether this is a likely stablecoin based on symbol
        /// </summary>
        public bool IsLikelyStablecoin()
        {
            if (string.IsNullOrEmpty(_symbol))
                return false;
                
            var upperSymbol = _symbol.ToUpperInvariant();
            return upperSymbol.Contains("USD") || 
                   upperSymbol == "USDT" || 
                   upperSymbol == "USDC" || 
                   upperSymbol == "DAI" ||
                   upperSymbol == "BUSD";
        }
        
        /// <summary>
        /// Gets whether this is a likely native token (NEO or GAS)
        /// </summary>
        public bool IsNativeToken()
        {
            if (string.IsNullOrEmpty(_symbol))
                return false;
                
            var upperSymbol = _symbol.ToUpperInvariant();
            return upperSymbol == "NEO" || upperSymbol == "GAS";
        }
        
        /// <summary>
        /// Validates all fields of this NEP-17 contract
        /// </summary>
        /// <returns>True if all fields are valid</returns>
        public bool IsValid()
        {
            return IsValidScriptHash() && 
                   !string.IsNullOrEmpty(_symbol) && 
                   IsValidDecimals();
        }
        
        /// <summary>
        /// Formats an amount with the token symbol
        /// </summary>
        /// <param name="amount">Amount to format</param>
        /// <returns>Formatted amount string</returns>
        public string FormatAmount(decimal amount)
        {
            return $"{amount:F{_decimals}} {_symbol}";
        }
        
        /// <summary>
        /// Formats a raw amount with the token symbol
        /// </summary>
        /// <param name="rawAmount">Raw amount in smallest units</param>
        /// <returns>Formatted amount string</returns>
        public string FormatRawAmount(long rawAmount)
        {
            var decimalAmount = FromRawAmount(rawAmount);
            return FormatAmount(decimalAmount);
        }
        
        public bool Equals(Nep17Contract other)
        {
            return _scriptHash == other._scriptHash && 
                   _symbol == other._symbol && 
                   _decimals == other._decimals;
        }
        
        public override bool Equals(object obj)
        {
            return obj is Nep17Contract other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_scriptHash, _symbol, _decimals);
        }
        
        public static bool operator ==(Nep17Contract left, Nep17Contract right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(Nep17Contract left, Nep17Contract right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"Nep17Contract({_symbol}, {_scriptHash}, {_decimals} decimals)";
        }
    }
}