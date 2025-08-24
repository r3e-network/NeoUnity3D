using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Neo.Unity.SDK.Crypto
{
    /// <summary>
    /// Base58 and Base58Check encoding/decoding implementation.
    /// Used for Neo addresses and other blockchain data encoding.
    /// Production-ready implementation with proper validation and error handling.
    /// </summary>
    public static class Base58
    {
        #region Constants
        
        /// <summary>Base58 alphabet used by Bitcoin and Neo</summary>
        private const string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        
        /// <summary>Base58 radix</summary>
        private const int BASE = 58;
        
        #endregion
        
        #region Encoding
        
        /// <summary>
        /// Encodes byte array to Base58 string.
        /// </summary>
        /// <param name="data">Data to encode</param>
        /// <returns>Base58 encoded string</returns>
        public static string Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            if (data.Length == 0)
                return string.Empty;
            
            // Count leading zeros
            var leadingZeros = 0;
            while (leadingZeros < data.Length && data[leadingZeros] == 0)
            {
                leadingZeros++;
            }
            
            // Convert to BigInteger and encode
            var number = new BigInteger(data.Reverse().Concat(new byte[] { 0 }).ToArray());
            var result = new StringBuilder();
            
            while (number > 0)
            {
                number = BigInteger.DivRem(number, BASE, out var remainder);
                result.Insert(0, ALPHABET[(int)remainder]);
            }
            
            // Add leading '1's for leading zeros
            for (int i = 0; i < leadingZeros; i++)
            {
                result.Insert(0, ALPHABET[0]);
            }
            
            return result.ToString();
        }
        
        /// <summary>
        /// Encodes string to Base58.
        /// </summary>
        /// <param name="input">String to encode</param>
        /// <returns>Base58 encoded string</returns>
        public static string Encode(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            var bytes = Encoding.UTF8.GetBytes(input);
            return Encode(bytes);
        }
        
        #endregion
        
        #region Decoding
        
        /// <summary>
        /// Decodes Base58 string to byte array.
        /// </summary>
        /// <param name="encoded">Base58 encoded string</param>
        /// <returns>Decoded byte array</returns>
        public static byte[] Decode(string encoded)
        {
            if (encoded == null)
                throw new ArgumentNullException(nameof(encoded));
            
            if (string.IsNullOrEmpty(encoded))
                return new byte[0];
            
            // Validate characters
            foreach (char c in encoded)
            {
                if (!ALPHABET.Contains(c))
                    throw new ArgumentException($"Invalid Base58 character: {c}", nameof(encoded));
            }
            
            // Count leading '1's
            var leadingOnes = 0;
            while (leadingOnes < encoded.Length && encoded[leadingOnes] == ALPHABET[0])
            {
                leadingOnes++;
            }
            
            // Decode to BigInteger
            var number = BigInteger.Zero;
            var baseMultiplier = BigInteger.One;
            
            for (int i = encoded.Length - 1; i >= leadingOnes; i--)
            {
                var digit = ALPHABET.IndexOf(encoded[i]);
                if (digit < 0)
                    throw new ArgumentException($"Invalid Base58 character: {encoded[i]}", nameof(encoded));
                
                number += digit * baseMultiplier;
                baseMultiplier *= BASE;
            }
            
            // Convert to byte array
            var result = number.ToByteArray().Reverse().ToArray();
            
            // Remove extra zero byte if present (from BigInteger)
            if (result.Length > 1 && result[0] == 0)
            {
                result = result.Skip(1).ToArray();
            }
            
            // Add leading zeros for leading '1's
            if (leadingOnes > 0)
            {
                var withLeadingZeros = new byte[leadingOnes + result.Length];
                Array.Copy(result, 0, withLeadingZeros, leadingOnes, result.Length);
                result = withLeadingZeros;
            }
            
            return result;
        }
        
        #endregion
        
        #region Base58Check Operations
        
        /// <summary>
        /// Encodes data with Base58Check (includes checksum).
        /// </summary>
        /// <param name="data">Data to encode</param>
        /// <returns>Base58Check encoded string</returns>
        public static string Base58CheckEncode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            
            // Add checksum (first 4 bytes of double SHA-256)
            var checksum = Hash.Hash256(data).Take(4).ToArray();
            var dataWithChecksum = data.Concat(checksum).ToArray();
            
            return Encode(dataWithChecksum);
        }
        
        /// <summary>
        /// Decodes Base58Check string and validates checksum.
        /// </summary>
        /// <param name="encoded">Base58Check encoded string</param>
        /// <returns>Decoded data without checksum</returns>
        /// <exception cref="ArgumentException">If checksum is invalid</exception>
        public static byte[] Base58CheckDecode(string encoded)
        {
            if (encoded == null)
                throw new ArgumentNullException(nameof(encoded));
            
            var decoded = Decode(encoded);
            
            if (decoded.Length < 4)
                throw new ArgumentException("Base58Check data too short for checksum", nameof(encoded));
            
            // Split data and checksum
            var data = decoded.Take(decoded.Length - 4).ToArray();
            var checksum = decoded.Skip(decoded.Length - 4).ToArray();
            
            // Verify checksum
            var computedChecksum = Hash.Hash256(data).Take(4).ToArray();
            
            if (!checksum.SequenceEqual(computedChecksum))
                throw new ArgumentException("Invalid Base58Check checksum", nameof(encoded));
            
            return data;
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Validates if a string is valid Base58 format.
        /// </summary>
        /// <param name="input">String to validate</param>
        /// <returns>True if valid Base58</returns>
        public static bool IsValidBase58(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            
            return input.All(c => ALPHABET.Contains(c));
        }
        
        /// <summary>
        /// Validates if a string is valid Base58Check format.
        /// </summary>
        /// <param name="input">String to validate</param>
        /// <returns>True if valid Base58Check</returns>
        public static bool IsValidBase58Check(string input)
        {
            try
            {
                Base58CheckDecode(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
    }
}