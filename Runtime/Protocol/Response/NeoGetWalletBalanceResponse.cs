using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Response for NEO getwalletbalance RPC method
    /// </summary>
    [Serializable]
    public class NeoGetWalletBalanceResponse : NeoResponse<NeoGetWalletBalanceResponse.WalletBalance>
    {
        /// <summary>
        /// Gets the wallet balance result
        /// </summary>
        public WalletBalance Balance => Result;
        
        /// <summary>
        /// Represents a wallet balance for a specific asset
        /// </summary>
        [Serializable]
        public struct WalletBalance : IEquatable<WalletBalance>
        {
            [JsonProperty("balance")]
            [SerializeField] private string _balance;
            
            [JsonProperty("Balance")]
            [SerializeField] private string _balanceAlt;
            
            /// <summary>
            /// Gets the balance as string
            /// </summary>
            public string BalanceString => !string.IsNullOrEmpty(_balance) ? _balance : _balanceAlt ?? "0";
            
            /// <summary>
            /// Gets the balance as decimal
            /// </summary>
            public decimal BalanceDecimal => decimal.TryParse(BalanceString, out var result) ? result : 0m;
            
            /// <summary>
            /// Gets the balance as double for compatibility
            /// </summary>
            public double BalanceDouble => (double)BalanceDecimal;
            
            /// <summary>
            /// Initializes a new instance of WalletBalance
            /// </summary>
            /// <param name="balance">Balance value as string</param>
            [JsonConstructor]
            public WalletBalance(string balance)
            {
                _balance = balance;
                _balanceAlt = null;
            }
            
            /// <summary>
            /// Initializes a new instance of WalletBalance for alternative field
            /// </summary>
            /// <param name="balance">Balance value as string</param>
            /// <param name="balanceAlt">Alternative balance field</param>
            internal WalletBalance(string balance, string balanceAlt)
            {
                _balance = balance;
                _balanceAlt = balanceAlt;
            }
            
            /// <summary>
            /// Creates a WalletBalance with zero balance
            /// </summary>
            /// <returns>Zero balance instance</returns>
            public static WalletBalance Zero()
            {
                return new WalletBalance("0");
            }
            
            /// <summary>
            /// Creates a WalletBalance from decimal value
            /// </summary>
            /// <param name="balance">Balance as decimal</param>
            /// <returns>WalletBalance instance</returns>
            public static WalletBalance FromDecimal(decimal balance)
            {
                return new WalletBalance(balance.ToString());
            }
            
            /// <summary>
            /// Gets whether the balance is positive
            /// </summary>
            public bool HasPositiveBalance => BalanceDecimal > 0;
            
            /// <summary>
            /// Gets whether the balance is zero
            /// </summary>
            public bool IsZero => BalanceDecimal == 0;
            
            /// <summary>
            /// Formats the balance with specified decimal places
            /// </summary>
            /// <param name="decimals">Number of decimal places</param>
            /// <returns>Formatted balance string</returns>
            public string FormatBalance(int decimals = 8)
            {
                return BalanceDecimal.ToString($"F{decimals}");
            }
            
            /// <summary>
            /// Gets the balance in smallest units (assuming 8 decimals)
            /// </summary>
            /// <returns>Balance in smallest units</returns>
            public long GetSmallestUnits()
            {
                return (long)(BalanceDecimal * 100_000_000);
            }
            
            /// <summary>
            /// Gets the balance in smallest units with specified decimals
            /// </summary>
            /// <param name="decimals">Number of decimal places</param>
            /// <returns>Balance in smallest units</returns>
            public long GetSmallestUnits(int decimals)
            {
                var multiplier = (decimal)Math.Pow(10, decimals);
                return (long)(BalanceDecimal * multiplier);
            }
            
            /// <summary>
            /// Compares this balance with another balance
            /// </summary>
            /// <param name="other">Balance to compare with</param>
            /// <returns>Comparison result (-1, 0, 1)</returns>
            public int CompareTo(WalletBalance other)
            {
                return BalanceDecimal.CompareTo(other.BalanceDecimal);
            }
            
            /// <summary>
            /// Adds two wallet balances
            /// </summary>
            /// <param name="left">First balance</param>
            /// <param name="right">Second balance</param>
            /// <returns>Sum of balances</returns>
            public static WalletBalance operator +(WalletBalance left, WalletBalance right)
            {
                return FromDecimal(left.BalanceDecimal + right.BalanceDecimal);
            }
            
            /// <summary>
            /// Subtracts two wallet balances
            /// </summary>
            /// <param name="left">First balance</param>
            /// <param name="right">Second balance</param>
            /// <returns>Difference of balances</returns>
            public static WalletBalance operator -(WalletBalance left, WalletBalance right)
            {
                return FromDecimal(Math.Max(0, left.BalanceDecimal - right.BalanceDecimal));
            }
            
            public bool Equals(WalletBalance other)
            {
                return BalanceDecimal == other.BalanceDecimal;
            }
            
            public override bool Equals(object obj)
            {
                return obj is WalletBalance other && Equals(other);
            }
            
            public override int GetHashCode()
            {
                return BalanceDecimal.GetHashCode();
            }
            
            public static bool operator ==(WalletBalance left, WalletBalance right)
            {
                return left.Equals(right);
            }
            
            public static bool operator !=(WalletBalance left, WalletBalance right)
            {
                return !left.Equals(right);
            }
            
            public static bool operator >(WalletBalance left, WalletBalance right)
            {
                return left.BalanceDecimal > right.BalanceDecimal;
            }
            
            public static bool operator <(WalletBalance left, WalletBalance right)
            {
                return left.BalanceDecimal < right.BalanceDecimal;
            }
            
            public static bool operator >=(WalletBalance left, WalletBalance right)
            {
                return left.BalanceDecimal >= right.BalanceDecimal;
            }
            
            public static bool operator <=(WalletBalance left, WalletBalance right)
            {
                return left.BalanceDecimal <= right.BalanceDecimal;
            }
            
            public override string ToString()
            {
                return $"WalletBalance({BalanceDecimal})";
            }
        }
    }
}