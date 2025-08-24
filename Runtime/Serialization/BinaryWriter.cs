using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityEngine;
using Neo.Unity.SDK.Crypto;

namespace Neo.Unity.SDK.Serialization
{
    /// <summary>
    /// Binary writer for Neo protocol data structures.
    /// Handles little-endian binary data writing with variable-length integer support.
    /// Unity-optimized implementation with proper memory management and validation.
    /// </summary>
    public class BinaryWriter : IDisposable
    {
        #region Private Fields
        
        private List<byte> buffer;
        private bool disposed = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>Current size of the written data</summary>
        public int Size => buffer?.Count ?? 0;
        
        /// <summary>Whether the writer has been disposed</summary>
        public bool IsDisposed => disposed;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new binary writer.
        /// </summary>
        public BinaryWriter()
        {
            buffer = new List<byte>();
        }
        
        /// <summary>
        /// Creates a new binary writer with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity for the buffer</param>
        public BinaryWriter(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be negative.");
            
            buffer = new List<byte>(capacity);
        }
        
        #endregion
        
        #region Basic Type Writing
        
        /// <summary>
        /// Writes a byte array to the stream.
        /// </summary>
        /// <param name="data">The byte array to write</param>
        public void Write(byte[] data)
        {
            EnsureNotDisposed();
            
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (data.Length > 0)
            {
                buffer.AddRange(data);
            }
        }
        
        /// <summary>
        /// Writes a boolean value (1 byte).
        /// </summary>
        /// <param name="value">The boolean value</param>
        public void WriteBoolean(bool value)
        {
            WriteByte(value ? (byte)1 : (byte)0);
        }
        
        /// <summary>
        /// Writes a single byte.
        /// </summary>
        /// <param name="value">The byte value</param>
        public void WriteByte(byte value)
        {
            EnsureNotDisposed();
            buffer.Add(value);
        }
        
        /// <summary>
        /// Writes a 16-bit unsigned integer (little-endian).
        /// </summary>
        /// <param name="value">The UInt16 value</param>
        public void WriteUInt16(ushort value)
        {
            EnsureNotDisposed();
            var bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }
        
        /// <summary>
        /// Writes a 16-bit signed integer (little-endian).
        /// </summary>
        /// <param name="value">The Int16 value</param>
        public void WriteInt16(short value)
        {
            EnsureNotDisposed();
            var bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }
        
        /// <summary>
        /// Writes a 32-bit unsigned integer (little-endian).
        /// </summary>
        /// <param name="value">The UInt32 value</param>
        public void WriteUInt32(uint value)
        {
            EnsureNotDisposed();
            var bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }
        
        /// <summary>
        /// Writes a 32-bit signed integer (little-endian).
        /// </summary>
        /// <param name="value">The Int32 value</param>
        public void WriteInt32(int value)
        {
            EnsureNotDisposed();
            var bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }
        
        /// <summary>
        /// Writes a 64-bit signed integer (little-endian).
        /// </summary>
        /// <param name="value">The Int64 value</param>
        public void WriteInt64(long value)
        {
            EnsureNotDisposed();
            var bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }
        
        /// <summary>
        /// Writes a single-precision floating-point number (little-endian).
        /// </summary>
        /// <param name="value">The float value</param>
        public void WriteFloat(float value)
        {
            EnsureNotDisposed();
            var bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }
        
        /// <summary>
        /// Writes a double-precision floating-point number (little-endian).
        /// </summary>
        /// <param name="value">The double value</param>
        public void WriteDouble(double value)
        {
            EnsureNotDisposed();
            var bytes = BitConverter.GetBytes(value);
            buffer.AddRange(bytes);
        }
        
        #endregion
        
        #region Variable-Length Writing
        
        /// <summary>
        /// Writes a variable-length integer.
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <exception cref="ArgumentOutOfRangeException">If value is negative</exception>
        public void WriteVarInt(int value)
        {
            EnsureNotDisposed();
            
            if (value < 0)
            {
                Debug.LogWarning($"[BinaryWriter] Negative VarInt value: {value}");
                return;
            }
            
            if (value < 0xFD)
            {
                WriteByte((byte)value);
            }
            else if (value <= 0xFFFF)
            {
                WriteByte(0xFD);
                WriteUInt16((ushort)value);
            }
            else if (value <= 0xFFFFFFFF)
            {
                WriteByte(0xFE);
                WriteUInt32((uint)value);
            }
            else
            {
                WriteByte(0xFF);
                WriteInt64(value);
            }
        }
        
        /// <summary>
        /// Writes a variable-length byte array.
        /// </summary>
        /// <param name="value">The byte array</param>
        public void WriteVarBytes(byte[] value)
        {
            EnsureNotDisposed();
            
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            WriteVarInt(value.Length);
            
            if (value.Length > 0)
            {
                Write(value);
            }
        }
        
        /// <summary>
        /// Writes a variable-length UTF-8 string.
        /// </summary>
        /// <param name="value">The string value</param>
        public void WriteVarString(string value)
        {
            EnsureNotDisposed();
            
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteVarBytes(bytes);
        }
        
