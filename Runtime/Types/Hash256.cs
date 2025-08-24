using System;
using System.Linq;
using UnityEngine;
using Neo.Unity.SDK.Serialization;
using Neo.Unity.SDK.Utils;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents a 256-bit hash used for Neo transaction hashes and block hashes.
    /// Immutable value type with proper validation and conversion utilities.
    /// </summary>
    [System.Serializable]
    [JsonConverter(typeof(Hash256JsonConverter))]
    public class Hash256 : IEquatable<Hash256>, INeoSerializable, IComparable<Hash256>
    {
        #region Constants
        
        /// <summary>The size of a Hash256 in bytes</summary>
        public const int SIZE = 32;
        
        /// <summary>Zero hash (all zeros)</summary>
        public static readonly Hash256 ZERO = new Hash256(new byte[SIZE]);
        
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
        /// Creates a Hash256 from a byte array.
        /// </summary>
        /// <param name="bytes">32-byte array in big-endian format</param>
        /// <exception cref="ArgumentException">If bytes is not exactly 32 bytes</exception>
        public Hash256(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            if (bytes.Length != SIZE)
                throw new ArgumentException($"Hash256 must be exactly {SIZE} bytes, got {bytes.Length}", nameof(bytes));
            
            value = (byte[])bytes.Clone();
        }
        
        /// <summary>
        /// Creates a Hash256 from a hex string.
        /// </summary>
        /// <param name="hexString">64-character hex string</param>
        /// <exception cref="ArgumentException">If hex string is invalid</exception>
        public Hash256(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                throw new ArgumentException("Hex string cannot be null or empty", nameof(hexString));
            
            var cleaned = hexString.CleanHexPrefix();
            
            if (cleaned.Length != SIZE * 2)
                throw new ArgumentException($"Hash256 hex string must be {SIZE * 2} characters, got {cleaned.Length}", nameof(hexString));
            
            if (!cleaned.IsValidHex())
                throw new ArgumentException("Invalid hex string format", nameof(hexString));
            
            value = Convert.FromHexString(cleaned);
        }
        
        /// <summary>
        /// Private constructor for internal use.
        /// </summary>
        private Hash256()
        {
            value = new byte[SIZE];
        }
        
        #endregion
        
        #region Static Factory Methods
        
        /// <summary>
        /// Creates a Hash256 by computing SHA-256 hash of the input data.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>Hash256 of the data</returns>
        public static Hash256 ComputeHash(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            var hash = data.Sha256();
            return new Hash256(hash);
        }
        
        /// <summary>
        /// Creates a Hash256 by computing double SHA-256 hash (Hash256) of the input data.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>Hash256 (double SHA-256) of the data</returns>
        public static Hash256 ComputeDoubleHash(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            var hash = data.Hash256();
            return new Hash256(hash);
        }
        
        /// <summary>
        /// Parses a Hash256 from various string formats.
        /// </summary>
        /// <param name="input">String input (hex with or without 0x prefix)</param>
        /// <returns>Parsed Hash256 or null if invalid</returns>
        public static Hash256 TryParse(string input)
        {
            try
            {
                return new Hash256(input);
            }
            catch
            {
                return null;
            }
        }
        
        #endregion
        
        #region Conversion Methods
        
        /// <summary>
        /// Converts to byte array (creates a copy).
        /// </summary>
        /// <returns>32-byte array copy</returns>
        public byte[] ToArray()
        {
            return (byte[])value.Clone();
        }
        
        /// <summary>
        /// Converts to hex string without 0x prefix.
        /// </summary>
        /// <returns>64-character hex string</returns>
        public override string ToString()
        {
            return Convert.ToHexString(value).ToLowerInvariant();
        }
        
        /// <summary>
        /// Converts to hex string with 0x prefix.
        /// </summary>
        /// <returns>66-character hex string with prefix</returns>
        public string ToHexString()
        {
            return "0x" + ToString();
        }
        
        /// <summary>
        /// Converts to reverse byte order (for little-endian usage).
        /// </summary>
        /// <returns>Hash256 with reversed byte order</returns>
        public Hash256 Reverse()
        {
            return new Hash256(value.SafeReverse());
        }
        
        #endregion
        
        #region Equality and Comparison
        
        /// <summary>
        /// Determines whether the specified Hash256 is equal to the current Hash256.
        /// </summary>
        /// <param name="other">The Hash256 to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(Hash256 other)
        {
            if (other == null)
                return false;
            
            return value.SequenceEqual(other.value);
        }
        
        /// <summary>
        /// Determines whether the specified object is equal to the current Hash256.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Hash256);
        }
        
        /// <summary>
        /// Returns a hash code for the current Hash256.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            var hash = 0;
            for (int i = 0; i < value.Length; i += 4)
            {
                var remaining = Math.Min(4, value.Length - i);
                if (remaining == 4)
                {
                    var chunk = BitConverter.ToInt32(value, i);
                    hash ^= chunk;
                }
                else
                {
                    // Handle remaining bytes
                    for (int j = 0; j < remaining; j++)
                    {
                        hash ^= value[i + j] << (j * 8);
                    }
                }
            }
            return hash;
        }
        
        /// <summary>
        /// Compares this Hash256 with another for ordering.
        /// </summary>
        /// <param name="other">The Hash256 to compare</param>
        /// <returns>Comparison result</returns>
        public int CompareTo(Hash256 other)
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
        public static bool operator ==(Hash256 left, Hash256 right)
        {
            if (ReferenceEquals(left, right))
                return true;
            
            if (left is null || right is null)
                return false;
            
            return left.Equals(right);
        }
        
        /// <summary>Inequality operator</summary>
        public static bool operator !=(Hash256 left, Hash256 right)
        {
            return !(left == right);
        }
        
        /// <summary>Less than operator</summary>
        public static bool operator <(Hash256 left, Hash256 right)
        {
            return left?.CompareTo(right) < 0;
        }
        
        /// <summary>Greater than operator</summary>
        public static bool operator >(Hash256 left, Hash256 right)
        {
            return left?.CompareTo(right) > 0;
        }
        
        /// <summary>Implicit conversion from string</summary>
        public static implicit operator Hash256(string hexString)
        {
            return new Hash256(hexString);
        }
        
        /// <summary>Implicit conversion to string</summary>
        public static implicit operator string(Hash256 hash)
        {
            return hash?.ToString();
        }
        
        #endregion
        
        #region INeoSerializable Implementation
        
        /// <summary>
        /// Serializes this Hash256 to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer</param>
        public void Serialize(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            
            writer.Write(value);
        }
        
        /// <summary>
        /// Deserializes a Hash256 from a binary reader.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        /// <returns>Deserialized Hash256</returns>
        public static Hash256 Deserialize(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            var bytes = reader.ReadBytes(SIZE);
            return new Hash256(bytes);
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates that this hash is not zero.
        /// </summary>
        /// <exception cref="InvalidOperationException">If hash is zero</exception>
        public void ValidateNotZero()
        {
            if (Equals(ZERO))
                throw new InvalidOperationException("Hash256 cannot be zero for this operation");
        }
        
        /// <summary>
        /// Checks if this hash is zero.
        /// </summary>
        /// <returns>True if all bytes are zero</returns>
        public bool IsZero()
        {
            return value.All(b => b == 0);
        }
        
        #endregion
    }
    
    /// <summary>
    /// JSON converter for Hash256 serialization.
    /// </summary>
    public class Hash256JsonConverter : JsonConverter<Hash256>
    {
        /// <summary>
        /// Reads Hash256 from JSON.
        /// </summary>
        public override Hash256 ReadJson(JsonReader reader, Type objectType, Hash256 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var value = (string)reader.Value;
                return string.IsNullOrEmpty(value) ? null : new Hash256(value);
            }
            
            if (reader.TokenType == JsonToken.Null)
                return null;
            
            throw new JsonException($"Unexpected token type {reader.TokenType} when parsing Hash256");
        }
        
        /// <summary>
        /// Writes Hash256 to JSON.
        /// </summary>
        public override void WriteJson(JsonWriter writer, Hash256 value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue("0x" + value.ToString());
        }
    }
}