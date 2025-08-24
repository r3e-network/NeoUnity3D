using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Core;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// Extension methods for string operations related to Neo blockchain functionality.
    /// Provides encoding, validation, conversion, and utility methods.
    /// </summary>
    public static class StringExtensions
    {
        #region Hex String Operations
        
        /// <summary>
        /// Converts hex string to byte array.
        /// </summary>
        /// <param name="hexString">The hex string</param>
        /// <returns>Byte array representation</returns>
        /// <exception cref="ArgumentException">If hex string is invalid</exception>
        public static byte[] BytesFromHex(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return new byte[0];
            
            var cleaned = hexString.CleanHexPrefix();
            
            if (!cleaned.IsValidHex())
                throw new ArgumentException("Invalid hex string format.", nameof(hexString));
            
            return Convert.FromHexString(cleaned);
        }
        
        /// <summary>
        /// Removes '0x' prefix from hex string if present.
        /// </summary>
        /// <param name="hexString">The hex string</param>
        /// <returns>Hex string without prefix</returns>
        public static string CleanHexPrefix(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return string.Empty;
            
            return hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? 
                hexString.Substring(2) : hexString;
        }
        
        /// <summary>
        /// Validates if string is a valid hexadecimal format.
        /// </summary>
        /// <param name="str">The string to validate</param>
        /// <returns>True if valid hex format</returns>
        public static bool IsValidHex(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return true; // Empty string is valid (represents empty byte array)
            
            var cleaned = str.CleanHexPrefix();
            
            // Must have even length for valid byte representation
            if (cleaned.Length % 2 != 0)
                return false;
            
            // All characters must be hex digits
            return cleaned.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
        }
        
        /// <summary>
        /// Reverses the byte order of a hex string.
        /// </summary>
        /// <param name="hexString">The hex string</param>
        /// <returns>Hex string with reversed byte order</returns>
        public static string ReverseHex(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return string.Empty;
            
            var bytes = hexString.BytesFromHex();
            return bytes.SafeReverse().ToHexString();
        }
        
        #endregion
        
        #region Base64 Operations
        
        /// <summary>
        /// Decodes Base64 string to byte array.
        /// </summary>
        /// <param name="base64String">The Base64 string</param>
        /// <returns>Decoded byte array</returns>
        public static byte[] Base64Decoded(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return new byte[0];
            
            try
            {
                return Convert.FromBase64String(base64String);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid Base64 string format.", nameof(base64String), ex);
            }
        }
        
        /// <summary>
        /// Encodes hex string to Base64.
        /// </summary>
        /// <param name="hexString">The hex string</param>
        /// <returns>Base64 encoded string</returns>
        public static string Base64Encoded(this string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return string.Empty;
            
            var bytes = hexString.BytesFromHex();
            return bytes.ToBase64();
        }
        
        #endregion
        
        #region Base58 Operations
        
        /// <summary>
        /// Decodes Base58 string to byte array.
        /// </summary>
        /// <param name="base58String">The Base58 string</param>
        /// <returns>Decoded byte array or null if invalid</returns>
        public static byte[] Base58Decoded(this string base58String)
        {
            if (string.IsNullOrEmpty(base58String))
                return new byte[0];
            
            try
            {
                return Base58.Decode(base58String);
            }
            catch
            {
                return null; // Return null for invalid Base58
            }
        }
        
        /// <summary>
        /// Decodes Base58Check string to byte array.
        /// </summary>
        /// <param name="base58CheckString">The Base58Check string</param>
        /// <returns>Decoded byte array or null if invalid</returns>
        public static byte[] Base58CheckDecoded(this string base58CheckString)
        {
            if (string.IsNullOrEmpty(base58CheckString))
                return new byte[0];
            
            try
            {
                return Base58.Base58CheckDecode(base58CheckString);
            }
            catch
            {
                return null; // Return null for invalid Base58Check
            }
        }
        
        /// <summary>
        /// Encodes string to Base58 using UTF-8 encoding.
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>Base58 encoded string</returns>
        public static string Base58Encoded(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            
            var bytes = Encoding.UTF8.GetBytes(str);
            return bytes.ToBase58();
        }
        
        #endregion
        
        #region Neo Address Operations
        
        /// <summary>
        /// Validates if string is a valid Neo address.
        /// </summary>
        /// <param name="address">The address string</param>
        /// <returns>True if valid Neo address</returns>
        public static bool IsValidAddress(this string address)
        {
            if (string.IsNullOrEmpty(address))
                return false;
            
            try
            {
                var decoded = address.Base58Decoded();
                if (decoded == null || decoded.Length != 25)
                    return false;
                
                // Check version byte
                if (decoded[0] != NeoUnityConfig.DEFAULT_ADDRESS_VERSION)
                    return false;
                
                // Verify checksum
                var addressData = decoded.Take(21).ToArray();
                var checksum = addressData.Hash256().Take(4).ToArray();
                var providedChecksum = decoded.Skip(21).ToArray();
                
                return checksum.SequenceEqual(providedChecksum);
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Converts Neo address to script hash bytes.
        /// </summary>
        /// <param name="address">The Neo address</param>
        /// <returns>Script hash bytes (20 bytes)</returns>
        /// <exception cref="ArgumentException">If address is invalid</exception>
        public static byte[] AddressToScriptHash(this string address)
        {
            if (!address.IsValidAddress())
                throw new ArgumentException("Not a valid Neo address.", nameof(address));
            
            var decoded = address.Base58Decoded();
            if (decoded == null)
                throw new ArgumentException("Failed to decode Neo address.", nameof(address));
            
            // Extract script hash (bytes 1-20) and reverse for Hash160 format
            return decoded.Skip(1).Take(20).Reverse().ToArray();
        }
        
        #endregion
        
        #region WIF Operations
        
        /// <summary>
        /// Extracts private key bytes from WIF (Wallet Import Format) string.
        /// </summary>
        /// <param name="wif">The WIF string</param>
        /// <returns>Private key bytes</returns>
        /// <exception cref="ArgumentException">If WIF is invalid</exception>
        public static byte[] PrivateKeyFromWIF(this string wif)
        {
            if (string.IsNullOrEmpty(wif))
                throw new ArgumentException("WIF cannot be null or empty.", nameof(wif));
            
            try
            {
                return WIF.GetPrivateKeyFromWIF(wif);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid WIF format: {ex.Message}", nameof(wif), ex);
            }
        }
        
        #endregion
        
        #region Numeric Conversions
        
        /// <summary>
        /// Converts string to integer with error handling.
        /// </summary>
        /// <param name="str">The string</param>
        /// <param name="defaultValue">Default value if conversion fails</param>
        /// <returns>The integer value or default</returns>
        public static int ToIntSafe(this string str, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;
            
            return int.TryParse(str, out var result) ? result : defaultValue;
        }
        
        /// <summary>
        /// Converts string to long with error handling.
        /// </summary>
        /// <param name="str">The string</param>
        /// <param name="defaultValue">Default value if conversion fails</param>
        /// <returns>The long value or default</returns>
        public static long ToLongSafe(this string str, long defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;
            
            return long.TryParse(str, out var result) ? result : defaultValue;
        }
        
        /// <summary>
        /// Converts string to decimal with error handling.
        /// </summary>
        /// <param name="str">The string</param>
        /// <param name="defaultValue">Default value if conversion fails</param>
        /// <returns>The decimal value or default</returns>
        public static decimal ToDecimalSafe(this string str, decimal defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;
            
            return decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? 
                result : defaultValue;
        }
        
        #endregion
        
        #region Size Calculation
        
        /// <summary>
        /// Gets the variable-length size for this string when encoded as VarString.
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>VarString size in bytes</returns>
        public static int GetVarSize(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 1; // Just the length prefix for empty string
            
            var bytes = Encoding.UTF8.GetBytes(str);
            return bytes.GetVarSize();
        }
        
        #endregion
        
        #region Validation Utilities
        
        /// <summary>
        /// Checks if string is null, empty, or whitespace.
        /// </summary>
        /// <param name="str">The string</param>
        /// <returns>True if null, empty, or whitespace only</returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
        
        /// <summary>
        /// Gets a safe substring that won't throw exceptions.
        /// </summary>
        /// <param name="str">The string</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="length">Length of substring</param>
        /// <returns>Safe substring or empty string if parameters are invalid</returns>
        public static string SafeSubstring(this string str, int startIndex, int length = -1)
        {
            if (string.IsNullOrEmpty(str) || startIndex < 0 || startIndex >= str.Length)
                return string.Empty;
            
            if (length < 0)
                length = str.Length - startIndex;
            
            var actualLength = Math.Min(length, str.Length - startIndex);
            return str.Substring(startIndex, actualLength);
        }
        
        #endregion
    }
}