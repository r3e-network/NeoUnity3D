using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents a contract storage entry with key-value pair
    /// </summary>
    [Serializable]
    public struct ContractStorageEntry : IEquatable<ContractStorageEntry>
    {
        [JsonProperty("key")]
        [SerializeField] private string _key;
        
        [JsonProperty("value")]
        [SerializeField] private string _value;
        
        /// <summary>
        /// Gets the storage key as a hex string
        /// </summary>
        public string Key => _key;
        
        /// <summary>
        /// Gets the storage value as a hex string
        /// </summary>
        public string Value => _value;
        
        /// <summary>
        /// Initializes a new instance of ContractStorageEntry
        /// </summary>
        /// <param name="key">Storage key as hex string</param>
        /// <param name="value">Storage value as hex string</param>
        [JsonConstructor]
        public ContractStorageEntry(string key, string value)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }
        
        /// <summary>
        /// Gets the key as byte array
        /// </summary>
        /// <returns>Key bytes</returns>
        public byte[] GetKeyBytes()
        {
            if (string.IsNullOrEmpty(_key))
                return Array.Empty<byte>();
                
            return Convert.FromHexString(_key);
        }
        
        /// <summary>
        /// Gets the value as byte array
        /// </summary>
        /// <returns>Value bytes</returns>
        public byte[] GetValueBytes()
        {
            if (string.IsNullOrEmpty(_value))
                return Array.Empty<byte>();
                
            return Convert.FromHexString(_value);
        }
        
        public bool Equals(ContractStorageEntry other)
        {
            return _key == other._key && _value == other._value;
        }
        
        public override bool Equals(object obj)
        {
            return obj is ContractStorageEntry other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_key, _value);
        }
        
        public static bool operator ==(ContractStorageEntry left, ContractStorageEntry right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(ContractStorageEntry left, ContractStorageEntry right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"ContractStorageEntry(Key: {_key}, Value: {_value})";
        }
    }
}