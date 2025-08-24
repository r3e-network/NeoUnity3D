using System;
using UnityEngine;
using Neo.Unity.SDK.Crypto;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Type aliases that provide compatibility with Neo naming conventions
    /// and simplify common type usage throughout the SDK.
    /// </summary>
    public static class TypeAliases
    {
        // Note: These are provided as static readonly fields rather than type aliases
        // because C# doesn't support Swift-style typealias declarations.
        // Use these types for consistency with Neo conventions.
    }

    /// <summary>
    /// Represents a single byte value in Neo operations.
    /// Equivalent to Swift's UInt8/Byte type.
    /// </summary>
    [Serializable]
    public struct NeoByte : IEquatable<NeoByte>, IComparable<NeoByte>
    {
        [SerializeField]
        private byte _value;

        /// <summary>
        /// The underlying byte value.
        /// </summary>
        public byte Value => _value;

        /// <summary>
        /// Initializes a new instance of the NeoByte struct.
        /// </summary>
        /// <param name="value">The byte value.</param>
        public NeoByte(byte value)
        {
            _value = value;
        }

        /// <summary>
        /// Implicit conversion from byte to NeoByte.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <returns>A new NeoByte instance.</returns>
        public static implicit operator NeoByte(byte value) => new(value);

        /// <summary>
        /// Implicit conversion from NeoByte to byte.
        /// </summary>
        /// <param name="neoByte">The NeoByte instance.</param>
        /// <returns>The underlying byte value.</returns>
        public static implicit operator byte(NeoByte neoByte) => neoByte._value;

        /// <summary>
        /// Converts the NeoByte to its hexadecimal string representation.
        /// </summary>
        /// <returns>A hexadecimal string representation.</returns>
        public override string ToString() => $"0x{_value:X2}";

        public bool Equals(NeoByte other) => _value == other._value;
        public override bool Equals(object obj) => obj is NeoByte other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(NeoByte other) => _value.CompareTo(other._value);

        public static bool operator ==(NeoByte left, NeoByte right) => left.Equals(right);
        public static bool operator !=(NeoByte left, NeoByte right) => !left.Equals(right);
        public static bool operator <(NeoByte left, NeoByte right) => left.CompareTo(right) < 0;
        public static bool operator >(NeoByte left, NeoByte right) => left.CompareTo(right) > 0;
        public static bool operator <=(NeoByte left, NeoByte right) => left.CompareTo(right) <= 0;
        public static bool operator >=(NeoByte left, NeoByte right) => left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Represents a collection of bytes in Neo operations.
    /// Equivalent to Swift's [UInt8]/Bytes type.
    /// </summary>
    [Serializable]
    public class NeoBytes : IEquatable<NeoBytes>
    {
        [SerializeField]
        private byte[] _bytes;

        /// <summary>
        /// The underlying byte array.
        /// </summary>
        public byte[] Value => _bytes ?? Array.Empty<byte>();

        /// <summary>
        /// The length of the byte array.
        /// </summary>
        public int Length => _bytes?.Length ?? 0;

        /// <summary>
        /// Gets or sets the byte at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns>The byte at the specified index.</returns>
        public byte this[int index]
        {
            get => _bytes[index];
            set => _bytes[index] = value;
        }

        /// <summary>
        /// Initializes a new instance of the NeoBytes class.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        public NeoBytes(byte[] bytes = null)
        {
            _bytes = bytes?.ToArray() ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Initializes a new instance of the NeoBytes class from a hex string.
        /// </summary>
        /// <param name="hexString">The hexadecimal string (with or without 0x prefix).</param>
        /// <returns>A new NeoBytes instance.</returns>
        public static NeoBytes FromHex(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return new NeoBytes();

            // Remove 0x prefix if present
            if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hexString = hexString.Substring(2);

            // Ensure even number of characters
            if (hexString.Length % 2 != 0)
                hexString = "0" + hexString;

            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return new NeoBytes(bytes);
        }

        /// <summary>
        /// Implicit conversion from byte array to NeoBytes.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>A new NeoBytes instance.</returns>
        public static implicit operator NeoBytes(byte[] bytes) => new(bytes);

        /// <summary>
        /// Implicit conversion from NeoBytes to byte array.
        /// </summary>
        /// <param name="neoBytes">The NeoBytes instance.</param>
        /// <returns>The underlying byte array.</returns>
        public static implicit operator byte[](NeoBytes neoBytes) => neoBytes.Value;

        /// <summary>
        /// Converts the NeoBytes to its hexadecimal string representation.
        /// </summary>
        /// <param name="prefix">Whether to include the 0x prefix.</param>
        /// <returns>A hexadecimal string representation.</returns>
        public string ToHex(bool prefix = true)
        {
            if (_bytes == null || _bytes.Length == 0)
                return prefix ? "0x" : "";

            var hex = BitConverter.ToString(_bytes).Replace("-", "").ToLowerInvariant();
            return prefix ? "0x" + hex : hex;
        }

        /// <summary>
        /// Converts the NeoBytes to its hexadecimal string representation with 0x prefix.
        /// </summary>
        /// <returns>A hexadecimal string representation.</returns>
        public override string ToString() => ToHex();

        /// <summary>
        /// Creates a copy of the NeoBytes.
        /// </summary>
        /// <returns>A new NeoBytes instance with the same byte values.</returns>
        public NeoBytes Copy() => new(_bytes);

        /// <summary>
        /// Concatenates this NeoBytes with another.
        /// </summary>
        /// <param name="other">The other NeoBytes to concatenate.</param>
        /// <returns>A new NeoBytes containing the concatenated bytes.</returns>
        public NeoBytes Concat(NeoBytes other)
        {
            if (other == null || other.Length == 0)
                return Copy();

            var result = new byte[Length + other.Length];
            Array.Copy(_bytes, 0, result, 0, Length);
            Array.Copy(other._bytes, 0, result, Length, other.Length);
            return new NeoBytes(result);
        }

        /// <summary>
        /// Gets a slice of the NeoBytes.
        /// </summary>
        /// <param name="start">The starting index.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>A new NeoBytes containing the sliced bytes.</returns>
        public NeoBytes Slice(int start, int length)
        {
            if (start < 0 || start >= Length || length <= 0)
                return new NeoBytes();

            length = Math.Min(length, Length - start);
            var result = new byte[length];
            Array.Copy(_bytes, start, result, 0, length);
            return new NeoBytes(result);
        }

        public bool Equals(NeoBytes other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            
            if (Length != other.Length) return false;
            
            for (int i = 0; i < Length; i++)
            {
                if (_bytes[i] != other._bytes[i])
                    return false;
            }
            
            return true;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is NeoBytes other && Equals(other));
        }

        public override int GetHashCode()
        {
            if (_bytes == null) return 0;
            
            int hash = 17;
            foreach (var b in _bytes)
            {
                hash = hash * 31 + b;
            }
            return hash;
        }

        public static bool operator ==(NeoBytes left, NeoBytes right) => 
            ReferenceEquals(left, right) || (left?.Equals(right) == true);
        
        public static bool operator !=(NeoBytes left, NeoBytes right) => !(left == right);
    }
}