using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents a Neo address with key and watch-only information
    /// </summary>
    [Serializable]
    public struct NeoAddress : IEquatable<NeoAddress>
    {
        [JsonProperty("address")]
        [SerializeField] private string _address;
        
        [JsonProperty("haskey")]
        [SerializeField] private bool _hasKey;
        
        [JsonProperty("label")]
        [SerializeField] private string _label;
        
        [JsonProperty("watchonly")]
        [SerializeField] private bool _watchOnly;
        
        /// <summary>
        /// Gets the Neo address string
        /// </summary>
        public string Address => _address;
        
        /// <summary>
        /// Gets whether this address has an associated private key
        /// </summary>
        public bool HasKey => _hasKey;
        
        /// <summary>
        /// Gets the optional label for this address
        /// </summary>
        public string Label => _label;
        
        /// <summary>
        /// Gets whether this is a watch-only address
        /// </summary>
        public bool WatchOnly => _watchOnly;
        
        /// <summary>
        /// Gets whether this address can sign transactions
        /// </summary>
        public bool CanSign => _hasKey && !_watchOnly;
        
        /// <summary>
        /// Initializes a new instance of NeoAddress
        /// </summary>
        /// <param name="address">Neo address string</param>
        /// <param name="hasKey">Whether address has private key</param>
        /// <param name="label">Optional label</param>
        /// <param name="watchOnly">Whether address is watch-only</param>
        [JsonConstructor]
        public NeoAddress(string address, bool hasKey, string label, bool watchOnly)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _hasKey = hasKey;
            _label = label;
            _watchOnly = watchOnly;
        }
        
        /// <summary>
        /// Creates a signing address (has key, not watch-only)
        /// </summary>
        /// <param name="address">Neo address string</param>
        /// <param name="label">Optional label</param>
        /// <returns>Signing address</returns>
        public static NeoAddress CreateSigningAddress(string address, string label = null)
        {
            return new NeoAddress(address, true, label, false);
        }
        
        /// <summary>
        /// Creates a watch-only address
        /// </summary>
        /// <param name="address">Neo address string</param>
        /// <param name="label">Optional label</param>
        /// <returns>Watch-only address</returns>
        public static NeoAddress CreateWatchOnlyAddress(string address, string label = null)
        {
            return new NeoAddress(address, false, label, true);
        }
        
        /// <summary>
        /// Validates if the address format is correct
        /// </summary>
        /// <returns>True if address format is valid</returns>
        public bool IsValidFormat()
        {
            if (string.IsNullOrEmpty(_address))
                return false;
                
            // Neo addresses start with 'N' and are 34 characters long
            return _address.Length == 34 && _address.StartsWith('N');
        }
        
        /// <summary>
        /// Gets the display name (label if available, otherwise address)
        /// </summary>
        /// <returns>Display name</returns>
        public string GetDisplayName()
        {
            return !string.IsNullOrEmpty(_label) ? _label : _address;
        }
        
        public bool Equals(NeoAddress other)
        {
            return _address == other._address && 
                   _hasKey == other._hasKey && 
                   _label == other._label && 
                   _watchOnly == other._watchOnly;
        }
        
        public override bool Equals(object obj)
        {
            return obj is NeoAddress other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_address, _hasKey, _label, _watchOnly);
        }
        
        public static bool operator ==(NeoAddress left, NeoAddress right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(NeoAddress left, NeoAddress right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            var status = CanSign ? "Signing" : (_watchOnly ? "Watch-Only" : "No Key");
            var labelPart = !string.IsNullOrEmpty(_label) ? $" ({_label})" : "";
            return $"NeoAddress({_address}{labelPart}, {status})";
        }
    }
}