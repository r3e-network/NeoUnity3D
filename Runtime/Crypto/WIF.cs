using System;
using System.Numerics;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Utils;

namespace Neo.Unity.SDK.Crypto
{
    /// <summary>
    /// Wallet Import Format (WIF) utilities for Neo private key encoding/decoding.
    /// Provides standard WIF operations following the Bitcoin/Neo WIF specification.
    /// Production-ready implementation with proper validation and error handling.
    /// </summary>
    public static class WIF
    {
        #region Constants
        
        /// <summary>WIF version byte for Neo (0x80)</summary>
        private const byte WIF_VERSION = 0x80;
        
        /// <summary>Compression flag for compressed public keys</summary>
        private const byte COMPRESSION_FLAG = 0x01;
        
        #endregion
        
        #region WIF Encoding
        
        /// <summary>
        /// Encodes a private key to WIF (Wallet Import Format).
        /// </summary>
        /// <param name="privateKey">32-byte private key</param>
        /// <param name="compressed">Whether the corresponding public key is compressed</param>
        /// <returns>WIF encoded private key</returns>
        public static string GetWIFFromPrivateKey(byte[] privateKey, bool compressed = true)
        {
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));
            
            if (privateKey.Length != 32)
                throw new ArgumentException("Private key must be exactly 32 bytes", nameof(privateKey));
            
            // Build WIF data: version + private key + compression flag (if compressed)
            var wifData = new byte[compressed ? 34 : 33];
            wifData[0] = WIF_VERSION;
            Array.Copy(privateKey, 0, wifData, 1, 32);
            
            if (compressed)
            {
                wifData[33] = COMPRESSION_FLAG;
            }
            
