using System;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents a token transfer instruction for building transactions
    /// </summary>
    [Serializable]
    public struct TransactionSendToken : IEquatable<TransactionSendToken>
    {
        [JsonProperty("asset")]
        [SerializeField] private string _token;
        
        [JsonProperty("value")]
        [SerializeField] private long _value;
        
        [JsonProperty("address")]
        [SerializeField] private string _address;
        
        /// <summary>
        /// Gets the token contract hash as string
        /// </summary>
        public string TokenString => _token;
        
        /// <summary>
        /// Gets the token contract hash as Hash160
        /// </summary>
        public Hash160 Token => new Hash160(_token);
        
        /// <summary>
        /// Gets the token amount in smallest units
        /// </summary>
        public long Value => _value;
        
        /// <summary>
        /// Gets the recipient address
        /// </summary>
        public string Address => _address;
        
        /// <summary>
        /// Gets the token amount as decimal (assuming 8 decimal places)
        /// </summary>
        public decimal ValueDecimal => _value / 100_000_000m;
        
        /// <summary>
        /// Initializes a new instance of TransactionSendToken
        /// </summary>
        /// <param name="token">Token contract hash as string</param>
        /// <param name="value">Token amount in smallest units</param>
        /// <param name="address">Recipient address</param>
        [JsonConstructor]
        public TransactionSendToken(string token, long value, string address)
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _value = value;
            _address = address ?? throw new ArgumentNullException(nameof(address));
        }
        
        /// <summary>
        /// Initializes a new instance of TransactionSendToken with Hash160
        /// </summary>
        /// <param name="token">Token contract hash</param>
        /// <param name="value">Token amount in smallest units</param>
        /// <param name="address">Recipient address</param>
        public TransactionSendToken(Hash160 token, long value, string address)
        {
            _token = token?.ToString() ?? throw new ArgumentNullException(nameof(token));
            _value = value;
            _address = address ?? throw new ArgumentNullException(nameof(address));
        }
        
        /// <summary>
        /// Creates a TransactionSendToken with decimal amount
        /// </summary>
        /// <param name="token">Token contract hash</param>
        /// <param name="decimalValue">Token amount as decimal</param>
        /// <param name="address">Recipient address</param>
        /// <param name="decimals">Token decimals (default: 8)</param>
        /// <returns>TransactionSendToken instance</returns>
        public static TransactionSendToken FromDecimal(Hash160 token, decimal decimalValue, string address, int decimals = 8)
        {
            var multiplier = (decimal)Math.Pow(10, decimals);
            var value = (long)(decimalValue * multiplier);
            return new TransactionSendToken(token, value, address);
        }
        
        /// <summary>
        /// Creates a TransactionSendToken with decimal amount
        /// </summary>
        /// <param name="tokenHash">Token contract hash as string</param>
        /// <param name="decimalValue">Token amount as decimal</param>
        /// <param name="address">Recipient address</param>
        /// <param name="decimals">Token decimals (default: 8)</param>
        /// <returns>TransactionSendToken instance</returns>
        public static TransactionSendToken FromDecimal(string tokenHash, decimal decimalValue, string address, int decimals = 8)
        {
            var multiplier = (decimal)Math.Pow(10, decimals);
            var value = (long)(decimalValue * multiplier);
            return new TransactionSendToken(tokenHash, value, address);
        }
        
        /// <summary>
        /// Validates if the token hash format is correct
        /// </summary>
        /// <returns>True if token hash format is valid</returns>
        public bool IsValidTokenHash()
        {
            if (string.IsNullOrEmpty(_token))
                return false;
                
            try
            {
                var hash = new Hash160(_token);
                return !hash.IsZero();
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Validates if the address format is correct
        /// </summary>
        /// <returns>True if address format is valid</returns>
        public bool IsValidAddress()
        {
            if (string.IsNullOrEmpty(_address))
                return false;
                
            // Neo addresses start with 'N' and are 34 characters long
            return _address.Length == 34 && _address.StartsWith('N');
        }
        
        /// <summary>
        /// Gets whether the transfer amount is positive
        /// </summary>
        public bool HasPositiveValue => _value > 0;
        
        /// <summary>
        /// Gets the token amount with specified decimal places
        /// </summary>
        /// <param name="decimals">Number of decimal places</param>
        /// <returns>Token amount as decimal</returns>
        public decimal GetValueWithDecimals(int decimals)
        {
            var divisor = (decimal)Math.Pow(10, decimals);
            return _value / divisor;
        }
        
        /// <summary>
        /// Validates all fields of this TransactionSendToken
        /// </summary>
        /// <returns>True if all fields are valid</returns>
        public bool IsValid()
        {
            return IsValidTokenHash() && IsValidAddress() && HasPositiveValue;
        }
        
        public bool Equals(TransactionSendToken other)
        {
            return _token == other._token && 
                   _value == other._value && 
                   _address == other._address;
        }
        
        public override bool Equals(object obj)
        {
            return obj is TransactionSendToken other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_token, _value, _address);
        }
        
        public static bool operator ==(TransactionSendToken left, TransactionSendToken right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(TransactionSendToken left, TransactionSendToken right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"TransactionSendToken(Token: {_token}, Amount: {ValueDecimal}, To: {_address})";
        }
    }
}