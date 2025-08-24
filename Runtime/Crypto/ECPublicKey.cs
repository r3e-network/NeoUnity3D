using System;
using System.Numerics;
using UnityEngine;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Crypto
{
    /// <summary>
    /// Represents an elliptic curve public key for the secp256r1 (NIST P-256) curve.
    /// Immutable value type with point operations and encoding utilities.
    /// </summary>
    [System.Serializable]
    public class ECPublicKey : IEquatable<ECPublicKey>
    {
        #region Private Fields
        
        [SerializeField]
        private readonly byte[] encodedBytes;
        
        [SerializeField]
        private readonly bool isCompressed;
        
        #endregion
        
        #region Properties
        
        /// <summary>The underlying EC point</summary>
        public ECPoint Point { get; private set; }
        
        /// <summary>Whether this public key is in compressed format</summary>
        public bool IsCompressed => isCompressed;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates an ECPublicKey from encoded bytes.
        /// </summary>
        /// <param name="encodedBytes">Encoded public key bytes (33 bytes compressed or 65 bytes uncompressed)</param>
        public ECPublicKey(byte[] encodedBytes)
        {
            if (encodedBytes == null)
                throw new ArgumentNullException(nameof(encodedBytes));
            
            if (!encodedBytes.IsValidPublicKey())
                throw new ArgumentException("Invalid public key format", nameof(encodedBytes));
            
            this.encodedBytes = (byte[])encodedBytes.Clone();
            this.isCompressed = encodedBytes.Length == 33;
            this.Point = ECPoint.DecodePoint(encodedBytes);
        }
        
        /// <summary>
        /// Creates an ECPublicKey from hex string.
        /// </summary>
        /// <param name="hexString">Hex encoded public key</param>
        public ECPublicKey(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                throw new ArgumentException("Hex string cannot be null or empty", nameof(hexString));
            
            var bytes = hexString.BytesFromHex();
            
            if (!bytes.IsValidPublicKey())
                throw new ArgumentException("Invalid public key hex format", nameof(hexString));
            
            this.encodedBytes = bytes;
            this.isCompressed = bytes.Length == 33;
            this.Point = ECPoint.DecodePoint(bytes);
        }
        
        /// <summary>
        /// Creates an ECPublicKey from an ECPoint.
        /// </summary>
        /// <param name="point">The EC point</param>
        /// <param name="compressed">Whether to use compressed encoding</param>
        public ECPublicKey(ECPoint point, bool compressed = true)
        {
            Point = point ?? throw new ArgumentNullException(nameof(point));
            isCompressed = compressed;
            encodedBytes = point.GetEncoded(compressed);
        }
        
        #endregion
        
        #region Encoding Methods
        
        /// <summary>
        /// Gets the encoded public key bytes.
        /// </summary>
        /// <param name="compressed">Whether to use compressed format</param>
        /// <returns>Encoded public key bytes</returns>
        public byte[] GetEncoded(bool compressed)
        {
            if (compressed == isCompressed)
            {
                return (byte[])encodedBytes.Clone();
            }
            
            // Convert between compressed and uncompressed formats
            return Point.GetEncoded(compressed);
        }
        
        /// <summary>
        /// Gets the encoded public key as hex string.
        /// </summary>
        /// <param name="compressed">Whether to use compressed format</param>
        /// <returns>Hex encoded public key</returns>
        public string GetEncodedHex(bool compressed = true)
        {
            return GetEncoded(compressed).ToHexString();
        }
        
        #endregion
        
        #region Address Operations
        
        /// <summary>
        /// Gets the Neo address for this public key.
        /// </summary>
        /// <returns>Neo address string</returns>
        public string GetAddress()
        {
            var hash160 = Hash160.FromPublicKey(GetEncoded(true));
            return hash160.ToAddress();
        }
        
        /// <summary>
        /// Gets the script hash for this public key.
        /// </summary>
        /// <returns>Script hash (Hash160)</returns>
        public Hash160 GetScriptHash()
        {
            return Hash160.FromPublicKey(GetEncoded(true));
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates that this public key is on the secp256r1 curve.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            try
            {
                return Point.IsValid();
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Validates that this public key is not the infinity point.
        /// </summary>
        /// <returns>True if not infinity, false if infinity</returns>
        public bool IsNotInfinity()
        {
            return !Point.IsInfinity();
        }
        
        #endregion
        
        #region Equality and Hashing
        
        /// <summary>
        /// Determines whether the specified ECPublicKey is equal to the current ECPublicKey.
        /// </summary>
        /// <param name="other">The ECPublicKey to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public bool Equals(ECPublicKey other)
        {
            if (other == null)
                return false;
            
            // Compare the actual point, not just the encoding
            return Point.Equals(other.Point);
        }
        
        /// <summary>
        /// Determines whether the specified object is equal to the current ECPublicKey.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ECPublicKey);
        }
        
        /// <summary>
        /// Returns a hash code for the current ECPublicKey.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return Point.GetHashCode();
        }
        
        #endregion
        
        #region Operators
        
        /// <summary>Equality operator</summary>
        public static bool operator ==(ECPublicKey left, ECPublicKey right)
        {
            if (ReferenceEquals(left, right))
                return true;
            
            if (left is null || right is null)
                return false;
            
            return left.Equals(right);
        }
        
        /// <summary>Inequality operator</summary>
        public static bool operator !=(ECPublicKey left, ECPublicKey right)
        {
            return !(left == right);
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this ECPublicKey.
        /// </summary>
        /// <returns>Hex encoded public key</returns>
        public override string ToString()
        {
            return GetEncodedHex(true);
        }
        
        #endregion
    }
}