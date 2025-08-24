using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neo.Unity.SDK.Serialization
{
    /// <summary>
    /// Interface for objects that can be serialized to and from Neo protocol binary format.
    /// Provides standardized serialization for blockchain data structures.
    /// </summary>
    public interface INeoSerializable
    {
        /// <summary>
        /// Gets the size in bytes when serialized.
        /// </summary>
        int Size { get; }
        
        /// <summary>
        /// Serializes this object to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer to write to</param>
        void Serialize(BinaryWriter writer);
        
        /// <summary>
        /// Converts this object to a byte array.
        /// </summary>
        /// <returns>The serialized byte array</returns>
        byte[] ToArray();
    }
    
    /// <summary>
    /// Static extension methods for INeoSerializable operations.
    /// </summary>
    public static class NeoSerializableExtensions
    {
        /// <summary>
        /// Deserializes an object from a binary reader.
        /// </summary>
        /// <typeparam name="T">The type implementing INeoSerializable</typeparam>
        /// <param name="reader">The binary reader to read from</param>
        /// <returns>The deserialized object</returns>
        public static T Deserialize<T>(BinaryReader reader) where T : INeoSerializable, new()
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            // Generic deserialization is not supported - each type must implement its own deserialization
            // This ensures type safety and proper handling of complex serialization formats
            throw new NotSupportedException($"Type {typeof(T).Name} must implement its own static Deserialize method. Generic deserialization is not supported for Neo blockchain data structures.");
        }
        
        /// <summary>
        /// Creates an object from a byte array.
        /// </summary>
        /// <typeparam name="T">The type implementing INeoSerializable</typeparam>
        /// <param name="bytes">The byte array to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public static T FromBytes<T>(byte[] bytes) where T : INeoSerializable, new()
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            using var reader = new BinaryReader(bytes);
            return Deserialize<T>(reader);
        }
        
        /// <summary>
        /// Gets the variable-length size for a list of serializable objects.
        /// Includes the count prefix and all object sizes.
        /// </summary>
        /// <param name="list">The list of serializable objects</param>
        /// <returns>The total variable-length size</returns>
        public static int GetVarSize<T>(this List<T> list) where T : INeoSerializable
        {
            if (list == null)
                return 1; // Empty list VarInt size
            
            var countSize = GetVarIntSize(list.Count);
            var contentSize = 0;
            
            foreach (var item in list)
            {
                contentSize += item?.Size ?? 0;
            }
            
            return countSize + contentSize;
        }
        
        /// <summary>
        /// Gets the variable-length size for an integer.
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <returns>The VarInt size in bytes</returns>
        public static int GetVarIntSize(int value)
        {
            if (value < 0)
                return 1; // Error case, but handle gracefully
            
            if (value < 0xFD)
                return 1;
            else if (value <= 0xFFFF)
                return 3; // 0xFD + 2 bytes
            else if (value <= 0xFFFFFFFF)
                return 5; // 0xFE + 4 bytes
            else
                return 9; // 0xFF + 8 bytes
        }
        
        /// <summary>
        /// Gets the variable-length size for a byte array.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>The VarBytes size including length prefix</returns>
        public static int GetVarBytesSize(byte[] bytes)
        {
            var length = bytes?.Length ?? 0;
            return GetVarIntSize(length) + length;
        }
        
        /// <summary>
        /// Gets the variable-length size for a string (UTF-8 encoded).
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>The VarString size including length prefix</returns>
        public static int GetVarStringSize(string str)
        {
            if (string.IsNullOrEmpty(str))
                return GetVarIntSize(0);
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(str);
            return GetVarBytesSize(bytes);
        }
    }
    
    /// <summary>
    /// Base implementation helper for INeoSerializable objects.
    /// Provides common serialization functionality.
    /// </summary>
    public abstract class NeoSerializableBase : INeoSerializable
    {
        /// <summary>
        /// Gets the size in bytes when serialized.
        /// </summary>
        public abstract int Size { get; }
        
        /// <summary>
        /// Serializes this object to a binary writer.
        /// </summary>
        /// <param name="writer">The binary writer to write to</param>
        public abstract void Serialize(BinaryWriter writer);
        
        /// <summary>
        /// Converts this object to a byte array.
        /// </summary>
        /// <returns>The serialized byte array</returns>
        public virtual byte[] ToArray()
        {
            using var writer = new BinaryWriter(Size);
            Serialize(writer);
            return writer.GetBuffer();
        }
        
        /// <summary>
        /// Creates a hex string representation of this object.
        /// </summary>
        /// <returns>The hex string</returns>
        public virtual string ToHexString()
        {
            return Convert.ToHexString(ToArray());
        }
        
        /// <summary>
        /// Validates the object state before serialization.
        /// </summary>
        /// <exception cref="InvalidOperationException">If object state is invalid</exception>
        protected virtual void ValidateForSerialization()
        {
            // Override in derived classes for specific validation
        }
        
        /// <summary>
        /// Called before serialization to ensure object is in valid state.
        /// </summary>
        protected void EnsureValidForSerialization()
        {
            try
            {
                ValidateForSerialization();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Object validation failed before serialization: {ex.Message}", ex);
            }
        }
    }
}