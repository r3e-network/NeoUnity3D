using System;
using UnityEngine;
using Neo.Unity.SDK.Types;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Protocol.Response
{
    /// <summary>
    /// Represents the state of a Neo Name Service (NNS) domain name.
    /// Contains information about domain registration, ownership, and expiration.
    /// </summary>
    [System.Serializable]
    public class NameState
    {
        #region Properties
        
        /// <summary>The domain name</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>The owner of the domain</summary>
        [JsonProperty("owner")]
        public Hash160 Owner { get; set; }
        
        /// <summary>The administrator of the domain</summary>
        [JsonProperty("admin")]
        public Hash160 Admin { get; set; }
        
        /// <summary>The expiration block height</summary>
        [JsonProperty("expiration")]
        public long Expiration { get; set; }
        
        /// <summary>Whether the domain is currently registered</summary>
        [JsonProperty("registered")]
        public bool IsRegistered { get; set; }
        
        /// <summary>The registration block height</summary>
        [JsonProperty("registered_at")]
        public long RegisteredAt { get; set; }
        
        /// <summary>The last renewal block height</summary>
        [JsonProperty("renewed_at")]
        public long RenewedAt { get; set; }
        
        /// <summary>Additional metadata</summary>
        [JsonProperty("metadata")]
        public string Metadata { get; set; }
        
        #endregion
        
        #region Computed Properties
        
        /// <summary>Whether the domain has expired</summary>
        [JsonIgnore]
        public bool IsExpired { get; set; } // Would need current block height to calculate
        
        /// <summary>Whether the domain has an administrator</summary>
        [JsonIgnore]
        public bool HasAdmin => Admin != null && !Admin.Equals(Hash160.ZERO);
        
        /// <summary>Whether the domain has an owner</summary>
        [JsonIgnore]
        public bool HasOwner => Owner != null && !Owner.Equals(Hash160.ZERO);
        
        /// <summary>Whether the domain is valid and registered</summary>
        [JsonIgnore]
        public bool IsValid => IsRegistered && HasOwner && !IsExpired;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new name state with the specified parameters.
        /// </summary>
        /// <param name="name">The domain name</param>
        /// <param name="owner">The owner address</param>
        /// <param name="admin">The admin address</param>
        /// <param name="expiration">The expiration block</param>
        /// <param name="isRegistered">Whether registered</param>
        /// <param name="registeredAt">Registration block</param>
        /// <param name="renewedAt">Last renewal block</param>
        /// <param name="metadata">Additional metadata</param>
        public NameState(string name, Hash160 owner, Hash160 admin, long expiration, 
                        bool isRegistered, long registeredAt, long renewedAt, string metadata = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Owner = owner;
            Admin = admin;
            Expiration = expiration;
            IsRegistered = isRegistered;
            RegisteredAt = registeredAt;
            RenewedAt = renewedAt;
            Metadata = metadata;
        }
        
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public NameState()
        {
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Checks if the domain is expired at the specified block height.
        /// </summary>
        /// <param name="currentBlockHeight">The current block height</param>
        /// <returns>True if expired</returns>
        public bool IsExpiredAt(long currentBlockHeight)
        {
            return IsRegistered && Expiration <= currentBlockHeight;
        }
        
        /// <summary>
        /// Gets the number of blocks until expiration.
        /// </summary>
        /// <param name="currentBlockHeight">The current block height</param>
        /// <returns>Blocks until expiration (negative if already expired)</returns>
        public long BlocksUntilExpiration(long currentBlockHeight)
        {
            return Expiration - currentBlockHeight;
        }
        
        /// <summary>
        /// Checks if the specified address is the owner of this domain.
        /// </summary>
        /// <param name="address">The address to check</param>
        /// <returns>True if the address is the owner</returns>
        public bool IsOwnedBy(Hash160 address)
        {
            return Owner != null && Owner.Equals(address);
        }
        
        /// <summary>
        /// Checks if the specified address is the administrator of this domain.
        /// </summary>
        /// <param name="address">The address to check</param>
        /// <returns>True if the address is the administrator</returns>
        public bool IsAdministeredBy(Hash160 address)
        {
            return Admin != null && Admin.Equals(address);
        }
        
        /// <summary>
        /// Validates that this name state is properly formed.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new InvalidOperationException("Name cannot be null or empty");
            
            if (IsRegistered)
            {
                if (Owner == null || Owner.Equals(Hash160.ZERO))
                    throw new InvalidOperationException("Registered domain must have a valid owner");
                
                if (Expiration <= 0)
                    throw new InvalidOperationException("Registered domain must have a valid expiration");
                
                if (RegisteredAt <= 0)
                    throw new InvalidOperationException("Registered domain must have a valid registration block");
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Gets the domain age in blocks.
        /// </summary>
        /// <param name="currentBlockHeight">The current block height</param>
        /// <returns>Domain age in blocks</returns>
        public long GetAgeInBlocks(long currentBlockHeight)
        {
            if (!IsRegistered)
                return 0;
            
            return Math.Max(0, currentBlockHeight - RegisteredAt);
        }
        
        /// <summary>
        /// Gets the time since last renewal in blocks.
        /// </summary>
        /// <param name="currentBlockHeight">The current block height</param>
        /// <returns>Blocks since last renewal</returns>
        public long GetBlocksSinceRenewal(long currentBlockHeight)
        {
            if (!IsRegistered || RenewedAt <= 0)
                return GetAgeInBlocks(currentBlockHeight);
            
            return Math.Max(0, currentBlockHeight - RenewedAt);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a detailed string representation of this name state.
        /// </summary>
        /// <returns>Detailed string representation</returns>
        public string ToDetailedString()
        {
            var result = $"NameState for '{Name}':\n";
            result += $"  Registered: {IsRegistered}\n";
            
            if (IsRegistered)
            {
                result += $"  Owner: {Owner}\n";
                result += $"  Admin: {(HasAdmin ? Admin.ToString() : "None")}\n";
                result += $"  Expiration: Block {Expiration}\n";
                result += $"  Registered At: Block {RegisteredAt}\n";
                result += $"  Renewed At: Block {(RenewedAt > 0 ? RenewedAt.ToString() : "Never")}\n";
                
                if (!string.IsNullOrEmpty(Metadata))
                {
                    result += $"  Metadata: {Metadata}\n";
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Returns a string representation of this name state.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var status = IsRegistered ? (IsExpired ? "Expired" : "Active") : "Unregistered";
            var ownerInfo = HasOwner ? $" (Owner: {Owner})" : "";
            
            return $"NameState(Name: {Name}, Status: {status}{ownerInfo})";
        }
        
        #endregion
    }
}