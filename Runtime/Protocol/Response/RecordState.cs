using System;
using Newtonsoft.Json;
using UnityEngine;
using NeoUnity.Types;

namespace NeoUnity.Protocol.Response
{
    /// <summary>
    /// Represents a DNS record state with name, type, and data
    /// </summary>
    [Serializable]
    public struct RecordState : IEquatable<RecordState>
    {
        [JsonProperty("name")]
        [SerializeField] private string _name;
        
        [JsonProperty("type")]
        [SerializeField] private RecordType _recordType;
        
        [JsonProperty("data")]
        [SerializeField] private string _data;
        
        /// <summary>
        /// Gets the record name/domain
        /// </summary>
        public string Name => _name;
        
        /// <summary>
        /// Gets the DNS record type
        /// </summary>
        public RecordType RecordType => _recordType;
        
        /// <summary>
        /// Gets the record data/value
        /// </summary>
        public string Data => _data;
        
        /// <summary>
        /// Initializes a new instance of RecordState
        /// </summary>
        /// <param name="name">Record name/domain</param>
        /// <param name="recordType">DNS record type</param>
        /// <param name="data">Record data/value</param>
        [JsonConstructor]
        public RecordState(string name, RecordType recordType, string data)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _recordType = recordType;
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }
        
        /// <summary>
        /// Creates a RecordState from a Neo stack item array
        /// </summary>
        /// <param name="stackItemArray">Stack item containing [name, type, data]</param>
        /// <returns>RecordState instance</returns>
        /// <exception cref="ArgumentException">If stack item format is invalid</exception>
        public static RecordState FromStackItemArray(object[] stackItemArray)
        {
            if (stackItemArray == null || stackItemArray.Length != 3)
                throw new ArgumentException("Stack item array must contain exactly 3 elements: [name, type, data]");
                
            if (stackItemArray[0] is not string name)
                throw new ArgumentException("First element must be a string (name)");
                
            if (stackItemArray[1] is not int typeValue)
                throw new ArgumentException("Second element must be an integer (record type)");
                
            if (stackItemArray[2] is not string data)
                throw new ArgumentException("Third element must be a string (data)");
                
            var recordType = RecordType.FromByte((byte)typeValue);
            return new RecordState(name, recordType, data);
        }
        
        /// <summary>
        /// Gets whether this is an address record (A or AAAA)
        /// </summary>
        public bool IsAddressRecord => _recordType == RecordType.A || _recordType == RecordType.AAAA;
        
        /// <summary>
        /// Gets whether this is a text record
        /// </summary>
        public bool IsTextRecord => _recordType == RecordType.TXT;
        
        /// <summary>
        /// Gets whether this is a canonical name record
        /// </summary>
        public bool IsCanonicalNameRecord => _recordType == RecordType.CNAME;
        
        /// <summary>
        /// Validates if the record data format is appropriate for the record type
        /// </summary>
        /// <returns>True if data format is valid for the record type</returns>
        public bool IsValidDataFormat()
        {
            if (string.IsNullOrEmpty(_data))
                return false;
                
            return _recordType switch
            {
                RecordType.A => IsValidIPv4Address(_data),
                RecordType.AAAA => IsValidIPv6Address(_data),
                RecordType.CNAME => IsValidDomainName(_data),
                RecordType.TXT => true, // TXT records can contain any text
                _ => false
            };
        }
        
        private static bool IsValidIPv4Address(string address)
        {
            return System.Net.IPAddress.TryParse(address, out var ip) && 
                   ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }
        
        private static bool IsValidIPv6Address(string address)
        {
            return System.Net.IPAddress.TryParse(address, out var ip) && 
                   ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
        }
        
        private static bool IsValidDomainName(string domain)
        {
            if (string.IsNullOrEmpty(domain) || domain.Length > 253)
                return false;
                
            // Basic domain name validation
            var parts = domain.Split('.');
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part) || part.Length > 63)
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Gets a user-friendly description of this record
        /// </summary>
        /// <returns>Formatted record description</returns>
        public string GetDescription()
        {
            var typeStr = _recordType.ToString();
            return $"{typeStr} record for '{_name}' pointing to '{_data}'";
        }
        
        public bool Equals(RecordState other)
        {
            return _name == other._name && 
                   _recordType == other._recordType && 
                   _data == other._data;
        }
        
        public override bool Equals(object obj)
        {
            return obj is RecordState other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_name, _recordType, _data);
        }
        
        public static bool operator ==(RecordState left, RecordState right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(RecordState left, RecordState right)
        {
            return !left.Equals(right);
        }
        
        public override string ToString()
        {
            return $"RecordState({_name} {_recordType} {_data})";
        }
    }
}