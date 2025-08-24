using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using NeoUnity.Runtime.Serialization;

namespace NeoUnity.Runtime.Types
{
    /// <summary>
    /// Enumeration of Neo blockchain roles for governance and consensus.
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role : byte
    {
        /// <summary>
        /// State validator role for state root validation.
        /// </summary>
        [Description("StateValidator")]
        StateValidator = 0x04,

        /// <summary>
        /// Oracle role for external data fetching.
        /// </summary>
        [Description("Oracle")]
        Oracle = 0x08,

        /// <summary>
        /// NeoFS Alphabet Node role for distributed storage.
        /// </summary>
        [Description("NeoFSAlphabetNode")]
        NeoFSAlphabetNode = 0x10
    }

    /// <summary>
    /// Extension methods for Role enum.
    /// </summary>
    public static class RoleExtensions
    {
        /// <summary>
        /// Gets the JSON string representation of the role.
        /// </summary>
        /// <param name="role">The role</param>
        /// <returns>JSON string value</returns>
        public static string ToJsonString(this Role role)
        {
            return role switch
            {
                Role.StateValidator => "StateValidator",
                Role.Oracle => "Oracle",
                Role.NeoFSAlphabetNode => "NeoFSAlphabetNode",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid role value")
            };
        }

        /// <summary>
        /// Gets the byte value of the role.
        /// </summary>
        /// <param name="role">The role</param>
        /// <returns>Byte value</returns>
        public static byte ToByte(this Role role)
        {
            return (byte)role;
        }

        /// <summary>
        /// Creates a Role from byte value.
        /// </summary>
        /// <param name="value">The byte value</param>
        /// <returns>The role</returns>
        /// <exception cref="ArgumentException">If value is not a valid role</exception>
        public static Role FromByte(byte value)
        {
            return value switch
            {
                0x04 => Role.StateValidator,
                0x08 => Role.Oracle,
                0x10 => Role.NeoFSAlphabetNode,
                _ => throw new ArgumentException($"Invalid role byte value: 0x{value:X2}", nameof(value))
            };
        }

        /// <summary>
        /// Creates a Role from string value.
        /// </summary>
        /// <param name="value">The string value</param>
        /// <returns>The role</returns>
        /// <exception cref="ArgumentException">If value is not a valid role</exception>
        public static Role FromString(string value)
        {
            return value switch
            {
                "StateValidator" => Role.StateValidator,
                "Oracle" => Role.Oracle,
                "NeoFSAlphabetNode" => Role.NeoFSAlphabetNode,
                _ => throw new ArgumentException($"Invalid role string value: {value}", nameof(value))
            };
        }

        /// <summary>
        /// Validates if the role is a valid Neo blockchain role.
        /// </summary>
        /// <param name="role">The role to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(this Role role)
        {
            return role == Role.StateValidator ||
                   role == Role.Oracle ||
                   role == Role.NeoFSAlphabetNode;
        }

        /// <summary>
        /// Gets a description of what the role does.
        /// </summary>
        /// <param name="role">The role</param>
        /// <returns>Description string</returns>
        public static string GetDescription(this Role role)
        {
            return role switch
            {
                Role.StateValidator => "Validates state roots and provides consensus for state changes",
                Role.Oracle => "Fetches external data and provides it to smart contracts",
                Role.NeoFSAlphabetNode => "Participates in NeoFS distributed storage network",
                _ => "Unknown role"
            };
        }
    }
}