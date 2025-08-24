using System;
using System.Numerics;
using System.Globalization;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// Extension methods for numeric types that provide convenience operations
    /// commonly used in Neo blockchain operations.
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// Converts a BigInteger to a byte array with the specified length, padding with leading zeros if necessary.
        /// </summary>
        /// <param name="value">The BigInteger value.</param>
        /// <param name="length">The desired byte array length.</param>
        /// <returns>A byte array representation padded to the specified length.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is too large for the specified length.</exception>
        public static byte[] ToBytesPadded(this BigInteger value, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative");

            var bytes = value.ToByteArray();
            
            // Remove unnecessary leading zero bytes for positive numbers
            if (bytes.Length > 1 && bytes[bytes.Length - 1] == 0)
            {
                var trimmed = new byte[bytes.Length - 1];
                Array.Copy(bytes, trimmed, trimmed.Length);
                bytes = trimmed;
            }

            if (bytes.Length > length)
                throw new ArgumentOutOfRangeException(nameof(length), $"Value too large for specified length. Required: {bytes.Length}, Available: {length}");

            if (bytes.Length == length)
                return bytes;

            // Pad with leading zeros
            var result = new byte[length];
            Array.Copy(bytes, result, bytes.Length);
            return result;
        }

        /// <summary>
        /// Raises an integer to the power of another integer.
        /// </summary>
        /// <param name="baseValue">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>The result of base raised to the power of exponent.</returns>
        public static int ToPowerOf(this int baseValue, int exponent)
        {
            if (exponent < 0)
                throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent cannot be negative");

            return (int)Math.Pow(baseValue, exponent);
        }

        /// <summary>
        /// Raises a long to the power of another integer.
        /// </summary>
        /// <param name="baseValue">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>The result of base raised to the power of exponent.</returns>
        public static long ToPowerOf(this long baseValue, int exponent)
        {
            if (exponent < 0)
                throw new ArgumentOutOfRangeException(nameof(exponent), "Exponent cannot be negative");

            return (long)Math.Pow(baseValue, exponent);
        }

        /// <summary>
        /// Gets the variable-length size encoding size for an integer.
        /// This matches the Neo protocol's variable-length integer encoding.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>The number of bytes required to encode the integer as a variable-length integer.</returns>
        public static int GetVarSize(this int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative for VarInt encoding");

            if (value < 0xFD)
                return 1;
            else if (value <= 0xFFFF)
                return 3;
            else if (value <= 0xFFFFFFFF)
                return 5;
            else
                return 9;
        }

        /// <summary>
        /// Gets the variable-length size encoding size for a long.
        /// This matches the Neo protocol's variable-length integer encoding.
        /// </summary>
        /// <param name="value">The long value.</param>
        /// <returns>The number of bytes required to encode the long as a variable-length integer.</returns>
        public static int GetVarSize(this long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative for VarInt encoding");

            if (value < 0xFD)
                return 1;
            else if (value <= 0xFFFF)
                return 3;
            else if (value <= 0xFFFFFFFF)
                return 5;
            else
                return 9;
        }

        /// <summary>
        /// Gets the variable-length size encoding size for a BigInteger.
        /// </summary>
        /// <param name="value">The BigInteger value.</param>
        /// <returns>The number of bytes required to encode the BigInteger as a variable-length integer.</returns>
        public static int GetVarSize(this BigInteger value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative for VarInt encoding");

            if (value < 0xFD)
                return 1;
            else if (value <= 0xFFFF)
                return 3;
            else if (value <= 0xFFFFFFFF)
                return 5;
            else
                return 9;
        }

        /// <summary>
        /// Converts an integer to its unsigned representation (masks to 32 bits).
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>The unsigned 32-bit representation.</returns>
        public static uint ToUnsigned(this int value)
        {
            return (uint)(value & 0xFFFFFFFF);
        }

        /// <summary>
        /// Converts a long to its unsigned representation (masks to 64 bits).
        /// </summary>
        /// <param name="value">The long value.</param>
        /// <returns>The unsigned 64-bit representation.</returns>
        public static ulong ToUnsigned(this long value)
        {
            return (ulong)value;
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in little-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in little-endian format.</returns>
        public static byte[] ToLittleEndianBytes(this short value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in little-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in little-endian format.</returns>
        public static byte[] ToLittleEndianBytes(this ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in little-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in little-endian format.</returns>
        public static byte[] ToLittleEndianBytes(this int value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in little-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in little-endian format.</returns>
        public static byte[] ToLittleEndianBytes(this uint value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in little-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in little-endian format.</returns>
        public static byte[] ToLittleEndianBytes(this long value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in little-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in little-endian format.</returns>
        public static byte[] ToLittleEndianBytes(this ulong value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in big-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in big-endian format.</returns>
        public static byte[] ToBigEndianBytes(this short value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in big-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in big-endian format.</returns>
        public static byte[] ToBigEndianBytes(this ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in big-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in big-endian format.</returns>
        public static byte[] ToBigEndianBytes(this int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in big-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in big-endian format.</returns>
        public static byte[] ToBigEndianBytes(this uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in big-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in big-endian format.</returns>
        public static byte[] ToBigEndianBytes(this long value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Converts a numeric value to its byte representation in big-endian format.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <returns>A byte array representing the value in big-endian format.</returns>
        public static byte[] ToBigEndianBytes(this ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        /// <summary>
        /// Creates a BigInteger from a byte array interpreting it as little-endian.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>A BigInteger representation of the byte array.</returns>
        public static BigInteger ToBigInteger(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return BigInteger.Zero;

            return new BigInteger(bytes);
        }

        /// <summary>
        /// Gets the scale (number of decimal places) from a decimal value.
        /// </summary>
        /// <param name="value">The decimal value.</param>
        /// <returns>The number of decimal places.</returns>
        public static int GetScale(this decimal value)
        {
            var bits = decimal.GetBits(value);
            return (bits[3] >> 16) & 0xFF;
        }

        /// <summary>
        /// Clamps a value between a minimum and maximum value.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The clamped value.</returns>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }

        /// <summary>
        /// Determines if a number is within a specified range (inclusive).
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The minimum value (inclusive).</param>
        /// <param name="max">The maximum value (inclusive).</param>
        /// <returns>True if the value is within the range, false otherwise.</returns>
        public static bool IsInRange<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
        }

        /// <summary>
        /// Safely converts a string to an integer, returning a default value on failure.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="defaultValue">The default value to return on failure.</param>
        /// <returns>The converted integer or the default value.</returns>
        public static int ToIntSafe(this string value, int defaultValue = 0)
        {
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Safely converts a string to a long, returning a default value on failure.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="defaultValue">The default value to return on failure.</param>
        /// <returns>The converted long or the default value.</returns>
        public static long ToLongSafe(this string value, long defaultValue = 0)
        {
            return long.TryParse(value, out long result) ? result : defaultValue;
        }

        /// <summary>
        /// Safely converts a string to a decimal, returning a default value on failure.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="defaultValue">The default value to return on failure.</param>
        /// <returns>The converted decimal or the default value.</returns>
        public static decimal ToDecimalSafe(this string value, decimal defaultValue = 0)
        {
            return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result) ? result : defaultValue;
        }

        /// <summary>
        /// Safely converts a string to a BigInteger, returning a default value on failure.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="defaultValue">The default value to return on failure.</param>
        /// <returns>The converted BigInteger or the default value.</returns>
        public static BigInteger ToBigIntegerSafe(this string value, BigInteger defaultValue = default)
        {
            return BigInteger.TryParse(value, out BigInteger result) ? result : defaultValue;
        }

        /// <summary>
        /// Converts milliseconds since Unix epoch to DateTime.
        /// </summary>
        /// <param name="milliseconds">The milliseconds since Unix epoch.</param>
        /// <returns>A DateTime representing the timestamp.</returns>
        public static DateTime FromUnixMilliseconds(this long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
        }

        /// <summary>
        /// Converts a DateTime to milliseconds since Unix epoch.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert.</param>
        /// <returns>Milliseconds since Unix epoch.</returns>
        public static long ToUnixMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }
    }
}