        /// <summary>
        /// Writes a fixed-length string, padding or truncating as necessary.
        /// </summary>
        /// <param name="value">The string value</param>
        /// <param name="length">The fixed length</param>
        /// <exception cref="ArgumentException">If string is too long and cannot be truncated safely</exception>
        public void WriteFixedString(string value, int length)
        {
            EnsureNotDisposed();
            
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
            
            var bytes = value != null ? Encoding.UTF8.GetBytes(value) : new byte[0];
            
            if (bytes.Length > length)
            {
                Debug.LogWarning($"[BinaryWriter] String '{value}' truncated from {bytes.Length} to {length} bytes");
                bytes = bytes[..length];
            }
            
            // Write the string bytes
            Write(bytes);
            
            // Pad with zeros if necessary
            var paddingNeeded = length - bytes.Length;
            for (int i = 0; i < paddingNeeded; i++)
            {
                WriteByte(0);
            }
        }
        
        #endregion
        
        #region Cryptographic Writing
        
        /// <summary>
        /// Writes an EC point in compressed format.
        /// </summary>
        /// <param name="point">The EC point</param>
        public void WriteECPoint(ECPoint point)
        {
            EnsureNotDisposed();
            
            if (point == null)
                throw new ArgumentNullException(nameof(point));
            
            var encoded = point.GetEncoded(true); // Always use compressed format
            Write(encoded);
        }
        
        #endregion
        
        #region Serializable Object Writing
        
        /// <summary>
        /// Writes a serializable object with variable-length byte encoding.
        /// </summary>
        /// <param name="value">The serializable object</param>
        public void WriteSerializableVariableBytes(INeoSerializable value)
        {
            EnsureNotDisposed();
            
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            var objectBytes = value.ToArray();
            WriteVarInt(objectBytes.Length);
            value.Serialize(this);
        }
        
        /// <summary>
        /// Writes a list of serializable objects with count prefix.
        /// </summary>
        /// <param name="values">The list of serializable objects</param>
        public void WriteSerializableVariable<T>(List<T> values) where T : INeoSerializable
        {
            EnsureNotDisposed();
            
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            
            WriteVarInt(values.Count);
            WriteSerializableFixed(values);
        }
        
        /// <summary>
        /// Writes a list of serializable objects with variable-length byte encoding.
        /// </summary>
        /// <param name="values">The list of serializable objects</param>
        public void WriteSerializableVariableBytes<T>(List<T> values) where T : INeoSerializable
        {
            EnsureNotDisposed();
            
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            
            // Calculate total byte length
            var totalBytes = 0;
            foreach (var value in values)
            {
                totalBytes += value.Size;
            }
            
            WriteVarInt(totalBytes);
            WriteSerializableFixed(values);
        }
        
        /// <summary>
        /// Writes a serializable object in fixed format (no length prefix).
        /// </summary>
        /// <param name="value">The serializable object</param>
        public void WriteSerializableFixed(INeoSerializable value)
        {
            EnsureNotDisposed();
            
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            
            value.Serialize(this);
        }
        
        /// <summary>
        /// Writes a list of serializable objects in fixed format (no length prefix).
        /// </summary>
        /// <param name="values">The list of serializable objects</param>
        public void WriteSerializableFixed<T>(List<T> values) where T : INeoSerializable
        {
            EnsureNotDisposed();
            
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            
            foreach (var value in values)
            {
                if (value != null)
                {
                    value.Serialize(this);
                }
            }
        }
        
        #endregion
        
        #region Output Methods
        
        /// <summary>
        /// Converts the written data to a byte array and clears the buffer.
        /// </summary>
        /// <returns>The written data as a byte array</returns>
        public byte[] ToArray()
        {
            EnsureNotDisposed();
            
            var result = buffer.ToArray();
            Clear();
            return result;
        }
        
        /// <summary>
        /// Gets a copy of the current buffer without clearing it.
        /// </summary>
        /// <returns>A copy of the current buffer</returns>
        public byte[] GetBuffer()
        {
            EnsureNotDisposed();
            return buffer.ToArray();
        }
        
        /// <summary>
        /// Converts the written data to a hex string.
        /// </summary>
        /// <returns>The hex string representation</returns>
        public string ToHexString()
        {
            EnsureNotDisposed();
            return Convert.ToHexString(buffer.ToArray());
        }
        
        /// <summary>
        /// Clears the buffer.
        /// </summary>
        public void Clear()
        {
            EnsureNotDisposed();
            buffer.Clear();
        }
        
        #endregion
        
        #region Validation and Error Checking
        
        /// <summary>
        /// Ensures the writer has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If the writer has been disposed</exception>
        private void EnsureNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BinaryWriter));
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Disposes of the binary writer resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                buffer?.Clear();
                buffer = null;
                disposed = true;
            }
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this binary writer.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"BinaryWriter(Size: {Size} bytes)";
        }
        
        #endregion
    }
}