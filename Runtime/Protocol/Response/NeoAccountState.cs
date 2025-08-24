using System;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Cryptography;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents a Neo account state with balance and voting information
    /// </summary>
    [Serializable]
    public struct NeoAccountState : IEquatable<NeoAccountState>
    {
        [JsonProperty("balance")]
        [SerializeField] private long _balance;
        
        [JsonProperty("balanceHeight")]
        [SerializeField] private int? _balanceHeight;
        
        [JsonProperty("voteTo")]
        [SerializeField] private string _publicKey;
        
        /// <summary>
        /// Gets the NEO balance
        /// </summary>
        public long Balance => _balance;
        
        /// <summary>
        /// Gets the height at which the balance was last updated
        /// </summary>
        public int? BalanceHeight => _balanceHeight;
        
        /// <summary>
        /// Gets the public key this account is voting to (if any)
        /// </summary>
        public string PublicKeyString => _publicKey;
        
        /// <summary>
        /// Gets whether this account is currently voting
        /// </summary>
        public bool IsVoting => !string.IsNullOrEmpty(_publicKey);
        
        /// <summary>
        /// Initializes a new instance of NeoAccountState
        /// </summary>
        /// <param name="balance">NEO balance</param>
        /// <param name="balanceHeight">Balance height</param>
        /// <param name="publicKey">Public key being voted to</param>
        [JsonConstructor]
        public NeoAccountState(long balance, int? balanceHeight, string publicKey)
        {
            _balance = balance;
            _balanceHeight = balanceHeight;
            _publicKey = publicKey;
        }
        
        /// <summary>
        /// Creates an account state with no vote
        /// </summary>
        /// <param name="balance">NEO balance</param>
        /// <param name="updateHeight">Update height</param>
        /// <returns>Account state with no vote</returns>
        public static NeoAccountState WithNoVote(long balance, int updateHeight)
        {
            return new NeoAccountState(balance, updateHeight, null);
        }
        
        /// <summary>
        /// Creates an account state with no balance
        /// </summary>
        /// <returns>Empty account state</returns>
        public static NeoAccountState WithNoBalance()
        {
            return new NeoAccountState(0, null, null);
        }
        
        /// <summary>
        /// Gets the public key as ECPublicKey if valid
        /// </summary>
        /// <returns>ECPublicKey or null if invalid/empty</returns>
        public ECPublicKey GetPublicKey()
        {
            if (string.IsNullOrEmpty(_publicKey))
                return null;
                
            try
            {
                return new ECPublicKey(_publicKey);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Gets the balance as decimal NEO (dividing by 100,000,000)
        /// </summary>
        /// <returns>Balance in decimal NEO</returns>
        public decimal GetBalanceAsDecimal()
        {
            return _balance / 100_000_000m;
        }
        
        public bool Equals(NeoAccountState other)
        {
            return _balance == other._balance && 
                   _balanceHeight == other._balanceHeight && 
                   _publicKey == other._publicKey;
        }
        
        public override bool Equals(object obj)
        {
            return obj is NeoAccountState other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_balance, _balanceHeight, _publicKey);
        }
        
        public static bool operator ==(NeoAccountState left, NeoAccountState right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(NeoAccountState left, NeoAccountState right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            var votingInfo = IsVoting ? $", VoteTo: {_publicKey}" : ", No Vote";
            return $"NeoAccountState(Balance: {GetBalanceAsDecimal()} NEO, Height: {_balanceHeight}{votingInfo})";
        }
    }
}