            // Add checksum and encode with Base58Check
            return Base58.Base58CheckEncode(wifData);
        }
        
        /// <summary>
        /// Encodes a BigInteger private key to WIF.
        /// </summary>
        /// <param name="privateKey">Private key as BigInteger</param>
        /// <param name="compressed">Whether the corresponding public key is compressed</param>
        /// <returns>WIF encoded private key</returns>
        public static string GetWIFFromPrivateKey(BigInteger privateKey, bool compressed = true)
        {
            if (privateKey <= 0)
                throw new ArgumentException("Private key must be positive", nameof(privateKey));
            
            // Convert to 32-byte array
            var privateKeyBytes = privateKey.ToByteArray();
            
            // Ensure exactly 32 bytes
            if (privateKeyBytes.Length > 32)
            {
                // Remove extra bytes from positive BigInteger
                var trimmed = new byte[32];
                Array.Copy(privateKeyBytes, 0, trimmed, 0, 32);
                privateKeyBytes = trimmed;
            }
            else if (privateKeyBytes.Length < 32)
            {
                // Pad with leading zeros
                var padded = new byte[32];
                Array.Copy(privateKeyBytes, 0, padded, 32 - privateKeyBytes.Length, privateKeyBytes.Length);
                privateKeyBytes = padded;
            }
            
            return GetWIFFromPrivateKey(privateKeyBytes, compressed);
        }
        
        #endregion
        
        #region WIF Decoding
        
        /// <summary>
        /// Decodes a WIF string to extract the private key.
        /// </summary>
        /// <param name="wif">WIF encoded private key</param>
        /// <returns>32-byte private key</returns>
        public static byte[] GetPrivateKeyFromWIF(string wif)
        {
            if (string.IsNullOrEmpty(wif))
                throw new ArgumentException("WIF cannot be null or empty", nameof(wif));
            
            try
            {
                // Decode Base58Check
                var decoded = Base58.Base58CheckDecode(wif);
                
                // Validate length (33 for uncompressed, 34 for compressed)
                if (decoded.Length != 33 && decoded.Length != 34)
                    throw new ArgumentException("Invalid WIF length", nameof(wif));
                
                // Validate version byte
                if (decoded[0] != WIF_VERSION)
                    throw new ArgumentException("Invalid WIF version byte", nameof(wif));
                
                // Validate compression flag if present
                if (decoded.Length == 34 && decoded[33] != COMPRESSION_FLAG)
                    throw new ArgumentException("Invalid WIF compression flag", nameof(wif));
                
                // Extract private key (32 bytes)
                var privateKey = new byte[32];
                Array.Copy(decoded, 1, privateKey, 0, 32);
                
                return privateKey;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new ArgumentException($"Failed to decode WIF: {ex.Message}", nameof(wif), ex);
            }
        }
        
        /// <summary>
        /// Decodes a WIF string to extract private key as BigInteger.
        /// </summary>
        /// <param name="wif">WIF encoded private key</param>
        /// <returns>Private key as BigInteger</returns>
        public static BigInteger GetPrivateKeyBigIntegerFromWIF(string wif)
        {
            var privateKeyBytes = GetPrivateKeyFromWIF(wif);
            return new BigInteger(privateKeyBytes, isUnsigned: true, isBigEndian: true);
        }
        
        /// <summary>
        /// Checks if a WIF represents a compressed public key.
        /// </summary>
        /// <param name="wif">WIF encoded private key</param>
        /// <returns>True if compressed</returns>
        public static bool IsCompressed(string wif)
        {
            if (string.IsNullOrEmpty(wif))
                return false;
            
            try
            {
                var decoded = Base58.Base58CheckDecode(wif);
                return decoded.Length == 34 && decoded[33] == COMPRESSION_FLAG;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates if a string is a valid WIF format.
        /// </summary>
        /// <param name="wif">String to validate</param>
        /// <returns>True if valid WIF</returns>
        public static bool IsValidWIF(string wif)
        {
            if (string.IsNullOrEmpty(wif))
                return false;
            
            try
            {
                GetPrivateKeyFromWIF(wif);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Validates WIF format and returns detailed validation info.
        /// </summary>
        /// <param name="wif">WIF to validate</param>
        /// <returns>Validation result with details</returns>
        public static WIFValidationResult ValidateWIF(string wif)
        {
            var result = new WIFValidationResult { WIF = wif };
            
            if (string.IsNullOrEmpty(wif))
            {
                result.IsValid = false;
                result.ErrorMessage = "WIF cannot be null or empty";
                return result;
            }
            
            try
            {
                var privateKey = GetPrivateKeyFromWIF(wif);
                var compressed = IsCompressed(wif);
                
                result.IsValid = true;
                result.IsCompressed = compressed;
                result.PrivateKeyLength = privateKey.Length;
                
                // Validate private key range
                var privateKeyBigInt = new BigInteger(privateKey, isUnsigned: true, isBigEndian: true);
                var curveOrder = BigInteger.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369");
                
                if (privateKeyBigInt >= curveOrder)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Private key value exceeds curve order";
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = ex.Message;
            }
            
            return result;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Result of WIF validation with detailed information.
    /// </summary>
    [System.Serializable]
    public class WIFValidationResult
    {
        /// <summary>The WIF string that was validated</summary>
        public string WIF { get; set; }
        
        /// <summary>Whether the WIF is valid</summary>
        public bool IsValid { get; set; }
        
        /// <summary>Whether the WIF represents a compressed key</summary>
        public bool IsCompressed { get; set; }
        
        /// <summary>Length of the extracted private key</summary>
        public int PrivateKeyLength { get; set; }
        
        /// <summary>Error message if validation failed</summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// String representation of the validation result.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (IsValid)
            {
                var compression = IsCompressed ? "Compressed" : "Uncompressed";
                return $"WIFValidationResult(Valid: {IsValid}, {compression}, KeyLength: {PrivateKeyLength})";
            }
            else
            {
                return $"WIFValidationResult(Valid: {IsValid}, Error: {ErrorMessage})";
            }
        }
    }
}