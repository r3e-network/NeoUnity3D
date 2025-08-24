using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Neo.Unity.SDK.Crypto;
using Neo.Unity.SDK.Core;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// Extension methods for byte arrays to support Neo protocol operations.
    /// Provides encoding, hashing, conversion, and validation utilities.
    /// </summary>
    public static class ByteExtensions
    {
        #region Encoding Extensions
        
        /// <summary>
        /// Converts byte array to Base64 encoded string.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Base64 encoded string</returns>
        public static string ToBase64(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            return Convert.ToBase64String(bytes);
        }
        
        /// <summary>
        /// Converts byte array to Base58 encoded string.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Base58 encoded string</returns>
        public static string ToBase58(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            return Base58.Encode(bytes);
        }
        
        /// <summary>
        /// Converts byte array to Base58Check encoded string.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Base58Check encoded string</returns>
        public static string ToBase58CheckEncoded(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            return Base58.Base58CheckEncode(bytes);
        }
        
        /// <summary>
        /// Converts byte array to hexadecimal string without '0x' prefix.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Hex string without prefix</returns>
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            return Convert.ToHexString(bytes);
        }
        
        /// <summary>
        /// Converts byte array to hexadecimal string with lowercase formatting.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Lowercase hex string</returns>
        public static string ToHexStringLower(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
        
        #endregion
        
        #region Hashing Extensions
        
        /// <summary>
        /// Computes SHA-256 hash of the byte array.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>SHA-256 hash</returns>
        public static byte[] Sha256(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            return Hash.Sha256(bytes);
        }
        
        /// <summary>
        /// Computes double SHA-256 hash (Hash256) of the byte array.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Double SHA-256 hash</returns>
        public static byte[] Hash256(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            return Hash.Hash256(bytes);
        }
        
        /// <summary>
        /// Computes Hash160 (RIPEMD-160 of SHA-256) of the byte array.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Hash160</returns>
        public static byte[] Hash160(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            return Hash.Hash160(bytes);
        }
        
        #endregion
        
        #region Conversion Extensions
        
        /// <summary>
        /// Converts byte array to BigInteger.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <param name="isUnsigned">Whether to treat as unsigned</param>
        /// <param name="isBigEndian">Whether bytes are in big-endian format</param>
        /// <returns>The BigInteger value</returns>
        public static BigInteger ToBigInteger(this byte[] bytes, bool isUnsigned = false, bool isBigEndian = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            if (bytes.Length == 0)
                return BigInteger.Zero;
            
            return new BigInteger(bytes, isUnsigned, isBigEndian);
        }
        
        /// <summary>
        /// Converts byte array to numeric type with endianness control.
        /// </summary>
        /// <typeparam name="T">The numeric type</typeparam>
        /// <param name="bytes">The byte array</param>
        /// <param name="littleEndian">Whether to use little-endian byte order</param>
        /// <returns>The converted numeric value</returns>
        public static T ToNumeric<T>(this byte[] bytes, bool littleEndian = true) where T : struct
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            var workingBytes = littleEndian ? bytes : bytes.Reverse().ToArray();
            
            return typeof(T).Name switch
            {
                nameof(UInt16) => (T)(object)BitConverter.ToUInt16(workingBytes, 0),
                nameof(Int16) => (T)(object)BitConverter.ToInt16(workingBytes, 0),
                nameof(UInt32) => (T)(object)BitConverter.ToUInt32(workingBytes, 0),
                nameof(Int32) => (T)(object)BitConverter.ToInt32(workingBytes, 0),
                nameof(UInt64) => (T)(object)BitConverter.ToUInt64(workingBytes, 0),
                nameof(Int64) => (T)(object)BitConverter.ToInt64(workingBytes, 0),
                nameof(Single) => (T)(object)BitConverter.ToSingle(workingBytes, 0),
                nameof(Double) => (T)(object)BitConverter.ToDouble(workingBytes, 0),
                _ => throw new NotSupportedException($"Numeric type {typeof(T).Name} is not supported.")
            };
        }
        
        /// <summary>
        /// Converts script hash bytes to Neo address.
        /// </summary>
        /// <param name="scriptHashBytes">The script hash bytes (20 bytes)</param>
        /// <returns>The Neo address</returns>
        public static string ScriptHashToAddress(this byte[] scriptHashBytes)
        {
            if (scriptHashBytes == null)
                throw new ArgumentNullException(nameof(scriptHashBytes));
            
            if (scriptHashBytes.Length != 20)
                throw new ArgumentException("Script hash must be 20 bytes long.", nameof(scriptHashBytes));
            
            // Create address: version byte + reversed script hash
            var addressBytes = new byte[21];
            addressBytes[0] = NeoUnityConfig.DEFAULT_ADDRESS_VERSION;
            Array.Copy(scriptHashBytes.Reverse().ToArray(), 0, addressBytes, 1, 20);
            
            // Add checksum
            var checksum = addressBytes.Hash256().Take(4).ToArray();
            var finalAddress = addressBytes.Concat(checksum).ToArray();
            
            return finalAddress.ToBase58();
        }
        
        #endregion
        
        #region Array Operations
        
        /// <summary>
        /// Pads byte array to specified length.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <param name="length">Target length</param>
        /// <param name="trailing">Whether to pad at the end (true) or beginning (false)</param>
        /// <returns>Padded byte array</returns>
        public static byte[] ToPadded(this byte[] bytes, int length, bool trailing = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
            
            // Handle leading zero byte
            var startIndex = (bytes.Length > 0 && bytes[0] == 0) ? 1 : 0;
            var effectiveLength = bytes.Length - startIndex;
            
            if (effectiveLength > length)
                throw new ArgumentException($"Input is too large to fit in byte array of size {length}");
            
            var result = new byte[length];
            var sourceBytes = bytes.Skip(startIndex).ToArray();
            
            if (trailing)
            {
                // Pad at the end
                Array.Copy(sourceBytes, 0, result, 0, sourceBytes.Length);
                // Remaining bytes are already zero from new array
            }
            else
            {
                // Pad at the beginning
                Array.Copy(sourceBytes, 0, result, length - sourceBytes.Length, sourceBytes.Length);
                // Leading bytes are already zero from new array
            }
            
            return result;
        }
        
        /// <summary>
        /// Trims trailing bytes of the specified value.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <param name="byteValue">The byte value to trim</param>
        /// <returns>Trimmed byte array</returns>
        public static byte[] TrimTrailingBytes(this byte[] bytes, byte byteValue)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            var list = bytes.ToList();
            
            while (list.Count > 0 && list[list.Count - 1] == byteValue)
            {
                list.RemoveAt(list.Count - 1);
            }
            
            return list.ToArray();
        }
        
        /// <summary>
        /// Performs XOR operation on two byte arrays.
        /// </summary>
        /// <param name="lhs">First byte array</param>
        /// <param name="rhs">Second byte array</param>
        /// <returns>XOR result</returns>
        /// <exception cref="ArgumentException">If arrays have different lengths</exception>
        public static byte[] Xor(this byte[] lhs, byte[] rhs)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));
            
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            
            if (lhs.Length != rhs.Length)
                throw new ArgumentException("Arrays must have the same length to perform XOR operation.");
            
            var result = new byte[lhs.Length];
            for (int i = 0; i < lhs.Length; i++)
            {
                result[i] = (byte)(lhs[i] ^ rhs[i]);
            }
            
            return result;
        }
        
        #endregion
        
        #region Size Calculation
        
        /// <summary>
        /// Gets the variable-length size for this byte array.
        /// Includes the count prefix and the actual data.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Variable-length size in bytes</returns>
        public static int GetVarSize(this byte[] bytes)
        {
            if (bytes == null)
                return GetVarIntSize(0);
            
            return GetVarIntSize(bytes.Length) + bytes.Length;
        }
        
        /// <summary>
        /// Gets the size needed to encode an integer as VarInt.
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <returns>VarInt size in bytes</returns>
        private static int GetVarIntSize(int value)
        {
            if (value < 0)
                return 1;
            
            if (value < 0xFD)
                return 1;
            else if (value <= 0xFFFF)
                return 3; // 0xFD + 2 bytes
            else if (value <= 0xFFFFFFFF)
                return 5; // 0xFE + 4 bytes
            else
                return 9; // 0xFF + 8 bytes
        }
        
        #endregion
        
        #region Variable Length Size Extensions
        
        /// <summary>
        /// Gets the variable-length size for this byte array.
        /// Includes the VarInt length prefix plus the actual data length.
        /// This matches the Swift varSize property.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Variable-length size in bytes (prefix + data)</returns>
        public static int GetVarSize(this byte[] bytes)
        {
            if (bytes == null)
                return GetVarIntSize(0);
            
            return GetVarIntSize(bytes.Length) + bytes.Length;
        }
        
        /// <summary>
        /// Gets the size needed to encode an integer as VarInt.
        /// Matches Neo protocol VarInt encoding rules.
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <returns>VarInt size in bytes</returns>
        private static int GetVarIntSize(int value)
        {
            if (value < 0)
                return 1;
            
            if (value < 0xFD)
                return 1;
            else if (value <= 0xFFFF)
                return 3; // 0xFD + 2 bytes
            else if (value <= 0xFFFFFFFF)
                return 5; // 0xFE + 4 bytes
            else
                return 9; // 0xFF + 8 bytes
        }
        
        #endregion
        
        #region Validation Extensions
        
        /// <summary>
        /// Validates that a byte array represents a valid signature.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>True if valid signature format</returns>
        public static bool IsValidSignature(this byte[] bytes)
        {
            return bytes != null && bytes.Length == NeoConstants.SIGNATURE_SIZE;
        }
        
        /// <summary>
        /// Validates that a byte array represents a valid public key.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>True if valid public key format</returns>
        public static bool IsValidPublicKey(this byte[] bytes)
        {
            if (bytes == null)
                return false;
            
            // Compressed public key: 33 bytes starting with 0x02 or 0x03
            if (bytes.Length == 33 && (bytes[0] == 0x02 || bytes[0] == 0x03))
                return true;
            
            // Uncompressed public key: 65 bytes starting with 0x04
            if (bytes.Length == 65 && bytes[0] == 0x04)
                return true;
            
            // Infinity point
            if (bytes.Length == 1 && bytes[0] == 0x00)
                return true;
            
            return false;
        }
        
        /// <summary>
        /// Validates that a byte array represents a valid Hash160 (script hash).
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>True if valid Hash160 format</returns>
        public static bool IsValidHash160(this byte[] bytes)
        {
            return bytes != null && bytes.Length == 20;
        }
        
        /// <summary>
        /// Validates that a byte array represents a valid Hash256.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>True if valid Hash256 format</returns>
        public static bool IsValidHash256(this byte[] bytes)
        {
            return bytes != null && bytes.Length == 32;
        }
        
        #endregion
        
        #region Utility Extensions
        
        /// <summary>
        /// Safely reverses a byte array (creates a copy).
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <returns>Reversed copy of the byte array</returns>
        public static byte[] SafeReverse(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            var reversed = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                reversed[i] = bytes[bytes.Length - 1 - i];
            }
            
            return reversed;
        }
        
        /// <summary>
        /// Safely concatenates two byte arrays.
        /// </summary>
        /// <param name="first">First byte array</param>
        /// <param name="second">Second byte array</param>
        /// <returns>Concatenated byte array</returns>
        public static byte[] Concat(this byte[] first, byte[] second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            
            var result = new byte[first.Length + second.Length];
            Array.Copy(first, 0, result, 0, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);
            
            return result;
        }
        
        /// <summary>
        /// Creates a safe slice of the byte array.
        /// </summary>
        /// <param name="bytes">The byte array</param>
        /// <param name="start">Start index</param>
        /// <param name="length">Length of slice</param>
        /// <returns>Sliced byte array</returns>
        public static byte[] SafeSlice(this byte[] bytes, int start, int length)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            
            if (start < 0 || start >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            
            if (length < 0 || start + length > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(length));
            
            var result = new byte[length];
            Array.Copy(bytes, start, result, 0, length);
            
            return result;
        }
        
        /// <summary>
        /// Compares two byte arrays for equality.
        /// </summary>
        /// <param name="first">First byte array</param>
        /// <param name="second">Second byte array</param>
        /// <returns>True if arrays are equal</returns>
        public static bool SequenceEqual(this byte[] first, byte[] second)
        {
            if (ReferenceEquals(first, second))
                return true;
            
            if (first == null || second == null)
                return false;
            
            if (first.Length != second.Length)
                return false;
            
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Neo Address Conversion
        
        /// <summary>
        /// Converts script hash bytes to Neo address format.
        /// </summary>
        /// <param name="scriptHashBytes">The script hash bytes (must be 20 bytes)</param>
        /// <returns>The Neo address string</returns>
        public static string ToNeoAddress(this byte[] scriptHashBytes)
        {
            return scriptHashBytes.ScriptHashToAddress();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Extension methods for individual bytes.
    /// </summary>
    public static class ByteValueExtensions
    {
        /// <summary>
        /// Checks if a byte value is between two OpCode values (inclusive).
        /// </summary>
        /// <param name="value">The byte value</param>
        /// <param name="opCode1">First OpCode</param>
        /// <param name="opCode2">Second OpCode</param>
        /// <returns>True if the byte is between the two opcodes</returns>
        public static bool IsBetween(this byte value, OpCode opCode1, OpCode opCode2)
        {
            return value >= (byte)opCode1 && value <= (byte)opCode2;
        }
        
        /// <summary>
        /// Converts a byte to its hex string representation.
        /// </summary>
        /// <param name="value">The byte value</param>
        /// <returns>Two-character hex string</returns>
        public static string ToHex(this byte value)
        {
            return value.ToString("X2");
        }
        
        /// <summary>
        /// Checks if a byte represents a valid OpCode.
        /// </summary>
        /// <param name="value">The byte value</param>
        /// <returns>True if it's a valid OpCode</returns>
        public static bool IsValidOpCode(this byte value)
        {
            return Enum.IsDefined(typeof(OpCode), value);
        }
    }
}