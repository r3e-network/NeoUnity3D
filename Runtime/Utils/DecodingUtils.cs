using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// Interface for types that can be decoded from string representations.
    /// This is useful for JSON deserialization and other string-based formats.
    /// </summary>
    public interface IStringDecodable
    {
        /// <summary>
        /// Creates an instance from a string representation.
        /// </summary>
        /// <param name="value">The string value to decode.</param>
        /// <returns>A decoded instance.</returns>
        static abstract object FromString(string value);

        /// <summary>
        /// Gets the string representation of this instance.
        /// </summary>
        /// <returns>The string representation.</returns>
        string ToString();
    }

    /// <summary>
    /// A safe decoder wrapper that attempts multiple decoding strategies.
    /// </summary>
    /// <typeparam name="T">The type to decode.</typeparam>
    [Serializable]
    public class SafeDecoder<T>
    {
        [SerializeField]
        private T _value;

        /// <summary>
        /// Gets the decoded value.
        /// </summary>
        public T Value => _value;

        /// <summary>
        /// Initializes a new instance with the specified value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public SafeDecoder(T value)
        {
            _value = value;
        }

        /// <summary>
        /// Attempts to decode a value from various sources.
        /// </summary>
        /// <param name="stringValue">The string representation.</param>
        /// <param name="directValue">The direct value.</param>
        /// <returns>A SafeDecoder instance.</returns>
        public static SafeDecoder<T> Decode(string stringValue = null, T directValue = default)
        {
            // Try direct value first
            if (!EqualityComparer<T>.Default.Equals(directValue, default))
            {
                return new SafeDecoder<T>(directValue);
            }

            // Try string decoding
            if (!string.IsNullOrEmpty(stringValue))
            {
                var decoded = DecodeFromString(stringValue);
                if (!EqualityComparer<T>.Default.Equals(decoded, default))
                {
                    return new SafeDecoder<T>(decoded);
                }
            }

            return new SafeDecoder<T>(default);
        }

        private static T DecodeFromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return default;

            var type = typeof(T);

            try
            {
                // Handle common types
                if (type == typeof(bool))
                {
                    return (T)(object)DecodingUtils.DecodeBool(value);
                }
                else if (type == typeof(int))
                {
                    return (T)(object)DecodingUtils.DecodeInt(value);
                }
                else if (type == typeof(long))
                {
                    return (T)(object)DecodingUtils.DecodeLong(value);
                }
                else if (type == typeof(decimal))
                {
                    return (T)(object)DecodingUtils.DecodeDecimal(value);
                }
                else if (type == typeof(BigInteger))
                {
                    return (T)(object)DecodingUtils.DecodeBigInteger(value);
                }
                else if (type == typeof(byte[]))
                {
                    return (T)(object)DecodingUtils.DecodeBytes(value);
                }
                else if (type == typeof(string))
                {
                    return (T)(object)value;
                }

                // Try JSON deserialization for complex types
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return default;
            }
        }
    }

    /// <summary>
    /// Utility methods for decoding various data types from strings and other formats.
    /// </summary>
    public static class DecodingUtils
    {
        /// <summary>
        /// Decodes a boolean value from a string.
        /// Supports various string representations of true/false.
        /// </summary>
        /// <param name="value">The string to decode.</param>
        /// <returns>The decoded boolean value.</returns>
        /// <exception cref="ArgumentException">Thrown when the string cannot be decoded as a boolean.</exception>
        public static bool DecodeBool(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Cannot decode boolean from null or empty string", nameof(value));

            value = value.Trim().ToLowerInvariant();

            return value switch
            {
                "true" or "1" or "yes" or "on" or "enabled" => true,
                "false" or "0" or "no" or "off" or "disabled" => false,
                _ => throw new ArgumentException($"Unable to decode boolean from string '{value}'", nameof(value))
            };
        }

        /// <summary>
        /// Safely decodes a boolean value from a string, returning a default value on failure.
        /// </summary>
        /// <param name="value">The string to decode.</param>
        /// <param name="defaultValue">The default value to return on failure.</param>
        /// <returns>The decoded boolean value or the default value.</returns>
        public static bool DecodeBoolSafe(string value, bool defaultValue = false)
        {
            try
            {
                return DecodeBool(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Decodes an integer value from a string.
        /// Supports various number formats including hexadecimal.
        /// </summary>
        /// <param name="value">The string to decode.</param>
        /// <returns>The decoded integer value.</returns>
        /// <exception cref="ArgumentException">Thrown when the string cannot be decoded as an integer.</exception>
        public static int DecodeInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Cannot decode integer from null or empty string", nameof(value));

            value = value.Trim();

            // Handle hexadecimal
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexResult))
                    return hexResult;
            }

            // Handle decimal
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                return result;

            throw new ArgumentException($"Unable to decode integer from string '{value}'", nameof(value));
        }

        /// <summary>
        /// Decodes a long value from a string.
        /// Supports various number formats including hexadecimal.
        /// </summary>
        /// <param name="value">The string to decode.</param>
        /// <returns>The decoded long value.</returns>
        /// <exception cref="ArgumentException">Thrown when the string cannot be decoded as a long.</exception>
        public static long DecodeLong(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Cannot decode long from null or empty string", nameof(value));

            value = value.Trim();

            // Handle hexadecimal
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (long.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long hexResult))
                    return hexResult;
            }

            // Handle decimal
            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
                return result;

            throw new ArgumentException($"Unable to decode long from string '{value}'", nameof(value));
        }

        /// <summary>
        /// Decodes a decimal value from a string.
        /// </summary>
        /// <param name="value">The string to decode.</param>
        /// <returns>The decoded decimal value.</returns>
        /// <exception cref="ArgumentException">Thrown when the string cannot be decoded as a decimal.</exception>
        public static decimal DecodeDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Cannot decode decimal from null or empty string", nameof(value));

            value = value.Trim();

            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result))
                return result;

            throw new ArgumentException($"Unable to decode decimal from string '{value}'", nameof(value));
        }

        /// <summary>
        /// Decodes a BigInteger value from a string.
        /// Supports various number formats including hexadecimal.
        /// </summary>
        /// <param name="value">The string to decode.</param>
        /// <returns>The decoded BigInteger value.</returns>
        /// <exception cref="ArgumentException">Thrown when the string cannot be decoded as a BigInteger.</exception>
        public static BigInteger DecodeBigInteger(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Cannot decode BigInteger from null or empty string", nameof(value));

            value = value.Trim();

            // Handle hexadecimal
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (BigInteger.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out BigInteger hexResult))
                    return hexResult;
            }

            // Handle decimal
            if (BigInteger.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out BigInteger result))
                return result;

            throw new ArgumentException($"Unable to decode BigInteger from string '{value}'", nameof(value));
        }

        /// <summary>
        /// Decodes a byte array from a string.
        /// Supports both Base64 and hexadecimal formats.
        /// </summary>
        /// <param name="value">The string to decode.</param>
        /// <returns>The decoded byte array.</returns>
        /// <exception cref="ArgumentException">Thrown when the string cannot be decoded as bytes.</exception>
        public static byte[] DecodeBytes(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Array.Empty<byte>();

            value = value.Trim();

            // Try hexadecimal first
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return DecodeHexString(value.Substring(2));
            }

            // Check if it looks like hex (even length, only hex characters)
            if (value.Length % 2 == 0 && IsHexString(value))
            {
                return DecodeHexString(value);
            }

            // Try Base64
            try
            {
                return Convert.FromBase64String(value);
            }
            catch
            {
                // If not Base64, try UTF-8 encoding
                return Encoding.UTF8.GetBytes(value);
            }
        }

        /// <summary>
        /// Decodes a hexadecimal string to a byte array.
        /// </summary>
        /// <param name="hex">The hexadecimal string (without 0x prefix).</param>
        /// <returns>The decoded byte array.</returns>
        /// <exception cref="ArgumentException">Thrown when the string is not valid hexadecimal.</exception>
        public static byte[] DecodeHexString(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Array.Empty<byte>();

            hex = hex.Trim();

            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hexadecimal string must have an even number of characters", nameof(hex));

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                catch
                {
                    throw new ArgumentException($"Invalid hexadecimal character at position {i * 2}", nameof(hex));
                }
            }

            return bytes;
        }

        /// <summary>
        /// Checks if a string contains only hexadecimal characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string contains only hexadecimal characters, false otherwise.</returns>
        public static bool IsHexString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            foreach (char c in value)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to decode any object from JSON format with error handling.
        /// </summary>
        /// <typeparam name="T">The type to decode to.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <param name="result">The decoded result if successful.</param>
        /// <returns>True if decoding was successful, false otherwise.</returns>
        public static bool TryDecodeJson<T>(string json, out T result)
        {
            result = default;

            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                result = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Decodes a collection from either a single value or an array.
        /// Useful for handling APIs that might return single values or arrays.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>A list containing the decoded elements.</returns>
        public static List<T> DecodeSingleValueOrArray<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new List<T>();

            try
            {
                // Try to deserialize as array first
                if (json.Trim().StartsWith("["))
                {
                    var array = JsonConvert.DeserializeObject<T[]>(json);
                    return array != null ? new List<T>(array) : new List<T>();
                }
                else
                {
                    // Try as single value
                    var single = JsonConvert.DeserializeObject<T>(json);
                    return single != null ? new List<T> { single } : new List<T>();
                }
            }
            catch
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Creates a specialized JSON converter for handling string-decodable types.
        /// </summary>
        /// <typeparam name="T">The type that can be decoded from strings.</typeparam>
        /// <returns>A JsonConverter for the specified type.</returns>
        public static JsonConverter CreateStringDecodableConverter<T>() where T : IStringDecodable
        {
            return new StringDecodableJsonConverter<T>();
        }
    }

    /// <summary>
    /// JSON converter for types that implement IStringDecodable.
    /// </summary>
    /// <typeparam name="T">The string-decodable type.</typeparam>
    public class StringDecodableJsonConverter<T> : JsonConverter<T> where T : IStringDecodable
    {
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value.ToString());
            }
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return default;

            if (reader.Value is string stringValue)
            {
                return (T)T.FromString(stringValue);
            }

            // Try to convert other types to string first
            var convertedString = reader.Value.ToString();
            return (T)T.FromString(convertedString);
        }
    }

    /// <summary>
    /// Attribute to mark properties that should be decoded flexibly (from single values or arrays).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class FlexibleArrayAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether null values should be treated as empty arrays.
        /// </summary>
        public bool NullAsEmpty { get; set; } = true;
    }
}