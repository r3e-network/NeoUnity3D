using System;
using Newtonsoft.Json;
using UnityEngine;
using Neo.Unity.SDK.Serialization;

namespace Neo.Unity.SDK.Transaction
{
    /// <summary>
    /// Represents a witness rule that combines an action with a condition.
    /// Witness rules determine how witnesses can be used based on specific conditions.
    /// </summary>
    [Serializable]
    public struct WitnessRule : INeoSerializable, IEquatable<WitnessRule>
    {
        [SerializeField]
        private WitnessAction _action;
        
        [SerializeField]
        private WitnessCondition _condition;

        /// <summary>
        /// Gets the action to be taken when the condition is met.
        /// </summary>
        public WitnessAction Action => _action;

        /// <summary>
        /// Gets the condition that must be evaluated.
        /// </summary>
        public WitnessCondition Condition => _condition;

        /// <summary>
        /// Gets the size of this witness rule when serialized.
        /// </summary>
        public int Size => 1 + _condition?.Size ?? 0;

        /// <summary>
        /// Initializes a new instance of the WitnessRule struct.
        /// </summary>
        /// <param name="action">The action to take when the condition is met.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <exception cref="ArgumentNullException">Thrown when condition is null.</exception>
        public WitnessRule(WitnessAction action, WitnessCondition condition)
        {
            _action = action;
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>
        /// Creates a witness rule that allows access when the condition is met.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>A new witness rule with Allow action.</returns>
        public static WitnessRule Allow(WitnessCondition condition)
        {
            return new WitnessRule(WitnessAction.Allow, condition);
        }

        /// <summary>
        /// Creates a witness rule that denies access when the condition is met.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>A new witness rule with Deny action.</returns>
        public static WitnessRule Deny(WitnessCondition condition)
        {
            return new WitnessRule(WitnessAction.Deny, condition);
        }

        /// <summary>
        /// Creates a witness rule that allows access when called by entry.
        /// </summary>
        /// <returns>A new witness rule for CalledByEntry condition.</returns>
        public static WitnessRule AllowCalledByEntry()
        {
            return Allow(new CalledByEntryCondition());
        }

        /// <summary>
        /// Creates a witness rule that denies access when called by entry.
        /// </summary>
        /// <returns>A new witness rule for CalledByEntry condition.</returns>
        public static WitnessRule DenyCalledByEntry()
        {
            return Deny(new CalledByEntryCondition());
        }

        /// <summary>
        /// Creates a witness rule that allows access for a specific script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash to allow.</param>
        /// <returns>A new witness rule for the specified script hash.</returns>
        public static WitnessRule AllowScriptHash(Hash160 scriptHash)
        {
            return Allow(new ScriptHashCondition(scriptHash));
        }

        /// <summary>
        /// Creates a witness rule that denies access for a specific script hash.
        /// </summary>
        /// <param name="scriptHash">The script hash to deny.</param>
        /// <returns>A new witness rule for the specified script hash.</returns>
        public static WitnessRule DenyScriptHash(Hash160 scriptHash)
        {
            return Deny(new ScriptHashCondition(scriptHash));
        }

        /// <summary>
        /// Creates a witness rule that allows access for a specific group.
        /// </summary>
        /// <param name="group">The public key representing the group to allow.</param>
        /// <returns>A new witness rule for the specified group.</returns>
        public static WitnessRule AllowGroup(ECPublicKey group)
        {
            return Allow(new GroupCondition(group));
        }

        /// <summary>
        /// Creates a witness rule that denies access for a specific group.
        /// </summary>
        /// <param name="group">The public key representing the group to deny.</param>
        /// <returns>A new witness rule for the specified group.</returns>
        public static WitnessRule DenyGroup(ECPublicKey group)
        {
            return Deny(new GroupCondition(group));
        }

        /// <summary>
        /// Serializes this witness rule to the specified writer.
        /// </summary>
        /// <param name="writer">The binary writer.</param>
        public void Serialize(BinaryWriter writer)
        {
            writer.WriteUInt8(_action.GetByteValue());
            _condition.Serialize(writer);
        }

        /// <summary>
        /// Deserializes a witness rule from the specified reader.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        /// <returns>The deserialized witness rule.</returns>
        public static WitnessRule Deserialize(BinaryReader reader)
        {
            var actionByte = reader.ReadByte();
            var action = WitnessActionExtensions.FromByteValue(actionByte);
            var condition = WitnessCondition.Deserialize(reader);
            return new WitnessRule(action, condition);
        }

        /// <summary>
        /// Determines if this witness rule is semantically equivalent to another.
        /// </summary>
        /// <param name="other">The other witness rule to compare.</param>
        /// <returns>True if the rules are equivalent, false otherwise.</returns>
        public bool Equals(WitnessRule other)
        {
            return _action == other._action && 
                   (_condition?.Equals(other._condition) ?? other._condition == null);
        }

        /// <summary>
        /// Determines if this witness rule is equivalent to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if the objects are equivalent, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is WitnessRule other && Equals(other);
        }

        /// <summary>
        /// Gets the hash code for this witness rule.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_action, _condition);
        }

        /// <summary>
        /// Gets a string representation of this witness rule.
        /// </summary>
        /// <returns>A string describing the rule.</returns>
        public override string ToString()
        {
            return $"{_action} when {_condition?.JsonValue ?? "null"}";
        }

        /// <summary>
        /// Validates that this witness rule is well-formed.
        /// </summary>
        /// <returns>True if the rule is valid, false otherwise.</returns>
        public bool IsValid()
        {
            return _condition != null && Enum.IsDefined(typeof(WitnessAction), _action);
        }

        /// <summary>
        /// Gets a human-readable description of what this rule does.
        /// </summary>
        /// <returns>A descriptive string explaining the rule's behavior.</returns>
        public string GetDescription()
        {
            if (_condition == null)
                return "Invalid rule (no condition)";

            var actionText = _action == WitnessAction.Allow ? "allow" : "deny";
            var conditionText = _condition.JsonValue.ToLowerInvariant();
            
            return $"Will {actionText} witness usage when condition '{conditionText}' is met";
        }

        public static bool operator ==(WitnessRule left, WitnessRule right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WitnessRule left, WitnessRule right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// JSON converter for WitnessRule.
    /// </summary>
    public class WitnessRuleJsonConverter : JsonConverter<WitnessRule>
    {
        public override void WriteJson(JsonWriter writer, WitnessRule value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("action");
            writer.WriteValue(value.Action.GetJsonValue());
            writer.WritePropertyName("condition");
            serializer.Serialize(writer, value.Condition);
            writer.WriteEndObject();
        }

        public override WitnessRule ReadJson(JsonReader reader, Type objectType, WitnessRule existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonException("Expected start of object for WitnessRule");

            WitnessAction? action = null;
            WitnessCondition condition = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value.ToString();
                    reader.Read();

                    switch (propertyName?.ToLowerInvariant())
                    {
                        case "action":
                            if (reader.Value is string actionStr)
                                action = WitnessActionExtensions.FromJsonValue(actionStr);
                            break;
                        case "condition":
                            condition = serializer.Deserialize<WitnessCondition>(reader);
                            break;
                    }
                }
            }

            if (!action.HasValue)
                throw new JsonException("Missing 'action' property in WitnessRule");
            if (condition == null)
                throw new JsonException("Missing 'condition' property in WitnessRule");

            return new WitnessRule(action.Value, condition);
        }
    }
}