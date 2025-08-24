using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Neo.Unity.SDK.Types
{
    /// <summary>
    /// Represents the scope of a witness in a Neo transaction.
    /// Defines how and where a witness can be used within the blockchain.
    /// </summary>
    [Flags]
    [Serializable]
    public enum WitnessScope : byte
    {
        /// <summary>
        /// A witness with this scope is only used for transactions and is disabled in contracts.
        /// </summary>
        [Description("None")]
        None = 0x00,

        /// <summary>
        /// This scope limits the use of a witness to the level of the contract called in the transaction.
        /// It only allows the invoked contract to use the witness.
        /// </summary>
        [Description("CalledByEntry")]
        CalledByEntry = 0x01,

        /// <summary>
        /// This scope allows the specification of additional contracts in which the witness can be used.
        /// </summary>
        [Description("CustomContracts")]
        CustomContracts = 0x10,

        /// <summary>
        /// This scope allows the specification of contract groups in which the witness can be used.
        /// </summary>
        [Description("CustomGroups")]
        CustomGroups = 0x20,

        /// <summary>
        /// Indicates that the current context must satisfy the specified rules.
        /// </summary>
        [Description("WitnessRules")]
        WitnessRules = 0x40,

        /// <summary>
        /// The global scope allows to use a witness in all contexts.
        /// It cannot be combined with other scopes.
        /// </summary>
        [Description("Global")]
        Global = 0x80
    }

    /// <summary>
    /// Extension methods for WitnessScope enum operations.
    /// </summary>
    public static class WitnessScopeExtensions
    {
        /// <summary>
        /// Gets the JSON string representation of the witness scope.
        /// </summary>
        /// <param name="scope">The witness scope.</param>
        /// <returns>The JSON-compatible string representation.</returns>
        public static string GetJsonValue(this WitnessScope scope)
        {
            return scope switch
            {
                WitnessScope.None => "None",
                WitnessScope.CalledByEntry => "CalledByEntry",
                WitnessScope.CustomContracts => "CustomContracts",
                WitnessScope.CustomGroups => "CustomGroups",
                WitnessScope.WitnessRules => "WitnessRules",
                WitnessScope.Global => "Global",
                _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Invalid witness scope")
            };
        }

        /// <summary>
        /// Gets the byte value of the witness scope.
        /// </summary>
        /// <param name="scope">The witness scope.</param>
        /// <returns>The byte representation.</returns>
        public static byte GetByteValue(this WitnessScope scope)
        {
            return (byte)scope;
        }

        /// <summary>
        /// Parses a witness scope from its JSON string representation.
        /// </summary>
        /// <param name="jsonValue">The JSON string value.</param>
        /// <returns>The corresponding witness scope.</returns>
        /// <exception cref="ArgumentException">Thrown when the JSON value is invalid.</exception>
        public static WitnessScope FromJsonValue(string jsonValue)
        {
            if (string.IsNullOrEmpty(jsonValue))
                throw new ArgumentException("JSON value cannot be null or empty", nameof(jsonValue));

            return jsonValue switch
            {
                "None" => WitnessScope.None,
                "CalledByEntry" => WitnessScope.CalledByEntry,
                "CustomContracts" => WitnessScope.CustomContracts,
                "CustomGroups" => WitnessScope.CustomGroups,
                "WitnessRules" => WitnessScope.WitnessRules,
                "Global" => WitnessScope.Global,
                _ => throw new ArgumentException($"Invalid witness scope JSON value: {jsonValue}", nameof(jsonValue))
            };
        }

        /// <summary>
        /// Encodes the given scopes into one byte.
        /// </summary>
        /// <param name="scopes">The scopes to encode.</param>
        /// <returns>The scope encoding byte.</returns>
        public static byte CombineScopes(IEnumerable<WitnessScope> scopes)
        {
            if (scopes == null)
                return (byte)WitnessScope.None;

            return scopes.Aggregate((byte)0, (current, scope) => (byte)(current | scope.GetByteValue()));
        }

        /// <summary>
        /// Extracts the scopes encoded in the given byte.
        /// </summary>
        /// <param name="combinedScopes">The byte representation of the scopes.</param>
        /// <returns>The list of scopes encoded by the given byte.</returns>
        public static WitnessScope[] ExtractCombinedScopes(byte combinedScopes)
        {
            if (combinedScopes == (byte)WitnessScope.None)
                return new[] { WitnessScope.None };

            var result = new List<WitnessScope>();
            var allScopes = new[] 
            {
                WitnessScope.CalledByEntry,
                WitnessScope.CustomContracts,
                WitnessScope.CustomGroups,
                WitnessScope.WitnessRules,
                WitnessScope.Global
            };

            foreach (var scope in allScopes)
            {
                if ((combinedScopes & scope.GetByteValue()) != 0)
                {
                    result.Add(scope);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Determines if the scope allows global access.
        /// </summary>
        /// <param name="scope">The witness scope.</param>
        /// <returns>True if global access is allowed, false otherwise.</returns>
        public static bool IsGlobal(this WitnessScope scope)
        {
            return (scope & WitnessScope.Global) == WitnessScope.Global;
        }

        /// <summary>
        /// Determines if the scope requires custom contracts.
        /// </summary>
        /// <param name="scope">The witness scope.</param>
        /// <returns>True if custom contracts are required, false otherwise.</returns>
        public static bool RequiresCustomContracts(this WitnessScope scope)
        {
            return (scope & WitnessScope.CustomContracts) == WitnessScope.CustomContracts;
        }

        /// <summary>
        /// Determines if the scope requires custom groups.
        /// </summary>
        /// <param name="scope">The witness scope.</param>
        /// <returns>True if custom groups are required, false otherwise.</returns>
        public static bool RequiresCustomGroups(this WitnessScope scope)
        {
            return (scope & WitnessScope.CustomGroups) == WitnessScope.CustomGroups;
        }

        /// <summary>
        /// Determines if the scope requires witness rules.
        /// </summary>
        /// <param name="scope">The witness scope.</param>
        /// <returns>True if witness rules are required, false otherwise.</returns>
        public static bool RequiresWitnessRules(this WitnessScope scope)
        {
            return (scope & WitnessScope.WitnessRules) == WitnessScope.WitnessRules;
        }

        /// <summary>
        /// Validates that the witness scope combination is valid according to Neo rules.
        /// </summary>
        /// <param name="scope">The witness scope to validate.</param>
        /// <returns>True if the scope is valid, false otherwise.</returns>
        public static bool IsValidScope(this WitnessScope scope)
        {
            // Global scope cannot be combined with other scopes
            if (scope.IsGlobal() && scope != WitnessScope.Global)
                return false;

            // All individual scopes are valid
            return true;
        }
    }

    /// <summary>
    /// JSON converter for WitnessScope that handles both individual scopes and scope combinations.
    /// </summary>
    public class WitnessScopeJsonConverter : JsonConverter<WitnessScope>
    {
        public override void WriteJson(JsonWriter writer, WitnessScope value, JsonSerializer serializer)
        {
            if (Enum.IsDefined(typeof(WitnessScope), value) && !value.ToString().Contains(','))
            {
                // Single scope
                writer.WriteValue(value.GetJsonValue());
            }
            else
            {
                // Combined scopes - write as comma-separated string
                var scopes = WitnessScopeExtensions.ExtractCombinedScopes((byte)value);
                var jsonValues = scopes.Select(s => s.GetJsonValue());
                writer.WriteValue(string.Join(",", jsonValues));
            }
        }

        public override WitnessScope ReadJson(JsonReader reader, Type objectType, WitnessScope existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return WitnessScope.None;

            return reader.Value switch
            {
                string stringValue => ParseScopeString(stringValue),
                long longValue => (WitnessScope)(byte)longValue,
                int intValue => (WitnessScope)(byte)intValue,
                byte byteValue => (WitnessScope)byteValue,
                _ => WitnessScope.None
            };
        }

        private static WitnessScope ParseScopeString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return WitnessScope.None;

            // Handle comma-separated scopes
            var scopeStrings = value.Replace(" ", "").Split(',');
            if (scopeStrings.Length == 1)
            {
                return WitnessScopeExtensions.FromJsonValue(scopeStrings[0]);
            }

            // Combine multiple scopes
            var scopes = scopeStrings.Select(WitnessScopeExtensions.FromJsonValue);
            return (WitnessScope)WitnessScopeExtensions.CombineScopes(scopes);
        }
    }

    /// <summary>
    /// Utility class for handling witness scope collections with JSON serialization.
    /// </summary>
    [Serializable]
    public class WitnessScopeCollection
    {
        [SerializeField]
        private WitnessScope[] _scopes;

        /// <summary>
        /// Gets the witness scopes in this collection.
        /// </summary>
        public WitnessScope[] Scopes => _scopes ?? Array.Empty<WitnessScope>();

        /// <summary>
        /// Initializes a new instance of the WitnessScopeCollection class.
        /// </summary>
        /// <param name="scopes">The witness scopes.</param>
        public WitnessScopeCollection(IEnumerable<WitnessScope> scopes = null)
        {
            _scopes = scopes?.ToArray() ?? Array.Empty<WitnessScope>();
        }

        /// <summary>
        /// Gets the combined byte representation of all scopes.
        /// </summary>
        /// <returns>The combined scope byte value.</returns>
        public byte GetCombinedValue()
        {
            return WitnessScopeExtensions.CombineScopes(_scopes);
        }

        /// <summary>
        /// Creates a WitnessScopeCollection from a combined byte value.
        /// </summary>
        /// <param name="combinedValue">The combined scope byte value.</param>
        /// <returns>A new WitnessScopeCollection instance.</returns>
        public static WitnessScopeCollection FromCombinedValue(byte combinedValue)
        {
            var scopes = WitnessScopeExtensions.ExtractCombinedScopes(combinedValue);
            return new WitnessScopeCollection(scopes);
        }
    }
}