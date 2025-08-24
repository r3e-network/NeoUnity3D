using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Neo.Unity.SDK.Utils
{
    /// <summary>
    /// Interface for enums that can be represented as bytes and have JSON values.
    /// This is equivalent to the Swift ByteEnum protocol.
    /// </summary>
    public interface IByteEnum
    {
        /// <summary>
        /// Gets the byte representation of this enum value.
        /// </summary>
        byte ByteValue { get; }

        /// <summary>
        /// Gets the JSON string representation of this enum value.
        /// </summary>
        string JsonValue { get; }
    }

    /// <summary>
    /// Extension methods for enum operations that provide convenience functions
    /// commonly used in Neo blockchain operations.
    /// </summary>
    public static class EnumExtensions
    {
        private static readonly Dictionary<Type, Dictionary<byte, object>> _byteValueCache = new();
        private static readonly Dictionary<Type, Dictionary<string, object>> _jsonValueCache = new();
        private static readonly Dictionary<Type, object[]> _allValuesCache = new();

        /// <summary>
        /// Gets the description attribute value for an enum value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The description string, or the enum name if no description is found.</returns>
        public static string GetDescription(this Enum value)
        {
            if (value == null)
                return string.Empty;

            var field = value.GetType().GetField(value.ToString());
            if (field == null)
                return value.ToString();

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// Gets all values of an enum type.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>An array of all enum values.</returns>
        public static T[] GetAllValues<T>() where T : Enum
        {
            var type = typeof(T);
            
            if (_allValuesCache.TryGetValue(type, out var cachedValues))
            {
                return (T[])cachedValues;
            }

            var values = Enum.GetValues(type).Cast<T>().ToArray();
            _allValuesCache[type] = values.Cast<object>().ToArray();
            return values;
        }

        /// <summary>
        /// Tries to parse an enum from a string value, ignoring case.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="result">The parsed enum value if successful.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public static bool TryParseIgnoreCase<T>(string value, out T result) where T : struct, Enum
        {
            return Enum.TryParse(value, true, out result);
        }

        /// <summary>
        /// Parses an enum from a string value, ignoring case, with a default value on failure.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <param name="defaultValue">The default value to return on failure.</param>
        /// <returns>The parsed enum value or the default value.</returns>
        public static T ParseOrDefault<T>(string value, T defaultValue = default) where T : struct, Enum
        {
            return TryParseIgnoreCase<T>(value, out T result) ? result : defaultValue;
        }

        /// <summary>
        /// Gets an enum value from its byte representation.
        /// This method works with enums that implement IByteEnum or have byte underlying types.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="byteValue">The byte value.</param>
        /// <returns>The enum value corresponding to the byte.</returns>
        /// <exception cref="ArgumentException">Thrown when no enum value matches the byte.</exception>
        public static T FromByteValue<T>(byte byteValue) where T : Enum
        {
            var type = typeof(T);

            // Check cache first
            if (_byteValueCache.TryGetValue(type, out var cache) && cache.TryGetValue(byteValue, out var cachedValue))
            {
                return (T)cachedValue;
            }

            // Initialize cache for this type if needed
            if (!_byteValueCache.ContainsKey(type))
            {
                _byteValueCache[type] = new Dictionary<byte, object>();
            }

            var allValues = GetAllValues<T>();

            foreach (var value in allValues)
            {
                byte enumByteValue;

                // Check if it implements IByteEnum
                if (value is IByteEnum byteEnum)
                {
                    enumByteValue = byteEnum.ByteValue;
                }
                else
                {
                    // Convert enum to byte
                    enumByteValue = Convert.ToByte(value);
                }

                // Cache the value
                _byteValueCache[type][enumByteValue] = value;

                if (enumByteValue == byteValue)
                {
                    return value;
                }
            }

            throw new ArgumentException($"No {type.Name} value found for byte value {byteValue}", nameof(byteValue));
        }

        /// <summary>
        /// Tries to get an enum value from its byte representation.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="byteValue">The byte value.</param>
        /// <param name="result">The enum value if found.</param>
        /// <returns>True if a value was found, false otherwise.</returns>
        public static bool TryFromByteValue<T>(byte byteValue, out T result) where T : Enum
        {
            try
            {
                result = FromByteValue<T>(byteValue);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Gets the byte representation of an enum value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The byte representation.</returns>
        public static byte GetByteValue(this Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Check if it implements IByteEnum
            if (value is IByteEnum byteEnum)
            {
                return byteEnum.ByteValue;
            }

            // Convert enum to byte
            return Convert.ToByte(value);
        }

        /// <summary>
        /// Gets an enum value from its JSON string representation.
        /// This method works with enums that implement IByteEnum or use DescriptionAttribute.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="jsonValue">The JSON string value.</param>
        /// <returns>The enum value corresponding to the JSON string.</returns>
        /// <exception cref="ArgumentException">Thrown when no enum value matches the JSON string.</exception>
        public static T FromJsonValue<T>(string jsonValue) where T : Enum
        {
            if (string.IsNullOrEmpty(jsonValue))
                throw new ArgumentException("JSON value cannot be null or empty", nameof(jsonValue));

            var type = typeof(T);

            // Check cache first
            if (_jsonValueCache.TryGetValue(type, out var cache) && cache.TryGetValue(jsonValue, out var cachedValue))
            {
                return (T)cachedValue;
            }

            // Initialize cache for this type if needed
            if (!_jsonValueCache.ContainsKey(type))
            {
                _jsonValueCache[type] = new Dictionary<string, object>();
            }

            var allValues = GetAllValues<T>();

            foreach (var value in allValues)
            {
                string enumJsonValue;

                // Check if it implements IByteEnum
                if (value is IByteEnum byteEnum)
                {
                    enumJsonValue = byteEnum.JsonValue;
                }
                else
                {
                    // Use description attribute or enum name
                    enumJsonValue = value.GetDescription();
                }

                // Cache the value
                _jsonValueCache[type][enumJsonValue] = value;

                if (string.Equals(enumJsonValue, jsonValue, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            // Try case-sensitive match with enum names
            if (Enum.TryParse<T>(jsonValue, false, out var exactMatch))
            {
                _jsonValueCache[type][jsonValue] = exactMatch;
                return exactMatch;
            }

            // Try case-insensitive match with enum names
            if (Enum.TryParse<T>(jsonValue, true, out var caseInsensitiveMatch))
            {
                _jsonValueCache[type][jsonValue] = caseInsensitiveMatch;
                return caseInsensitiveMatch;
            }

            throw new ArgumentException($"No {type.Name} value found for JSON value '{jsonValue}'", nameof(jsonValue));
        }

        /// <summary>
        /// Tries to get an enum value from its JSON string representation.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="jsonValue">The JSON string value.</param>
        /// <param name="result">The enum value if found.</param>
        /// <returns>True if a value was found, false otherwise.</returns>
        public static bool TryFromJsonValue<T>(string jsonValue, out T result) where T : Enum
        {
            try
            {
                result = FromJsonValue<T>(jsonValue);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Gets the JSON string representation of an enum value.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The JSON string representation.</returns>
        public static string GetJsonValue(this Enum value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Check if it implements IByteEnum
            if (value is IByteEnum byteEnum)
            {
                return byteEnum.JsonValue;
            }

            // Use description attribute or enum name
            return value.GetDescription();
        }

        /// <summary>
        /// Checks if an enum has a specific flag set (for flag enums).
        /// </summary>
        /// <param name="value">The enum value to check.</param>
        /// <param name="flag">The flag to check for.</param>
        /// <returns>True if the flag is set, false otherwise.</returns>
        public static bool HasFlag(this Enum value, Enum flag)
        {
            if (value == null || flag == null)
                return false;

            var valueInt = Convert.ToInt64(value);
            var flagInt = Convert.ToInt64(flag);

            return (valueInt & flagInt) == flagInt;
        }

        /// <summary>
        /// Adds a flag to an enum value (for flag enums).
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value.</param>
        /// <param name="flag">The flag to add.</param>
        /// <returns>The enum value with the flag added.</returns>
        public static T AddFlag<T>(this T value, T flag) where T : Enum
        {
            var valueInt = Convert.ToInt64(value);
            var flagInt = Convert.ToInt64(flag);
            var result = valueInt | flagInt;

            return (T)Enum.ToObject(typeof(T), result);
        }

        /// <summary>
        /// Removes a flag from an enum value (for flag enums).
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value.</param>
        /// <param name="flag">The flag to remove.</param>
        /// <returns>The enum value with the flag removed.</returns>
        public static T RemoveFlag<T>(this T value, T flag) where T : Enum
        {
            var valueInt = Convert.ToInt64(value);
            var flagInt = Convert.ToInt64(flag);
            var result = valueInt & ~flagInt;

            return (T)Enum.ToObject(typeof(T), result);
        }

        /// <summary>
        /// Toggles a flag in an enum value (for flag enums).
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value.</param>
        /// <param name="flag">The flag to toggle.</param>
        /// <returns>The enum value with the flag toggled.</returns>
        public static T ToggleFlag<T>(this T value, T flag) where T : Enum
        {
            return value.HasFlag(flag) ? value.RemoveFlag(flag) : value.AddFlag(flag);
        }

        /// <summary>
        /// Gets all flags that are set in an enum value (for flag enums).
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value.</param>
        /// <returns>An enumerable of all set flags.</returns>
        public static IEnumerable<T> GetSetFlags<T>(this T value) where T : Enum
        {
            var allValues = GetAllValues<T>();
            return allValues.Where(flag => value.HasFlag(flag) && Convert.ToInt64(flag) != 0);
        }

        /// <summary>
        /// Determines if an enum value is defined (valid).
        /// </summary>
        /// <param name="value">The enum value to check.</param>
        /// <returns>True if the value is defined, false otherwise.</returns>
        public static bool IsDefined(this Enum value)
        {
            if (value == null)
                return false;

            return Enum.IsDefined(value.GetType(), value);
        }
    }

    /// <summary>
    /// JSON converter for enums that implement IByteEnum.
    /// Handles both string and numeric representations.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public class ByteEnumJsonConverter<T> : JsonConverter<T> where T : Enum, IByteEnum
    {
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(value.JsonValue);
            }
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return default;

            return reader.Value switch
            {
                string stringValue => EnumExtensions.FromJsonValue<T>(stringValue),
                long longValue => EnumExtensions.FromByteValue<T>((byte)longValue),
                int intValue => EnumExtensions.FromByteValue<T>((byte)intValue),
                byte byteValue => EnumExtensions.FromByteValue<T>(byteValue),
                _ => throw new JsonException($"Cannot convert {reader.Value.GetType().Name} to {typeof(T).Name}")
            };
        }
    }
}