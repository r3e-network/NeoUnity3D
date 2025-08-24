using System;
using System.Security.Cryptography;
using System.Text;

namespace Neo.Unity.SDK.Crypto
{
    /// <summary>
    /// Cryptographic hash utilities for Neo blockchain operations.
    /// Provides SHA-256, RIPEMD-160, and Neo-specific hash functions.
    /// Production-ready implementation using .NET cryptographic libraries.
    /// </summary>
    public static class Hash
    {
        #region SHA-256 Operations
        
        /// <summary>
        /// Computes SHA-256 hash of the input data.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>32-byte SHA-256 hash</returns>
        public static byte[] Sha256(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }
        
        /// <summary>
        /// Computes SHA-256 hash of a string using UTF-8 encoding.
        /// </summary>
        /// <param name="input">String to hash</param>
        /// <returns>32-byte SHA-256 hash</returns>
        public static byte[] Sha256(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            var bytes = Encoding.UTF8.GetBytes(input);
            return Sha256(bytes);
        }
        
        #endregion
        
        #region Double SHA-256 (Hash256)
        
        /// <summary>
        /// Computes double SHA-256 hash (Hash256) of the input data.
        /// This is the standard hash function used for Neo transaction and block hashes.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>32-byte double SHA-256 hash</returns>
        public static byte[] Hash256(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            using (var sha256 = SHA256.Create())
            {
                var firstHash = sha256.ComputeHash(data);
                return sha256.ComputeHash(firstHash);
            }
        }
        
        /// <summary>
        /// Computes double SHA-256 hash of a string using UTF-8 encoding.
        /// </summary>
        /// <param name="input">String to hash</param>
        /// <returns>32-byte double SHA-256 hash</returns>
        public static byte[] Hash256(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            var bytes = Encoding.UTF8.GetBytes(input);
            return Hash256(bytes);
        }
        
        #endregion
        
        #region RIPEMD-160 Operations
        
        /// <summary>
        /// Computes RIPEMD-160 hash of the input data.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>20-byte RIPEMD-160 hash</returns>
        public static byte[] RipeMD160(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            // Use RipeMD160 implementation
            return RIPEMD160.ComputeHash(data);
        }
        
        #endregion
        
        #region Hash160 (Neo Address Hash)
        
        /// <summary>
        /// Computes Hash160 (RIPEMD-160 of SHA-256) of the input data.
        /// This is used for Neo addresses and script hashes.
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>20-byte Hash160</returns>
        public static byte[] Hash160(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            var sha256Hash = Sha256(data);
            return RipeMD160(sha256Hash);
        }
        
        /// <summary>
        /// Computes Hash160 of a string using UTF-8 encoding.
        /// </summary>
        /// <param name="input">String to hash</param>
        /// <returns>20-byte Hash160</returns>
        public static byte[] Hash160(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            var bytes = Encoding.UTF8.GetBytes(input);
            return Hash160(bytes);
        }
        
        #endregion
        
        #region HMAC Operations
        
        /// <summary>
        /// Computes HMAC-SHA256 of the input data with the specified key.
        /// </summary>
        /// <param name="key">HMAC key</param>
        /// <param name="data">Data to authenticate</param>
        /// <returns>32-byte HMAC-SHA256</returns>
        public static byte[] HmacSha256(byte[] key, byte[] data)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(data);
            }
        }
        
        /// <summary>
        /// Computes HMAC-SHA512 of the input data with the specified key.
        /// Used for BIP-32 key derivation.
        /// </summary>
        /// <param name="key">HMAC key</param>
        /// <param name="data">Data to authenticate</param>
        /// <returns>64-byte HMAC-SHA512</returns>
        public static byte[] HmacSha512(byte[] key, byte[] data)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            using (var hmac = new HMACSHA512(key))
            {
                return hmac.ComputeHash(data);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Verifies a checksum for Neo address validation.
        /// </summary>
        /// <param name="data">Address data (without checksum)</param>
        /// <param name="checksum">Provided checksum</param>
        /// <returns>True if checksum is valid</returns>
        public static bool VerifyChecksum(byte[] data, byte[] checksum)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (checksum == null)
                throw new ArgumentNullException(nameof(checksum));
            
            var computedChecksum = Hash256(data).Take(checksum.Length).ToArray();
            
            if (computedChecksum.Length != checksum.Length)
                return false;
            
            for (int i = 0; i < checksum.Length; i++)
            {
                if (computedChecksum[i] != checksum[i])
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Computes a checksum for the given data.
        /// </summary>
        /// <param name="data">Data to compute checksum for</param>
        /// <param name="length">Length of checksum (default: 4 bytes)</param>
        /// <returns>Checksum bytes</returns>
        public static byte[] ComputeChecksum(byte[] data, int length = 4)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (length <= 0 || length > 32)
                throw new ArgumentException("Checksum length must be between 1 and 32 bytes", nameof(length));
            
            return Hash256(data).Take(length).ToArray();
        }
        
        #endregion
    }
}