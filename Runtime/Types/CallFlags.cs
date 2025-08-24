using System;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents the call flags that determine what operations a contract invocation can perform.
    /// These flags control the permissions for smart contract execution.
    /// </summary>
    [Flags]
    [Serializable]
    public enum CallFlags : byte
    {
        /// <summary>
        /// No permissions granted.
        /// </summary>
        [Description("None")]
        None = 0,

        /// <summary>
        /// Allows reading from storage.
        /// </summary>
        [Description("ReadStates")]
        ReadStates = 0b00000001,

        /// <summary>
        /// Allows writing to storage.
        /// </summary>
        [Description("WriteStates")]
        WriteStates = 0b00000010,

        /// <summary>
        /// Allows calling other contracts.
        /// </summary>
        [Description("AllowCall")]
        AllowCall = 0b00000100,

        /// <summary>
        /// Allows sending notifications.
        /// </summary>
        [Description("AllowNotify")]
        AllowNotify = 0b00001000,

        /// <summary>
        /// Combination of ReadStates and WriteStates.
        /// </summary>
        [Description("States")]
        States = ReadStates | WriteStates,

        /// <summary>
        /// Combination of ReadStates and AllowCall (read-only operations).
        /// </summary>
        [Description("ReadOnly")]
        ReadOnly = ReadStates | AllowCall,

        /// <summary>
        /// All permissions granted.
        /// </summary>
        [Description("All")]
        All = States | AllowCall | AllowNotify
    }

    /// <summary>
    /// Extension methods for CallFlags enum operations.
    /// </summary>
    public static class CallFlagsExtensions
    {
        /// <summary>
        /// Gets the byte value of the call flags.
        /// </summary>
        /// <param name="flags">The call flags.</param>
        /// <returns>The byte representation.</returns>
        public static byte GetValue(this CallFlags flags)
        {
            return (byte)flags;
        }

        /// <summary>
        /// Creates CallFlags from a byte value.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <returns>The corresponding CallFlags.</returns>
        /// <exception cref="ArgumentException">Thrown when the byte value doesn't correspond to valid call flags.</exception>
        public static CallFlags FromValue(byte value)
        {
            // Validate that the value represents valid flag combinations
            if (!Enum.IsDefined(typeof(CallFlags), value) && !IsValidFlagsCombination(value))
            {
                throw new ArgumentException($"Invalid call flags value: {value}", nameof(value));
            }
            
            return (CallFlags)value;
        }

        /// <summary>
        /// Determines if a byte value represents a valid combination of call flags.
        /// </summary>
        /// <param name="value">The byte value to validate.</param>
        /// <returns>True if the value is a valid flags combination, false otherwise.</returns>
        private static bool IsValidFlagsCombination(byte value)
        {
            // Check if value is within the valid range (0-15 for 4 bits)
            return value <= (byte)CallFlags.All;
        }

        /// <summary>
        /// Checks if the call flags include the specified permission.
        /// </summary>
        /// <param name="flags">The call flags.</param>
        /// <param name="permission">The permission to check.</param>
        /// <returns>True if the permission is included, false otherwise.</returns>
        public static bool HasPermission(this CallFlags flags, CallFlags permission)
        {
            return (flags & permission) == permission;
        }

        /// <summary>
        /// Gets a human-readable description of the call flags.
        /// </summary>
        /// <param name="flags">The call flags.</param>
        /// <returns>A descriptive string.</returns>
        public static string GetDescription(this CallFlags flags)
        {
            return flags switch
            {
                CallFlags.None => "No permissions",
                CallFlags.ReadStates => "Read states only",
                CallFlags.WriteStates => "Write states only",
                CallFlags.AllowCall => "Allow contract calls only",
                CallFlags.AllowNotify => "Allow notifications only",
                CallFlags.States => "Read and write states",
                CallFlags.ReadOnly => "Read states and call contracts",
                CallFlags.All => "All permissions",
                _ => GetCombinedDescription(flags)
            };
        }

        /// <summary>
        /// Gets a description for combined flags that don't match standard combinations.
        /// </summary>
        /// <param name="flags">The call flags.</param>
        /// <returns>A descriptive string for the combination.</returns>
        private static string GetCombinedDescription(CallFlags flags)
        {
            var permissions = new System.Collections.Generic.List<string>();
            
            if (flags.HasPermission(CallFlags.ReadStates))
                permissions.Add("Read states");
            if (flags.HasPermission(CallFlags.WriteStates))
                permissions.Add("Write states");
            if (flags.HasPermission(CallFlags.AllowCall))
                permissions.Add("Allow calls");
            if (flags.HasPermission(CallFlags.AllowNotify))
                permissions.Add("Allow notifications");

            return permissions.Count > 0 ? string.Join(", ", permissions) : "Unknown combination";
        }

        /// <summary>
        /// Checks if the flags represent read-only permissions.
        /// </summary>
        /// <param name="flags">The call flags.</param>
        /// <returns>True if the flags are read-only, false otherwise.</returns>
        public static bool IsReadOnly(this CallFlags flags)
        {
            return !flags.HasPermission(CallFlags.WriteStates);
        }

        /// <summary>
        /// Checks if the flags allow any storage operations.
        /// </summary>
        /// <param name="flags">The call flags.</param>
        /// <returns>True if storage operations are allowed, false otherwise.</returns>
        public static bool AllowsStorageOperations(this CallFlags flags)
        {
            return flags.HasPermission(CallFlags.ReadStates) || flags.HasPermission(CallFlags.WriteStates);
        }
    }

    /// <summary>
    /// JSON converter for CallFlags.
    /// </summary>
    public class CallFlagsJsonConverter : JsonConverter<CallFlags>
    {
        public override void WriteJson(JsonWriter writer, CallFlags value, JsonSerializer serializer)
        {
            writer.WriteValue((byte)value);
        }

        public override CallFlags ReadJson(JsonReader reader, Type objectType, CallFlags existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return CallFlags.None;

            return reader.Value switch
            {
                long longValue => CallFlagsExtensions.FromValue((byte)longValue),
                int intValue => CallFlagsExtensions.FromValue((byte)intValue),
                byte byteValue => CallFlagsExtensions.FromValue(byteValue),
                _ => CallFlags.None
            };
        }
    }
}