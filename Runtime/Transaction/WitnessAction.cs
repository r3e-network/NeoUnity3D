using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// Represents the action to be taken when a witness rule condition is met.
    /// </summary>
    [Serializable]
    public enum WitnessAction : byte
    {
        /// <summary>
        /// Deny the witness if the condition is met.
        /// </summary>
        [Description("Deny")]
        Deny = 0,

        /// <summary>
        /// Allow the witness if the condition is met.
        /// </summary>
        [Description("Allow")]
        Allow = 1
    }

    /// <summary>
    /// Extension methods for WitnessAction enum operations.
    /// </summary>
    public static class WitnessActionExtensions
    {
        /// <summary>
        /// Gets the JSON string representation of the witness action.
        /// </summary>
        /// <param name="action">The witness action.</param>
        /// <returns>The JSON-compatible string representation.</returns>
        public static string GetJsonValue(this WitnessAction action)
        {
            return action switch
            {
                WitnessAction.Deny => "Deny",
                WitnessAction.Allow => "Allow",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Invalid witness action")
            };
        }

        /// <summary>
        /// Gets the byte value of the witness action.
        /// </summary>
        /// <param name="action">The witness action.</param>
        /// <returns>The byte representation.</returns>
        public static byte GetByteValue(this WitnessAction action)
        {
            return (byte)action;
        }

        /// <summary>
        /// Parses a witness action from its JSON string representation.
        /// </summary>
        /// <param name="jsonValue">The JSON string value.</param>
        /// <returns>The corresponding witness action.</returns>
        /// <exception cref="ArgumentException">Thrown when the JSON value is invalid.</exception>
        public static WitnessAction FromJsonValue(string jsonValue)
        {
            if (string.IsNullOrEmpty(jsonValue))
                throw new ArgumentException("JSON value cannot be null or empty", nameof(jsonValue));

            return jsonValue switch
            {
                "Deny" => WitnessAction.Deny,
                "Allow" => WitnessAction.Allow,
                _ => throw new ArgumentException($"Invalid witness action JSON value: {jsonValue}", nameof(jsonValue))
            };
        }

        /// <summary>
        /// Creates a WitnessAction from a byte value.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <returns>The corresponding WitnessAction.</returns>
        /// <exception cref="ArgumentException">Thrown when the byte value is invalid.</exception>
        public static WitnessAction FromByteValue(byte value)
        {
            return value switch
            {
                0 => WitnessAction.Deny,
                1 => WitnessAction.Allow,
                _ => throw new ArgumentException($"Invalid witness action byte value: {value}", nameof(value))
            };
        }

        /// <summary>
        /// Determines if the action allows the witness.
        /// </summary>
        /// <param name="action">The witness action.</param>
        /// <returns>True if the action allows the witness, false otherwise.</returns>
        public static bool IsAllow(this WitnessAction action)
        {
            return action == WitnessAction.Allow;
        }

        /// <summary>
        /// Determines if the action denies the witness.
        /// </summary>
        /// <param name="action">The witness action.</param>
        /// <returns>True if the action denies the witness, false otherwise.</returns>
        public static bool IsDeny(this WitnessAction action)
        {
            return action == WitnessAction.Deny;
        }

        /// <summary>
        /// Gets a human-readable description of the witness action.
        /// </summary>
        /// <param name="action">The witness action.</param>
        /// <returns>A descriptive string.</returns>
        public static string GetDescription(this WitnessAction action)
        {
            return action switch
            {
                WitnessAction.Deny => "Deny witness usage when condition is met",
                WitnessAction.Allow => "Allow witness usage when condition is met",
                _ => "Unknown witness action"
            };
        }
    }

    /// <summary>
    /// JSON converter for WitnessAction.
    /// </summary>
    public class WitnessActionJsonConverter : JsonConverter<WitnessAction>
    {
        public override void WriteJson(JsonWriter writer, WitnessAction value, JsonSerializer serializer)
        {
            writer.WriteValue(value.GetJsonValue());
        }

        public override WitnessAction ReadJson(JsonReader reader, Type objectType, WitnessAction existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value switch
            {
                string stringValue => WitnessActionExtensions.FromJsonValue(stringValue),
                long longValue => WitnessActionExtensions.FromByteValue((byte)longValue),
                int intValue => WitnessActionExtensions.FromByteValue((byte)intValue),
                byte byteValue => WitnessActionExtensions.FromByteValue(byteValue),
                _ => throw new JsonException($"Cannot convert {reader.Value?.GetType().Name ?? "null"} to WitnessAction")
            };
        }
    }
}