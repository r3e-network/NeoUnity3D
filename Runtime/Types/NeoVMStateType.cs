using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents the execution state of the Neo Virtual Machine.
    /// Corresponds to the VM state values used in Neo blockchain operations.
    /// </summary>
    [Serializable]
    public enum NeoVMStateType : byte
    {
        /// <summary>
        /// No state or uninitialized state.
        /// </summary>
        [Description("NONE")]
        None = 0,

        /// <summary>
        /// Execution completed successfully.
        /// </summary>
        [Description("HALT")]
        Halt = 1,

        /// <summary>
        /// Execution encountered a fault or error.
        /// </summary>
        [Description("FAULT")]
        Fault = 2, // 1 << 1

        /// <summary>
        /// Execution was interrupted or paused.
        /// </summary>
        [Description("BREAK")]
        Break = 4  // 1 << 2
    }

    /// <summary>
    /// Extension methods for NeoVMStateType enum operations.
    /// </summary>
    public static class NeoVMStateTypeExtensions
    {
        /// <summary>
        /// Gets the JSON string representation of the VM state.
        /// </summary>
        /// <param name="state">The VM state.</param>
        /// <returns>The JSON-compatible string representation.</returns>
        public static string GetJsonValue(this NeoVMStateType state)
        {
            return state switch
            {
                NeoVMStateType.None => "NONE",
                NeoVMStateType.Halt => "HALT",
                NeoVMStateType.Fault => "FAULT",
                NeoVMStateType.Break => "BREAK",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Invalid VM state type")
            };
        }

        /// <summary>
        /// Gets the integer representation of the VM state.
        /// </summary>
        /// <param name="state">The VM state.</param>
        /// <returns>The integer value of the state.</returns>
        public static int GetIntValue(this NeoVMStateType state)
        {
            return (int)state;
        }

        /// <summary>
        /// Parses a VM state from its JSON string representation.
        /// </summary>
        /// <param name="jsonValue">The JSON string value.</param>
        /// <returns>The corresponding VM state, or None if parsing fails.</returns>
        public static NeoVMStateType FromJsonValue(string jsonValue)
        {
            if (string.IsNullOrEmpty(jsonValue))
                return NeoVMStateType.None;

            return jsonValue.ToUpperInvariant() switch
            {
                "NONE" => NeoVMStateType.None,
                "HALT" => NeoVMStateType.Halt,
                "FAULT" => NeoVMStateType.Fault,
                "BREAK" => NeoVMStateType.Break,
                _ => NeoVMStateType.None
            };
        }

        /// <summary>
        /// Parses a VM state from its integer representation.
        /// </summary>
        /// <param name="intValue">The integer value.</param>
        /// <returns>The corresponding VM state, or None if parsing fails.</returns>
        public static NeoVMStateType FromIntValue(int intValue)
        {
            return intValue switch
            {
                0 => NeoVMStateType.None,
                1 => NeoVMStateType.Halt,
                2 => NeoVMStateType.Fault,
                4 => NeoVMStateType.Break,
                _ => NeoVMStateType.None
            };
        }

        /// <summary>
        /// Determines if the VM state represents a successful execution.
        /// </summary>
        /// <param name="state">The VM state.</param>
        /// <returns>True if the state represents success, false otherwise.</returns>
        public static bool IsSuccess(this NeoVMStateType state)
        {
            return state == NeoVMStateType.Halt;
        }

        /// <summary>
        /// Determines if the VM state represents an error condition.
        /// </summary>
        /// <param name="state">The VM state.</param>
        /// <returns>True if the state represents an error, false otherwise.</returns>
        public static bool IsError(this NeoVMStateType state)
        {
            return state == NeoVMStateType.Fault;
        }
    }

    /// <summary>
    /// JSON converter for NeoVMStateType that handles both string and integer representations.
    /// </summary>
    public class NeoVMStateTypeJsonConverter : JsonConverter<NeoVMStateType>
    {
        public override void WriteJson(JsonWriter writer, NeoVMStateType value, JsonSerializer serializer)
        {
            writer.WriteValue(value.GetJsonValue());
        }

        public override NeoVMStateType ReadJson(JsonReader reader, Type objectType, NeoVMStateType existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return NeoVMStateType.None;

            return reader.Value switch
            {
                string stringValue => NeoVMStateTypeExtensions.FromJsonValue(stringValue),
                long longValue => NeoVMStateTypeExtensions.FromIntValue((int)longValue),
                int intValue => NeoVMStateTypeExtensions.FromIntValue(intValue),
                _ => NeoVMStateType.None
            };
        }
    }
}