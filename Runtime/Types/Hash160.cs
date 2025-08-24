using System;
using System.Linq;
using UnityEngine;
using Neo.Unity.SDK.Serialization;
using Neo.Unity.SDK.Utils;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents a 160-bit hash used for Neo script hashes and addresses.
    /// Immutable value type with proper validation and conversion utilities.
    /// </summary>
    [System.Serializable]
    [JsonConverter(typeof(Hash160JsonConverter))]
    public class Hash160 : IEquatable<Hash160>, INeoSerializable, IComparable<Hash160>
    {
        #region Constants
        
        /// <summary>The size of a Hash160 in bytes</summary>
        public const int SIZE = 20;
        
        /// <summary>Zero hash (all zeros)</summary>
        public static readonly Hash160 ZERO = new Hash160(new byte[SIZE]);
        
        #endregion
        
        #region Private Fields
        
        [SerializeField]
        private readonly byte[] value;
        
        #endregion
        
        #region Properties
        
        /// <summary>The hash bytes (read-only copy)</summary>
        public byte[] Value => (byte[])value.Clone();
        
        /// <summary>Size in bytes for serialization</summary>
        public int Size => SIZE;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a Hash160 from a byte array.
        /// </summary>
        /// <param name="bytes">20-byte array in big-endian format</param>
        /// <exception cref="ArgumentException">If bytes is not exactly 20 bytes</exception>
        public Hash160(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            if (bytes.Length != SIZE)
                throw new ArgumentException($"Hash160 must be exactly {SIZE} bytes, got {bytes.Length}", nameof(bytes));
            
            value = (byte[])bytes.Clone();
        }
        
        /// <summary>
        /// Creates a Hash160 from a hex string.
        /// </summary>
        /// <param name="hexString">40-character hex string</param>
        /// <exception cref="ArgumentException">If hex string is invalid</exception>
        public Hash160(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                throw new ArgumentException("Hex string cannot be null or empty", nameof(hexString));
            
            var cleaned = hexString.CleanHexPrefix();
            
            if (cleaned.Length != SIZE * 2)
                throw new ArgumentException($"Hash160 hex string must be {SIZE * 2} characters, got {cleaned.Length}", nameof(hexString));
            
            if (!cleaned.IsValidHex())
                throw new ArgumentException("Invalid hex string format", nameof(hexString));
            
            value = Convert.FromHexString(cleaned);
        }
        
        /// <summary>
        /// Private constructor for internal use.
        /// </summary>
        private Hash160()
        {
            value = new byte[SIZE];
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates a Hash160 from a Neo address.
        /// </summary>
        /// <param name="address">Valid Neo address</param>
        /// <returns>Hash160 representation of the address</returns>
        /// <exception cref="ArgumentException">If address is invalid</exception>
        public static Hash160 FromAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("Address cannot be null or empty", nameof(address));
            
            if (!address.IsValidAddress())
                throw new ArgumentException("Invalid Neo address format", nameof(address));
            
            var scriptHashBytes = address.AddressToScriptHash();
            return new Hash160(scriptHashBytes);
        }
        
        /// <summary>
        /// Creates a Hash160 from a script (Hash160 of the script).
        /// </summary>
        /// <param name="script">Script bytes</param>
        /// <returns>Hash160 of the script</returns>
        public static Hash160 FromScript(byte[] script)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));
            
            var hash = script.Hash160();
            return new Hash160(hash);
        }
        
        /// <summary>
        /// Creates a Hash160 from a public key.
        /// </summary>
        /// <param name="publicKey">Public key bytes</param>
        /// <returns>Hash160 of the public key script</returns>
        public static Hash160 FromPublicKey(byte[] publicKey)
        {
            if (publicKey == null)
                throw new ArgumentNullException(nameof(publicKey));
            
            if (!publicKey.IsValidPublicKey())
                throw new ArgumentException("Invalid public key format", nameof(publicKey));
            
            // Create verification script for single-sig account
            var script = new byte[publicKey.Length + 2];
            script[0] = 0x0C; // PUSHDATA1
            script[1] = (byte)publicKey.Length;
            Array.Copy(publicKey, 0, script, 2, publicKey.Length);
            
            return FromScript(script);
        }
        
        #endregion
        
        #region Conversion Methods
        
        /// <summary>
        /// Converts this Hash160 to a Neo address.
        /// </summary>
        /// <returns>Neo address string</returns>
        public string ToAddress()
        {
            return value.ScriptHashToAddress();
        }
        
        /// <summary>
        /// Converts to byte array (creates a copy).
        /// </summary>
        /// <returns>20-byte array copy</returns>
        public byte[] ToArray()
        {
            return (byte[])value.Clone();
        }
        
        /// <summary>
        /// Converts to hex string without 0x prefix.
        /// </summary>
        /// <returns>40-character hex string</returns>
        public override string ToString()
        {
            return Convert.ToHexString(value).ToLowerInvariant();
        }
        
        /// <summary>
        /// Converts to hex string with 0x prefix.
        /// </summary>
        /// <returns>42-character hex string with prefix</returns>
        public string ToHexString()
        {
            return "0x" + ToString();
        }
        
        #endregion
        
        #region Equality and Comparison
        
        /// <summary>
        /// Determines whether the specified Hash160 is equal to the current Hash160.
        /// </summary>
        /// <param name="other">The Hash160 to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(Hash160 other)
        {
            if (other == null)
                return false;
            
            return value.SequenceEqual(other.value);
        }
        
        /// <summary>
        /// Determines whether the specified object is equal to the current Hash160.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Hash160);
        }
        
        /// <summary>
        /// Returns a hash code for the current Hash160.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hash = 0;
            for (int i = 0; i < value.Length; i += 4)
            {
                var chunk = BitConverter.ToInt32(value, i);
                hash ^= chunk;
            }
            return hash;
        }
        
        /// <summary>
        /// Compares this Hash160 with another for ordering.
        /// </summary>
        /// <param name="other">The Hash160 to compare</param>
        /// <returns>Comparison result</returns>
        public int CompareTo(Hash160 other)
        {
            if (other == null)
                return 1;
            
            for (int i = 0; i < SIZE; i++)
            {
                var comparison = value[i].CompareTo(other.value[i]);
                if (comparison != 0)
                    return comparison;
            }
            
            return 0;
        }
        
        #endregion
        
        #region Operators
        
        /// <summary>Equality operator</summary>
        public static bool operator ==(Hash160 left, Hash160 right)
        {
            if (ReferenceEquals(left, right))
                return true;
            
            if (left is null || right is null)
                return false;
            
            return left.Equals(right);
        }
        
        /// <summary>Inequality operator</summary>
        public static bool operator !=(Hash160 left, Hash160 right)
        {
            return !(left == right);
        }
        
        /// <summary>Less than operator</summary>
        public static bool operator <(Hash160 left, Hash160 right)
        {
            return left?.CompareTo(right) < 0;
        }
        
        /// <summary>Greater than operator</summary>
        public static bool operator >(Hash160 left, Hash160 right)
        {
            return left?.CompareTo(right) > 0;
        }
        
        /// <summary>Implicit conversion from string</summary>
        public static implicit operator Hash160(string hexString)
        {
            return new Hash160(hexString);
        }
        
        /// <summary>Implicit conversion to string</summary>
        public static implicit operator string(Hash160 hash)
        {
            return hash?.ToString();
        }
        
        #endregion
        
        #region INeoSerializable Implementation
        
        /// <summary>
        /// Serializes this Hash160 to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer</param>
        public void Serialize(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            
            writer.Write(value);
        }
        
        /// <summary>
        /// Deserializes a Hash160 from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <returns>Deserialized Hash160</returns>
        public static Hash160 Deserialize(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            var bytes = reader.ReadBytes(SIZE);
            return new Hash160(bytes);
        }
        
        /// <summary>
        /// Converts this Hash160 to a byte array.
        /// </summary>
        /// <returns>20-byte array</returns>
        public byte[] ToArray()
        {
            return (byte[])value.Clone();
        }
        
        #endregion
    }
    
    /// <summary>
    /// JSON converter for Hash160 serialization.
    /// </summary>
    public class Hash160JsonConverter : JsonConverter<Hash160>
    {
        /// <summary>
        /// Reads Hash160 from JSON.
        /// </summary>
        public override Hash160 ReadJson(JsonReader reader, Type objectType, Hash160 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var value = (string)reader.Value;
                return string.IsNullOrEmpty(value) ? null : new Hash160(value);
            }
            
            if (reader.TokenType == JsonToken.Null)
                return null;
            
            throw new JsonException($"Unexpected token type {reader.TokenType} when parsing Hash160");
        }
        
        /// <summary>
        /// Writes Hash160 to JSON.
        /// </summary>
        public override void WriteJson(JsonWriter writer, Hash160 value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue("0x" + value.ToString());
        }
    }
}